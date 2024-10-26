using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Data;

namespace Main_Project.Pages
{
    public class Single_movieModel : PageModel
    {
        private readonly NetflixDataContext _context;
        private readonly ILogger<Single_movieModel> _logger;

        string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

        public Single_movieModel(NetflixDataContext context, ILogger<Single_movieModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public MoviesTable Movie { get; set; }
        public IList<MoviesTable> SuggestedMovies { get; set; }
        public IList<MoviesTable> Allmovies { get; set; }
        public IList<MoviesTable> IMDBrating { get; set; }
        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string UserRole { get; set; }
        public int id { get; set; }
        public string SearchKeyword { get; set; }
        public IList<MoviesTable> MoviesTable { get; set; } = new List<MoviesTable>();
        public List<Search_history> SearchHistory { get; set; } = new List<Search_history>();
        public IList<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();

        public async Task<IActionResult> OnGetAsync(int? movieId, string SearchKeyword)
        {
            profilepic = HttpContext.Session.GetString("profilepic");
            UserName = HttpContext.Session.GetString("Username");
            string sessionEmail = HttpContext.Session.GetString("email");

            string subscriptionActive = HttpContext.Session.GetString("SubscriptionActive");

            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
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
                    }
                }
            }

            if (string.IsNullOrEmpty(sessionEmail))
            {
                _logger.LogError("Email not found in session");
                return RedirectToPage("/Sign_in"); // Or handle this situation as needed
            }

            int? sessionUserId = HttpContext.Session.GetInt32("Id");

            if (sessionUserId.HasValue) // Check if the user ID is present
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
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


            // Fetch the user's subscription status from the database
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                string query = "SELECT subscriptionactive FROM User_data WHERE email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", sessionEmail);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isSubscriptionActive = reader.GetBoolean(0); // Assuming SubscriptionActive is a bool
                            HttpContext.Session.SetString("SubscriptionActive", isSubscriptionActive ? "True" : "False");
                        }
                    }
                }
            }

            // HashSet to track already added movie ids
            HashSet<int> addedMovieIds = new HashSet<int>();

            // Check if a valid movieId is provided
            if (movieId.HasValue && movieId.Value > 0)
            {
                Movie = await _context.MoviesTables
                    .FirstOrDefaultAsync(m => m.Movieid == movieId.Value);

                if (Movie == null)
                {
                    _logger.LogWarning("Movie not found with id: {movieId}", movieId);
                    return NotFound(); // Return a 404 Not Found if movie is not found
                }

                // Get all movie types as a list
                var movieTypes = Movie.Movietype.Split(',').Select(m => m.Trim()).ToList();

                // Fetch suggested movies based on all movie types, exclude movies already added
                SuggestedMovies = await _context.MoviesTables
                    .Where(m => m.Movieid != movieId.Value && !addedMovieIds.Contains(m.Movieid) && movieTypes.Any(mt => m.Movietype.Contains(mt)))
                    .OrderByDescending(m => m.Movielike)
                    .Take(10) // Limit to top 10 suggested movies
                    .ToListAsync();

                // Add suggested movies to HashSet
                foreach (var suggestion in SuggestedMovies)
                {
                    addedMovieIds.Add(suggestion.Movieid);
                }

                // Fetch top 10 IMDB rated movies, exclude movies already added
                IMDBrating = await _context.MoviesTables
                    .Where(m => m.Movieid != movieId.Value && !addedMovieIds.Contains(m.Movieid))
                    .OrderByDescending(m => m.Movierating)
                    .Take(10) // Limit to top 10 IMDB rated movies
                    .ToListAsync();

                // Add IMDB rated movies to HashSet
                foreach (var rating in IMDBrating)
                {
                    addedMovieIds.Add(rating.Movieid);
                }
            }

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
                            Movie_category_table Categories = new Movie_category_table
                            {
                                categoryid = dr.GetInt32(0),
                                moviecategory = dr.GetString(1)
                            };
                            MovieCategories.Add(Categories);
                        }
                    }
                }
            }

            // Fetch all movies, exclude those already added
            Allmovies = await _context.MoviesTables
                .Where(m => !addedMovieIds.Contains(m.Movieid))
                .ToListAsync();

            Shuffle(Allmovies); // Shuffle the list of all movies




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

        public async Task<IActionResult> OnPostAsync(int movieId, string actionType)
        {
            var userId = HttpContext.Session.GetInt32("Id");
            var subscriptionActiveString = HttpContext.Session.GetString("SubscriptionActive");
            bool subscriptionActive = subscriptionActiveString != null && subscriptionActiveString == "True";

            if (userId == null)
            {
                return RedirectToPage("/Sign_in");
            }

            // Check if subscription is active
            if (!subscriptionActive)
            {
                // If the subscription is inactive, prevent any action
                TempData["ErrorMessage"] = "Your subscription has expired. Please renew your subscription to continue.";
                return RedirectToPage("/Single_movie", new { movieId });
            }

            // If subscription is active, allow the action
            switch (actionType)
            {
                case "addToWatchList":
                    var existingWatchListItem = await _context.WatchLists
                        .FirstOrDefaultAsync(w => w.Userid == userId.Value && w.Movieid == movieId);

                    if (existingWatchListItem != null)
                    {
                        TempData["Message"] = "You have already added this movie to your watch list.";
                        return RedirectToPage("/Single_movie", new { movieId });
                    }

                    var watchListItem = new WatchList
                    {
                        Userid = userId.Value,
                        Movieid = movieId
                    };

                    _context.WatchLists.Add(watchListItem);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Movie added to your watch list.";
                    break;

                case "like":
                    var movieToLike = await _context.MoviesTables.FirstOrDefaultAsync(m => m.Movieid == movieId);
                    if (movieToLike != null)
                    {
                        movieToLike.Movielike = (movieToLike.Movielike ?? 0) + 1;
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "You liked this movie.";
                    }
                    break;

                case "dislike":
                    var movieToDislike = await _context.MoviesTables.FirstOrDefaultAsync(m => m.Movieid == movieId);
                    if (movieToDislike != null)
                    {
                        movieToDislike.Movielike = (movieToDislike.Movielike ?? 0) - 1;
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "You disliked this movie.";
                    }
                    break;
            }

            return RedirectToPage("/Single_movie", new { movieId });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostSaveMovieTypeAsync(string movietype, string movieid)
        {
            // Retrieve user ID from session
            int? userId = HttpContext.Session.GetInt32("Id");

            // Log the userId for debugging purposes
            Console.WriteLine($"UserId from session: {userId}");

            // Check if the required values are present
            if (userId.HasValue && !string.IsNullOrEmpty(movietype) && !string.IsNullOrEmpty(movieid))
            {
                try
                {
                    // Using Entity Framework or ADO.NET to insert the user activity
                    using (SqlConnection con = new SqlConnection(connectionstring))
                    {
                        string query = "INSERT INTO User_activity (userid, movietype, DateTime) VALUES (@UserId, @MovieType, @DateTime)";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            // Add parameters to the SQL command
                            cmd.Parameters.AddWithValue("@UserId", userId.Value);
                            cmd.Parameters.AddWithValue("@MovieType", movietype);
                            cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);

                            // Open connection and execute the command
                            await con.OpenAsync();
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // After successfully saving the data, redirect to Single_movie page with the movie ID
                    return RedirectToPage("/Single_movie", new { movieId = movieid });
                }
                catch (Exception ex)
                {
                    // Log the exception message
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
