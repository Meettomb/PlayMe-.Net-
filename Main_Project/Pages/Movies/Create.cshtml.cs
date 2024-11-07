using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Main_Project.Models;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Netflix.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace Main_Project.Pages.Movies
{
    public class CreateModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;
        private IHostingEnvironment _environment;
        private readonly string _connectionString;

        public List<Movie_category_table> category = new List<Movie_category_table>();

        // List to store the selected categories from the dropdown
        [BindProperty]
        public List<string> SelectedCategories { get; set; } = new List<string>();
        [BindProperty]
        public List<IFormFile> UplodeMovies { get; set; }

        public CreateModel(Main_Project.Models.NetflixDataContext context, IHostingEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }

        public IActionResult OnGet()
        {
            
            string sessionEmail = HttpContext.Session.GetString("email");

            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
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
                                email = sessionEmail;
                            }
                        }
                        con.Close();
                    }
                }
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
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
                                moviecategory = dr.GetString(1)
                            };
                            category.Add(movie_category);
                        }
                        con.Close();
                    }
                }
            }

            UserName = HttpContext.Session.GetString("Username");
            return Page();
        }

        [BindProperty]
        public MoviesTable MoviesTable { get; set; } = new MoviesTable();

        public IFormFile uplode { get; set; }
        public IFormFile uplodetrailer { get; set; }
        public IFormFile uplodeposter2 { get; set; }

        // Property to handle multiple movie file uploads
        [BindProperty]
        public List<IFormFile> uplodemovie { get; set; } = new List<IFormFile>();

        public async Task<IActionResult> OnPostAsync()
        {
            // Handle file uploads for multiple movie files
            // Handle file uploads for multiple movie files
            if (UplodeMovies != null && UplodeMovies.Count > 0) // Use the correct property name
            {
                foreach (var movieFile in UplodeMovies)
                {
                    var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot/video", movieFile.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await movieFile.CopyToAsync(fileStream);
                    }

                    // Append the movie file name to the MoviesTable.Movie property (comma-separated)
                    MoviesTable.Movie += movieFile.FileName + ",";
                }

                // Remove the trailing comma after all file names are concatenated
                MoviesTable.Movie = MoviesTable.Movie.TrimEnd(',');
            }

            // Handle the poster and other single files as before
            var posterFile = Path.Combine(_environment.ContentRootPath, "wwwroot/poster", uplode?.FileName);
            using (var fileStream = new FileStream(posterFile, FileMode.Create))
            {
                await uplode.CopyToAsync(fileStream);
            }

            var posterFile2 = Path.Combine(_environment.ContentRootPath, "wwwroot/poster", uplodeposter2?.FileName);
            using (var fileStream = new FileStream(posterFile2, FileMode.Create))
            {
                await uplodeposter2.CopyToAsync(fileStream);
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

            // Set the posters in the MoviesTable model
            MoviesTable.Movieposter = uplode?.FileName;
            MoviesTable.Movieposter2 = uplodeposter2?.FileName;
            MoviesTable.Trailer = uplodetrailer?.FileName;

            // Join selected categories into a single string (comma-separated)
            MoviesTable.Movietype = string.Join(",", SelectedCategories);

            // Save the MoviesTable to the database
            _context.MoviesTables.Add(MoviesTable);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }



    }

}