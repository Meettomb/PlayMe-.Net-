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
using System.Configuration;

namespace Main_Project.Pages
{
    public class Single_movieModel : PageModel
    {
        private readonly NetflixDataContext _context;
        private readonly ILogger<Single_movieModel> _logger;


        private readonly string _connectionString;
        public Single_movieModel(NetflixDataContext context, ILogger<Single_movieModel> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public MoviesTable Movie { get; set; }
        public IList<MoviesTable> SuggestedMovies { get; set; }
        public IList<MoviesTable> Allmovies { get; set; }
        public IList<MoviesTable> IMDBrating { get; set; }
        public IList<Movie_like> Movie_Like { get; set; }
        public IList<Movie_dislike> Movie_dislike { get; set; }
        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string UserRole { get; set; }
        public int id { get; set; }
        public string SearchKeyword { get; set; }
        public IList<MoviesTable> MoviesTable { get; set; } = new List<MoviesTable>();
        public List<Search_history> SearchHistory { get; set; } = new List<Search_history>();
        public IList<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();
        public bool HasUserLikedMovie { get; set; }
        public bool HasUserDislikedMovie { get; set; }
        public bool HasUserAddedOnWatchlistMovie { get; set; }

        public async Task<IActionResult> OnGetAsync(int? movieId, string SearchKeyword)
        {
            profilepic = HttpContext.Session.GetString("profilepic");
            UserName = HttpContext.Session.GetString("Username");
            string sessionEmail = HttpContext.Session.GetString("email");

            string subscriptionActive = HttpContext.Session.GetString("SubscriptionActive");

            var userId = HttpContext.Session.GetInt32("Id");

            if (!userId.HasValue)
            {
                return RedirectToPage("/Sign_in");
            }

            HasUserAddedOnWatchlistMovie = await _context.WatchLists
                .AnyAsync(w => w.Userid == userId.Value && w.Movieid == movieId);

            string checkLikeQuery = $"SELECT COUNT(*) FROM Movie_like WHERE userid = {userId.Value} AND movieid = {movieId}";
            HasUserLikedMovie = await CheckIfExistsAsync(checkLikeQuery);

            string checkDislikeQuery = $"SELECT COUNT(*) FROM Movie_dislike WHERE userid = {userId.Value} AND movieid = {movieId}";
            HasUserDislikedMovie = await CheckIfExistsAsync(checkDislikeQuery);

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
                    }
                }
            }

            if (string.IsNullOrEmpty(sessionEmail))
            {
                _logger.LogError("Email not found in session");
                return RedirectToPage("/Sign_in"); 
            }

            int? sessionUserId = HttpContext.Session.GetInt32("Id");

