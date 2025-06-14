using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Main_Project.Models; // Ensure this namespace is correct
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Netflix.Models;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace Main_Project.Pages
{
    public class HomeModel : PageModel
    {

        private readonly NetflixDataContext _context;
        private readonly string _connectionString;


        public HomeModel(NetflixDataContext context, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
            _context = context;
        }

        public string UserName { get; set; }
        public int id { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string UserRole { get; set; }
        public string SearchKeyword { get; set; }
        public string Message { get; set; }
        public IList<MoviesTable> MoviesTable { get; set; } = new List<MoviesTable>();

        public List<MoviesTable> Movies { get; set; }
        public List<MoviesTable> TvShows { get; set; }

        public List<Search_history> SearchHistory { get; set; } = new List<Search_history>();
        public List<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();
        public List<Watch_history> WatchHistories { get; set; }
        public List<Watch_history> WatchHistoryList { get; set; } = new();

        // Declare SuggestedMovies property
        public List<MoviesTable> SuggestedMovies { get; set; } = new List<MoviesTable>();
        public List<MoviesTable> Topwatchmovies { get; set; } = new List<MoviesTable>();



        public async Task<IActionResult> OnGetAsync()
        {

            // Check if the user is logged in by checking if the session has a valid user ID
            int? userId = HttpContext.Session.GetInt32("Id");
            if (!userId.HasValue)
            {
                return Redirect("/");
            }
            // Ensure user is logged in before fetching user activity
            if (userId.HasValue)
            {
                // Fetch the movie types from the last 5 days
                string typeQuery = @"
                SELECT movietype 
                FROM User_activity 
                WHERE userid = @UserId AND DateTime >= DATEADD(DAY, -5, GETDATE())";

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(typeQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId.Value);
                        con.Open();

                        // Dictionary to store movie type frequencies
                        Dictionary<string, int> movieTypeFrequency = new Dictionary<string, int>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Split movie types if there are multiple in a single row
                                string[] movieTypes = reader.GetString(0).Split(',');

                                // Count each movie type's occurrence
                                foreach (var type in movieTypes)
                                {
                                    string trimmedType = type.Trim();
                                    if (movieTypeFrequency.ContainsKey(trimmedType))
                                    {
                                        movieTypeFrequency[trimmedType]++;
                                    }
                                    else
                                    {
                                        movieTypeFrequency[trimmedType] = 1;
                                    }
                                }
                            }
                        }

                        // If no records in the past 5 days, fetch all user activity
                        if (!movieTypeFrequency.Any())
                        {
                            // Re-run query without the 5-day filter
                            typeQuery = "SELECT movietype FROM User_activity WHERE userid = @UserId";

                            cmd.CommandText = typeQuery;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string[] movieTypes = reader.GetString(0).Split(',');
                                    foreach (var type in movieTypes)
                                    {
                                        string trimmedType = type.Trim();
                                        if (movieTypeFrequency.ContainsKey(trimmedType))
                                        {
                                            movieTypeFrequency[trimmedType]++;
                                        }
                                        else
                                        {
                                            movieTypeFrequency[trimmedType] = 1;
                                        }
                                    }
                                }
                            }
                        }

                        // Find the most-watched movie type
                        var mostWatchedMovieType = movieTypeFrequency
                            .OrderByDescending(kv => kv.Value)
                            .FirstOrDefault().Key;

                        if (mostWatchedMovieType != null)
                        {
                            // Prepare a list to hold all suggested movies
                            var allSuggestedMovies = new List<MoviesTable>();

                            // Fetch movies based on the most-watched category first
                            string movieQuery = @"
                            SELECT Movieid, Moviename, Movieposter2, movielike, trailer, content_advisory  
                            FROM Movies_table 
                            WHERE movietype LIKE @MovieType 
                            ORDER BY movielike DESC"; // Fetch movies in descending order of likes

                            using (SqlCommand movieCmd = new SqlCommand(movieQuery, con))
                            {
                                movieCmd.Parameters.AddWithValue("@MovieType", $"%{mostWatchedMovieType}%");

                                using (SqlDataReader movieReader = movieCmd.ExecuteReader())
                                {
                                    while (movieReader.Read())
                                    {
                                        var movie = new MoviesTable
                                        {
                                            Movieid = movieReader.GetInt32(0),
                                            Moviename = movieReader.GetString(1),
                                            Movieposter2 = movieReader.GetString(2),
                                            Movielike = movieReader.IsDBNull(3) ? (int?)null : movieReader.GetInt32(3),
                                            Trailer = movieReader.IsDBNull(4) ? string.Empty : movieReader.GetString(4), // Fetch trailer
                                            Content_advisory = movieReader.IsDBNull(5) ? string.Empty : movieReader.GetString(5), // Fetch content advisory
                                            Movietype = mostWatchedMovieType // Add the current movie type
                                        };
                                        allSuggestedMovies.Add(movie); // Add movies to the combined list
                                    }
                                }
                            }

                            // Sort all suggested movies by movielike in descending order and take the top 10
                            var top10Movies = allSuggestedMovies
                                .GroupBy(m => m.Movieid) // Group by Movieid to avoid duplicates
                                .Select(g => g.First())
                                .OrderByDescending(m => m.Movielike) // Order by likes in descending order
                                .Take(10)
                                .ToList();

                            // Randomize the order of top 10 movies
                            var random = new Random();
                            var randomizedTop10Movies = top10Movies.OrderBy(x => random.Next()).ToList();

                            // Add the randomized top 10 movies to SuggestedMovies
                            SuggestedMovies.AddRange(randomizedTop10Movies);
                        }
                    }
                }
            }



            Dictionary<string, List<MoviesTable>> movieTypeGroups = new Dictionary<string, List<MoviesTable>>();

            string moviesQuery = @"
                SELECT Movieid, Moviename, Movieposter2, movietype, trailer, Content_advisory 
                FROM Movies_table;";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                using (SqlCommand movieCmd = new SqlCommand(moviesQuery, con))
                {
                    using (SqlDataReader movieReader = movieCmd.ExecuteReader())
                    {
                        while (movieReader.Read())
                        {
                            // Create a new movie object, with null handling
                            var movie = new MoviesTable
                            {
                                Movieid = movieReader.GetInt32(0),
                                Moviename = movieReader.GetString(1),
                                Movieposter2 = !movieReader.IsDBNull(2) ? movieReader.GetString(2) : string.Empty,
                                Movietype = !movieReader.IsDBNull(3) ? movieReader.GetString(3) : string.Empty,
                                Trailer = !movieReader.IsDBNull(4) ? movieReader.GetString(4) : string.Empty,
                                Content_advisory = !movieReader.IsDBNull(5) ? movieReader.GetString(5) : string.Empty
                            };

                            // Split the movie types
                            string[] movieTypes = movie.Movietype.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var movieType in movieTypes)
                            {
                                string trimmedMovieType = movieType.Trim();

                                // Add movie to the corresponding movie type group
                                if (!movieTypeGroups.ContainsKey(trimmedMovieType))
                                {
                                    movieTypeGroups[trimmedMovieType] = new List<MoviesTable>();
                                }

                                movieTypeGroups[trimmedMovieType].Add(movie);
                            }
                        }
                    }
                }

                // Step 2: Randomly select 10 movies for each movie type
                var randomMovieTypeGroups = new Dictionary<string, List<MoviesTable>>();
                Random random = new Random();

                foreach (var kvp in movieTypeGroups)
                {
                    string movieType = kvp.Key;
                    List<MoviesTable> allMovies = kvp.Value;

                    // Shuffle the list and take the first 10
                    var randomMovies = allMovies.OrderBy(x => random.Next()).Take(10).ToList();
                    randomMovieTypeGroups[movieType] = randomMovies;
                }

                // Store the movie groups in ViewData for use in the view
                ViewData["MovieTypeGroups"] = randomMovieTypeGroups;
            }







            // Fetch other data like SearchHistory and WatchHistories here



            string sessionEmail = HttpContext.Session.GetString("email");

            // Fetch the user's username from the database
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
                                email = sessionEmail;
                                UserRole = reader["role"].ToString();
                            }
                        }
                    }
                }
            }

            // Retrieve the logged-in user's search history from the database
            if (userId.HasValue)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT TOP 5 * FROM Search_history WHERE userid = @UserId ORDER BY searchDateTime DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId.Value); // Use .Value to get the int value
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
                                SearchHistory.Add(searchHistoryItem);
                            }
                        }
                    }
                }
            }

            // Retrieve movie categories from the database
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
                            var Categories = new Movie_category_table
                            {
                                categoryid = dr.GetInt32(0),
                                moviecategory = dr.GetString(1)
                            };
                            MovieCategories.Add(Categories);
                        }
                    }
                }
            }

            // Retrieve the list of movies from the database using Entity Framework
            MoviesTable = await _context.MoviesTables.ToListAsync();

            // Pass the SearchHistory to the ViewData for access in the header
            ViewData["SearchHistory"] = SearchHistory;

            // Return the page for rendering

            if (userId == null)
            {
                return Redirect("/");
            }
            else
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                string query = @"
    SELECT 
        WH.id, WH.userid, WH.movieid, WH.watchtime, WH.toteltime, WH.lastwatchtime, WH.moviecomplet, WH.filename, 
        M.Movieid, M.Moviename, M.Movieposter, M.Movieposter2, M.trailer, 
        U.id, U.username, U.profilepic, U.email, U.role
    FROM Watch_history WH
    LEFT JOIN Movies_Table M ON WH.movieid = M.Movieid
    LEFT JOIN User_data U ON WH.userid = U.id
    WHERE WH.userid = @userId AND WH.moviecomplet = 0";


                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", id);
                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var watchHistory = new Watch_history
                    {
                        id = reader.GetInt32(0),
                        userid = reader.GetInt32(1),
                        movieid = reader.GetInt32(2),
                        watchtime = reader.GetString(3),
                        toteltime = reader.GetString(4),
                        lastwatchtime = DateOnly.FromDateTime(reader.GetDateTime(5)),
                        moviecomplet = reader.GetBoolean(6),
                        filename = reader.IsDBNull(7) ? null : reader.GetString(7)
                    };

                    var movie = new MoviesTable
                    {
                        Movieid = reader.GetInt32(8),
                        Moviename = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Movieposter = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Movieposter2 = reader.IsDBNull(11) ? null : reader.GetString(11),
                        Trailer = reader.IsDBNull(12) ? null : reader.GetString(12)
                    };

                    watchHistory.Movie = movie;

                    WatchHistoryList.Add(watchHistory);

                    if (!MoviesTable.Any(m => m.Movieid == movie.Movieid))
                    {
                        MoviesTable.Add(movie);
                    }
                }
            }


                return Page();
        }



        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostSaveMovieTypeAsync(string movietype, string movieid)
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            //Console.WriteLine($"UserId from session: {userId}");

            if (userId == null)
            {
                // Handle the case when the user is not logged in or UserId is not found
                return RedirectToPage("/Sign_in"); // Redirect to login page or show an error
            }

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
                    //Console.WriteLine("Exception: " + ex.Message);
                    // Handle the exception (logging, etc.) and redirect to an error page
                    return RedirectToPage("/Error"); // Change this to your error handling logic
                }
            }

            //Console.WriteLine("Invalid data");
            return RedirectToPage("/Error"); // Redirect on invalid data
        }



        public async Task<IActionResult> OnPostRemoveFromList()
        {
            var userIdStr = Request.Form["userid"].ToString();
            var movieIdStr = Request.Form["movieid"].ToString();

            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid userid");
            }

            if (!int.TryParse(movieIdStr, out int movieId))
            {
                return BadRequest("Invalid movieid");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "UPDATE Watch_history SET moviecomplet = @moviecomplet WHERE userid = @userid AND movieid = @movieid";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@moviecomplet", true);
                    command.Parameters.AddWithValue("@userid", userId);
                    command.Parameters.AddWithValue("@movieid", movieId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound("Movie watch history not found.");
                    }
                }
            }

            return RedirectToPage("/Home");
        }


    }
}
