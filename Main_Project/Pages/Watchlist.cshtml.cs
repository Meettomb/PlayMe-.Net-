using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Netflix.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Main_Project.Pages
{
    public class WatchlistModel : PageModel
    {
        private readonly NetflixDataContext _context;

        public WatchlistModel(NetflixDataContext context)
        {
            _context = context;
        }

        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string UserRole { get; set; }
        public int id { get; set; }
        public List<MoviesTable> AllMovies { get; set; } = new List<MoviesTable>();

        public IList<WatchList> WatchLists { get; set; } = new List<WatchList>();
        public IList<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();
        public List<Search_history> SearchHistory { get; set; } = new List<Search_history>();

        public async Task OnGetAsync()
        {
            UserName = HttpContext.Session.GetString("Username");
            profilepic = HttpContext.Session.GetString("profilepic");
            string sessionEmail = HttpContext.Session.GetString("email");


            string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

            AllMovies = await _context.MoviesTables.ToListAsync(); // Fetch all movies if no category is specified


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
                        con.Close();
                    }
                }
            }



            // Fetch watch list items including related movie details
            WatchLists = await _context.WatchLists
                .Include(wl => wl.Movie) // Include related movie details
                .Where(wl => wl.Userid == HttpContext.Session.GetInt32("Id"))
                .ToListAsync();


            using (SqlConnection con = new SqlConnection(connectionstring))
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


        }

        public async Task<IActionResult> OnPostAsync(int movieId, string action)
        {
            var userId = HttpContext.Session.GetInt32("Id");
            if (userId == null)
            {
                return RedirectToPage("/SignIn"); // Redirect to sign-in page if user is not logged in
            }

            if (action == "add")
            {
                // Check if the movie is already in the user's watch list
                var existingWatchListItem = await _context.WatchLists
                    .FirstOrDefaultAsync(wl => wl.Userid == userId && wl.Movieid == movieId);

                if (existingWatchListItem != null)
                {
                    // Movie is already in the watch list, return a message
                    TempData["Message"] = "You have already added this movie to your watch list.";
                    return RedirectToPage("/Home");
                }

                // Create a new WatchList object
                var watchListItem = new WatchList
                {
                    Userid = userId.Value,
                    Movieid = movieId
                };

                // Add the watch list item to the database
                _context.WatchLists.Add(watchListItem);
                await _context.SaveChangesAsync();

                // Return a message indicating success
                TempData["Message"] = "Movie added to your watch list.";
                return RedirectToPage("/Home");
            }
            else if (action == "remove")
            {
                // Find the watch list item to be removed
                var watchListItem = await _context.WatchLists
                    .FirstOrDefaultAsync(wl => wl.Userid == userId && wl.Movieid == movieId);

                if (watchListItem != null)
                {
                    // Remove the watch list item from the database
                    _context.WatchLists.Remove(watchListItem);
                    await _context.SaveChangesAsync();

                    // Return a message indicating success
                    TempData["Message"] = "Movie removed from your watch list.";
                }

                // Redirect back to the watch list page
                return RedirectToPage("/Watchlist");
            }

            return Page();
        }
    }
}