            if (sessionUserId.HasValue) 
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT TOP 5 * FROM Search_history WHERE userid = @UserId ORDER BY searchDateTime DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", sessionUserId.Value); 

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
            }


            using (SqlConnection con = new SqlConnection(_connectionString))
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
                            bool isSubscriptionActive = reader.GetBoolean(0);
                            HttpContext.Session.SetString("SubscriptionActive", isSubscriptionActive ? "True" : "False");
                        }
                    }
                }
            }

            HashSet<int> addedMovieIds = new HashSet<int>();

            if (movieId.HasValue && movieId.Value > 0)
            {
                Movie = await _context.MoviesTables
                    .FirstOrDefaultAsync(m => m.Movieid == movieId.Value);

                if (Movie == null)
                {
                    _logger.LogWarning("Movie not found with id: {movieId}", movieId);
                    return NotFound();
                }

                var movieTypes = Movie.Movietype.Split(',').Select(m => m.Trim()).ToList();
                
                SuggestedMovies = await _context.MoviesTables
                    .Where(m => m.Movieid != movieId.Value && !addedMovieIds.Contains(m.Movieid) && movieTypes.Any(mt => m.Movietype.Contains(mt)))
                    .OrderByDescending(m => m.Movielike)
                    .Take(10) 
                    .ToListAsync();

                foreach (var suggestion in SuggestedMovies)
                {
                    addedMovieIds.Add(suggestion.Movieid);
                }

                IMDBrating = await _context.MoviesTables
                    .Where(m => m.Movieid != movieId.Value && !addedMovieIds.Contains(m.Movieid))
                    .OrderByDescending(m => m.Movierating)
                    .Take(10) 
                    .ToListAsync();

                foreach (var rating in IMDBrating)
                {
                    addedMovieIds.Add(rating.Movieid);
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

            Allmovies = await _context.MoviesTables
                .Where(m => !addedMovieIds.Contains(m.Movieid))
                .ToListAsync();

            Shuffle(Allmovies);




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

            if (!subscriptionActive)
            {
                TempData["ErrorMessage"] = "Your subscription has expired. Please renew your subscription to continue.";
                return RedirectToPage("/Single_movie", new { movieId });
            }

            switch (actionType)
            {
                case "addToWatchList":
                    var existingWatchListItem = await _context.WatchLists
                        .FirstOrDefaultAsync(w => w.Userid == userId.Value && w.Movieid == movieId);

                    if (existingWatchListItem != null)
                    {
                        _context.WatchLists.Remove(existingWatchListItem);
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Movie removed from your watch list.";
                    }
                    else
                    {
                        var watchListItem = new WatchList
                        {
                            Userid = userId.Value,
                            Movieid = movieId
                        };

                        _context.WatchLists.Add(watchListItem);
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Movie added to your watch list.";
                    }
                    break;



                case "like":
                    string checkLikeQuery = $"SELECT COUNT(*) FROM Movie_like WHERE userid = {userId} AND movieid = {movieId}";
                    bool alreadyLiked = await CheckIfExistsAsync(checkLikeQuery);

                    if (alreadyLiked)
                    {
                        string deleteLikeQuery = $"DELETE FROM Movie_like WHERE userid = {userId} AND movieid = {movieId}";
                        await ExecuteSqlCommandAsync(deleteLikeQuery);
                        
                        string decrementLikeQuery = $"UPDATE Movies_table SET Movielike = COALESCE(Movielike, 0) - 1 WHERE Movieid = {movieId}";
                        await ExecuteSqlCommandAsync(decrementLikeQuery);

                        TempData["Message"] = "You removed your like from this movie.";
                    }
                    else
                    {
                        string insertLikeQuery = $"INSERT INTO Movie_like (userid, movieid) VALUES ({userId}, {movieId})";
                        await ExecuteSqlCommandAsync(insertLikeQuery);

                        string incrementLikeQuery = $"UPDATE Movies_table SET Movielike = COALESCE(Movielike, 0) + 1 WHERE Movieid = {movieId}";
                        await ExecuteSqlCommandAsync(incrementLikeQuery);

                        TempData["Message"] = "You liked this movie.";
                    }
                    break;


                case "dislike":
                    string checkDislikeQuery = $"SELECT COUNT(*) FROM Movie_dislike WHERE userid = {userId.Value} AND movieid = {movieId}";
                    bool alreadyDisliked = await CheckIfExistsAsync(checkDislikeQuery);

                    if (alreadyDisliked)
                    {
                        string deleteDislikeQuery = $"DELETE FROM Movie_dislike WHERE userid = {userId.Value} AND movieid = {movieId}";
                        await ExecuteSqlCommandAsync(deleteDislikeQuery);
                        
                        string incrementLikeQuery = $"UPDATE Movies_table SET Movielike = COALESCE(Movielike, 0) + 1 WHERE Movieid = {movieId}";
                        await ExecuteSqlCommandAsync(incrementLikeQuery);

                        TempData["Message"] = "You removed your dislike from this movie.";
                    }
                    else
                    {
                        string insertDislikeQuery = $"INSERT INTO Movie_dislike (userid, movieid) VALUES ({userId.Value}, {movieId})";
                        await ExecuteSqlCommandAsync(insertDislikeQuery);

                        string decrementLikeQuery = $"UPDATE Movies_table SET Movielike = COALESCE(Movielike, 0) - 1 WHERE Movieid = {movieId}";
                        await ExecuteSqlCommandAsync(decrementLikeQuery);

                        TempData["Message"] = "You disliked this movie.";
                    }
                    break;
            }

            return RedirectToPage("/Single_movie", new { movieId });
        }

        private async Task ExecuteSqlCommandAsync(string query)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        private async Task<bool> CheckIfExistsAsync(string query)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostSaveMovieTypeAsync(string movietype, string movieid)
        {
            int? userId = HttpContext.Session.GetInt32("Id");

        
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
                            cmd.Parameters.AddWithValue("@MovieType", movietype);
                            cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);

                            await con.OpenAsync();
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    return RedirectToPage("/Single_movie", new { movieId = movieid });
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error");
                }
            }

            return RedirectToPage("/Error");
        }



    }
}
