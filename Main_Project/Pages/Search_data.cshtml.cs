using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Netflix.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Main_Project.Pages
{
    public class Search_dataModel : PageModel
    {
        private readonly NetflixDataContext _context;
        private readonly string _connectionString;
      
        public Search_dataModel(NetflixDataContext context, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
            _context = context;
        }

        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string UserRole { get; set; }
        public int id { get; set; }
        public IList<MoviesTable> Movies { get; set; } = new List<MoviesTable>();
        public List<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();
        public List<Search_history> SearchHistory { get; set; } = new List<Search_history>();

        [BindProperty(SupportsGet = true)]
        public string SearchKeyword { get; set; }

        public async Task OnGetAsync(string searchKeyword)
        {


            UserName = HttpContext.Session.GetString("Username");
            profilepic = HttpContext.Session.GetString("profilepic");
            string sessionEmail = HttpContext.Session.GetString("email");

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
                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }

            // Fetch movies matching the search keyword (case-insensitive)
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                Movies = await _context.MoviesTables
                    .Where(m => EF.Functions.Like(m.Moviename.ToLower(), $"%{searchKeyword.ToLower()}%"))
                    .ToListAsync();
            }




            // Retrieve the logged-in user's ID from session
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


            // Fetch movie categories
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
                            Movie_category_table category = new Movie_category_table
                            {
                                categoryid = dr.GetInt32(0),
                                moviecategory = dr.GetString(1),
                            };
                            MovieCategories.Add(category);
                        }
                        con.Close();
                    }
                }
            }

            // If a search keyword is provided, save to search history
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                // Get the user ID from the session
                int? userId = HttpContext.Session.GetInt32("Id");

                if (userId.HasValue) // Check if userId is valid
                {
                    // Insert search history directly into the database
                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        string insertQuery = "INSERT INTO Search_history (userid, searchtext, searchDateTime) VALUES (@UserId, @SearchText, @SearchDateTime)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId.Value); // Store user ID from session
                            cmd.Parameters.AddWithValue("@SearchText", searchKeyword);
                            cmd.Parameters.AddWithValue("@SearchDateTime", DateTime.Now);

                            try
                            {
                                con.Open();
                                await cmd.ExecuteNonQueryAsync();
                                con.Close();
                            }
                            catch (Exception ex)
                            {
                                // Log the exception or display an error message
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                    }
                }
            }
        } // This closing brace should be here, indicating the end of the OnGetAsync method

        private int GetCurrentUserId()
        {
            // Get UserId from session
            string userIdString = HttpContext.Session.GetString("UserId");

            // Check if userIdString is null or empty, return -1 if it is
            if (string.IsNullOrEmpty(userIdString))
            {
                return -1; // Indicating user ID is not available
            }

            return int.Parse(userIdString); // Convert to integer
        }
    }
}
