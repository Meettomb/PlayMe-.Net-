using Main_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Netflix.Models;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace Main_Project.Pages.Movies
{
    public class Movie_CategoryModel : PageModel
    {

        private readonly NetflixDataContext _context;

        private readonly string _connectionString;

        public Movie_CategoryModel(NetflixDataContext context, IConfiguration configuration)
        {
            _context = context;

            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string UserRole { get; set; }
        public string Category { get; set; }
        public int id { get; set; }
        public List<MoviesTable> Movies { get; set; } = new List<MoviesTable>();
        public List<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();
        public List<Search_history> SearchHistory { get; set; } = new List<Search_history>();
        public List<MoviesTable> AllMovies { get; set; } = new List<MoviesTable>();



        public async Task<IActionResult> OnGetAsync(string category)
        {
            string sessionEmail = HttpContext.Session.GetString("email");
            int? userId = HttpContext.Session.GetInt32("Id");

            if (!userId.HasValue)
            {
                // If the user is not authenticated, redirect to home
                return RedirectToPage("/Index");
            }
            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                id = reader.GetInt32("id");
                                UserName = reader["username"].ToString();
                                profilepic = reader["profilepic"].ToString();
                                email = sessionEmail; // Set email from session

                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }

            int? sessionUserId = HttpContext.Session.GetInt32("Id");

            if (sessionUserId.HasValue) // Check if the user ID is present
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT TOP 5 * FROM Search_history WHERE userid = @UserId ORDER BY searchDateTime DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", sessionUserId.Value); // Use .Value to get the int
                        con.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var searchHistoryItem = new Search_history
                                {
                                    id = dr.GetInt32(0),
                                    userid = dr.IsDBNull(1) ? (int?)null : dr.GetInt32(1),
                                    searchtext = dr.IsDBNull(2) ? null : dr.GetString(2),
                                    searchDateTime = dr.GetDateTime(3)
                                };
                                this.SearchHistory.Add(searchHistoryItem);
                            }
                        }
                    }
                }
            }
            else
            {
                // Handle the case where user ID is not found in session
                // For example, redirect to login or show an error message
            }

            Category = category;

            // Fetch movie categories from the database
            await LoadMovieCategoriesAsync();

            // Load movies by category or all movies if no category is specified
            if (!string.IsNullOrEmpty(Category))
            {
                await LoadMoviesByCategoryAsync(Category);
                AllMovies = await _context.MoviesTables.ToListAsync(); // Fetch all movies if no category is specified
            }
            else
            {
            }

            return Page();
        }


        private async Task LoadMovieCategoriesAsync()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Movie_category_table";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Movie_category_table category = new Movie_category_table
                            {
                                categoryid = reader.GetInt32(0),
                                moviecategory = reader.GetString(1)
                            };
                            MovieCategories.Add(category);
                        }
                    }
                }
            }
        }

        private async Task LoadMoviesByCategoryAsync(string category)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Movies_table WHERE Movietype LIKE @category";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@category", "%" + category + "%");
                    con.Open();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            MoviesTable movie = new MoviesTable
                            {
                                Movieid = reader.GetInt32(0),
                                Moviename = reader.GetString(5),
                                Movietype = reader.GetString(7),
                                Movieposter2 = reader.GetString(3),
                            };
                            Movies.Add(movie);
                        }
                    }
                }
            }
        }


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostSaveMovieTypeAsync(string movietype, string movieid)
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            Console.WriteLine($"UserId from session: {userId}");

            if (userId.HasValue && !string.IsNullOrEmpty(movietype) && !string.IsNullOrEmpty(movieid))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        string query = "INSERT INTO User_activity (userid, movietype, DateTime) VALUES (@UserId, @MovieType, @DateTime)";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId.Value);
                            cmd.Parameters.AddWithValue("@MovieType", movietype); // Store the movietype
                            cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);

                            con.Open();
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // After successfully saving the data, redirect to Single_movie page with the movie ID
                    return RedirectToPage("/Single_movie", new { movieId = movieid }); // Redirect using movieId
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    // Handle the exception (logging, etc.) and redirect to an error page
                    return RedirectToPage("/Error"); // Change this to your error handling logic
                }
            }

            Console.WriteLine("Invalid data");
            return RedirectToPage("/Error"); // Redirect on invalid data
        }

    }
}