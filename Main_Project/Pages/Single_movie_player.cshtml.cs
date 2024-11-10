using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Main_Project.Pages
{
    public class Single_movie_playerModel : PageModel
    {
        private readonly NetflixDataContext _context;
        private readonly ILogger<Single_movieModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;

        private readonly string  _connectionString;

        
        public Single_movie_playerModel(NetflixDataContext context, ILogger<Single_movieModel> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;

            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }



        [BindProperty]
        public Watch_history WatchHistory { get; set; }
        // Add the MovieFiles property
        public List<string> MovieFiles { get; set; } = new List<string>(); // Initializes as an empty list

        public MoviesTable Movie { get; set; }
        public IList<MoviesTable> SuggestedMovies { get; set; }
        public IList<MoviesTable> Allmovies { get; set; }
        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string id { get; set; }
        public string UserRole { get; set; }
        public string watchTime { get; set; }
        public List<Search_history> SearchHistory { get; set; } = new List<Search_history>();
        public IList<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();
        public async Task<IActionResult> OnGetAsync(int movieId)
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            if (!userId.HasValue)
            {
                return Redirect("/");
            }
            // Fetch search history from HttpContext
            if (HttpContext.Items["SearchHistory"] is List<Search_history> searchHistory)
            {
                SearchHistory = searchHistory;
            }
            else
            {
                SearchHistory = new List<Search_history>(); // Initialize as empty if no history found
            }


            profilepic = HttpContext.Session.GetString("profilepic");
            UserName = HttpContext.Session.GetString("Username");
            string sessionEmail = HttpContext.Session.GetString("email");

            // Fetch user details if the session email exists
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
                                UserName = reader["username"].ToString();
                                profilepic = reader["profilepic"].ToString();
                                id = reader["id"].ToString();
                                email = sessionEmail; // Set email from session
                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }

            if (movieId <= 0)
            {
                return NotFound(); // Return a 404 Not Found if movieId is invalid
            }

            // Fetch the movie with the corresponding movieId
            Movie = await _context.MoviesTables.FirstOrDefaultAsync(m => m.Movieid == movieId);

            if (Movie == null)
            {
                return NotFound(); // Return a 404 Not Found if movie is not found
            }

            // Split the movie field if there are multiple movies (comma-separated)
            MovieFiles = Movie.Movie?.Split(',').ToList() ?? new List<string>();

            SuggestedMovies = await _context.MoviesTables
                .Where(m => m.Movieid != movieId && m.Movietype == Movie.Movietype)
                .ToListAsync();
            Shuffle(SuggestedMovies);
            SuggestedMovies = SuggestedMovies.Take(10).ToList();

            Allmovies = await _context.MoviesTables.ToListAsync();
            Shuffle(Allmovies);



            int? sessionUserId = HttpContext.Session.GetInt32("Id");

            if (sessionUserId.HasValue) // Check if the user ID is present
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    // Prepare the SQL query to select top 5 records ordered by searchDateTime in ascending order
                    string query = "SELECT TOP 5 * FROM Search_history WHERE userid = @UserId ORDER BY searchDateTime DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Add the parameter to the command
                        cmd.Parameters.AddWithValue("@UserId", sessionUserId.Value); // Use .Value to get the int

                        con.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            // Check if there are any rows returned
                            while (dr.Read())
                            {
                                // Create a new Search_history object and populate it
                                var searchHistoryItem = new Search_history
                                {
                                    id = dr.GetInt32(0), // Assuming the first column is id
                                    userid = dr.IsDBNull(1) ? (int?)null : dr.GetInt32(1), // Assuming the second column is userid
                                    searchtext = dr.IsDBNull(2) ? null : dr.GetString(2), // Assuming the third column is searchtext
                                    searchDateTime = dr.GetDateTime(3) // Assuming the fourth column is searchDateTime
                                };
                                this.SearchHistory.Add(searchHistoryItem); // Use 'this' to refer to the instance property
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



            // Load movie categories
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string quary = "select * from Movie_category_table";
                using (SqlCommand cmd = new SqlCommand(quary, con))
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Movie_category_table Categories = new Movie_category_table();
                            Categories.categoryid = dr.GetInt32(0);
                            Categories.moviecategory = dr.GetString(1);
                            MovieCategories.Add(Categories);
                        }
                        con.Close();
                    }
                }
            }

            return Page();
        }


        private void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public async Task<IActionResult> OnPostSaveAsync(int userid, int movieid, string watchtime, string toteltime, string moviecomplet)
        {
            // Parse watchtime and toteltime
            double parsedWatchtime = double.TryParse(watchtime, out parsedWatchtime) ? parsedWatchtime : 0;
            double parsedToteltime = double.TryParse(toteltime, out parsedToteltime) ? parsedToteltime : 0;

            // Determine if the movie is completed (parse from "1" or "0")
            bool movieCompleted = moviecomplet == "1";  // '1' for true, '0' for false

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query;

                if (movieCompleted)
                {
                    // If the movie is completed, delete the record from the watch history
                    query = @"
            DELETE FROM Watch_history 
            WHERE userid = @userid AND movieid = @movieid";
                }
                else
                {
                    // If the movie is not completed, update or insert the watch history
                    query = @"
            IF EXISTS (SELECT 1 FROM Watch_history WHERE userid = @userid AND movieid = @movieid)
            BEGIN
                UPDATE Watch_history 
                SET watchtime = @watchtime, toteltime = @toteltime, lastwatchtime = @lastwatchtime, moviecomplet = @moviecomplet
                WHERE userid = @userid AND movieid = @movieid
            END
            ELSE
            BEGIN
                INSERT INTO Watch_history (userid, movieid, watchtime, toteltime, lastwatchtime, moviecomplet)
                VALUES (@userid, @movieid, @watchtime, @toteltime, @lastwatchtime, @moviecomplet)
            END";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.Int) { Value = userid });
                    cmd.Parameters.Add(new SqlParameter("@movieid", SqlDbType.Int) { Value = movieid });

                    if (!movieCompleted)
                    {
                        cmd.Parameters.Add(new SqlParameter("@watchtime", SqlDbType.VarChar) { Value = watchtime });
                        cmd.Parameters.Add(new SqlParameter("@toteltime", SqlDbType.VarChar) { Value = toteltime });
                        cmd.Parameters.Add(new SqlParameter("@lastwatchtime", SqlDbType.DateTime) { Value = DateTime.Now });
                        cmd.Parameters.Add(new SqlParameter("@moviecomplet", SqlDbType.Bit) { Value = movieCompleted });
                    }

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return a JSON response to the client
            return new JsonResult(new { success = true });
        }




    }
}
