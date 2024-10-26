using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.Data.SqlClient;
using Netflix.Models;

namespace Main_Project.Pages.Movies
{
    public class EditModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;
        private IHostingEnvironment _environment;

        public List<Movie_category_table> category { get; set; } = new List<Movie_category_table>();

        [BindProperty]
        public List<string> SelectedCategories { get; set; }

        public EditModel(Main_Project.Models.NetflixDataContext context, IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public MoviesTable MoviesTable { get; set; } = default!;

        public IFormFile uploade { get; set; }
        public IFormFile uplodetrailer { get; set; }
        public List<IFormFile> uplodemovie { get; set; }
        public IFormFile uplodeposter2 { get; set; }
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }

        public string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Fetch user details from session
            string sessionEmail = HttpContext.Session.GetString("email");
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    string query = "SELECT username, dob, gender, profilepic FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                profilepic = reader["profilepic"].ToString();
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }

            // Fetch categories from Movie_category_table
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                string query = "SELECT * FROM Movie_category_table";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Movie_category_table movie_category = new Movie_category_table
                            {
                                categoryid = dr.GetInt32(0),
                                moviecategory = dr.GetString(1)
                            };
                            category.Add(movie_category);
                        }
                        con.Close();
                    }
                }
            }

            if (id == null)
            {
                return NotFound();
            }

            var moviestable = await _context.MoviesTables.FirstOrDefaultAsync(m => m.Movieid == id);
            if (moviestable == null)
            {
                return NotFound();
            }
            MoviesTable = moviestable;

            // Set SelectedCategories based on the movie's current categories
            if (!string.IsNullOrEmpty(MoviesTable.Movietype))
            {
                SelectedCategories = MoviesTable.Movietype.Split(',').ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if any movie files were uploaded
            if (uplodemovie != null && uplodemovie.Count > 0)
            {
                List<string> movieFiles = new List<string>(); // To store multiple movie filenames

                foreach (var movieFile in uplodemovie)
                {
                    if (movieFile.Length > 0) // Check if the file is valid
                    {
                        // Define the path where the file will be saved
                        var movieFilePath = Path.Combine(_environment.ContentRootPath, "wwwroot/video", movieFile.FileName);

                        // Save the file
                        using (var fileStream = new FileStream(movieFilePath, FileMode.Create))
                        {
                            await movieFile.CopyToAsync(fileStream); // Save each file
                        }

                        // Add the filename to the list (or store as a comma-separated string for multiple files)
                        movieFiles.Add(movieFile.FileName);
                    }
                }

                // Store the uploaded video filenames in the MoviesTable.Movie column (comma-separated if multiple)
                MoviesTable.Movie = string.Join(",", movieFiles);
            }

            // Upload the main poster
            if (uploade != null && uploade.Length > 0)
            {
                var posterFilePath = Path.Combine(_environment.ContentRootPath, "wwwroot/poster", uploade.FileName);
                using (var posterStream = new FileStream(posterFilePath, FileMode.Create))
                {
                    await uploade.CopyToAsync(posterStream);
                }
                MoviesTable.Movieposter = uploade.FileName; // Store poster filename
            }

            // Upload the second poster
            if (uplodeposter2 != null && uplodeposter2.Length > 0)
            {
                var poster2FilePath = Path.Combine(_environment.ContentRootPath, "wwwroot/poster", uplodeposter2.FileName);
                using (var poster2Stream = new FileStream(poster2FilePath, FileMode.Create))
                {
                    await uplodeposter2.CopyToAsync(poster2Stream);
                }
                MoviesTable.Movieposter2 = uplodeposter2.FileName; // Store second poster filename
            }

            // Upload the Trailer
            if (uplodetrailer != null && uplodetrailer.Length > 0)
            {
                var uplodetrailerFilePath = Path.Combine(_environment.ContentRootPath, "wwwroot/trailer", uplodetrailer.FileName);
                using (var TrailerStream = new FileStream(uplodetrailerFilePath, FileMode.Create))
                {
                    await uplodetrailer.CopyToAsync(TrailerStream);
                }
                MoviesTable.Trailer = uplodetrailer.FileName; // Store second poster filename
            }

            // Save selected categories as a comma-separated string
            if (SelectedCategories != null && SelectedCategories.Any())
            {
                MoviesTable.Movietype = string.Join(",", SelectedCategories);
            }

            // Update the MoviesTable record in the database
            _context.Attach(MoviesTable).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MoviesTableExists(MoviesTable.Movieid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }


        private bool MoviesTableExists(int id)
        {
            return _context.MoviesTables.Any(e => e.Movieid == id);
        }
    }
}
