using Main_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Netflix.Models;

namespace Main_Project.Pages.Movies
{
    public class Movies_or_ShowModel : PageModel
    {
        private readonly NetflixDataContext _context;

        private readonly string _connectionString;

        public Movies_or_ShowModel(NetflixDataContext context, IConfiguration configuration)
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
        public async Task OnGetAsync(string category)
        {
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
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id"));
                                UserName = reader["username"].ToString();
                                profilepic = reader["profilepic"].ToString();
                                email = sessionEmail;
                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }

            // Set the Category based on the provided parameter or default to "Movies"
            Category = string.IsNullOrEmpty(category) ? "Movies" : category;

            // Retrieve movies based on the specified category
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Movies_table WHERE Category = @Category";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Category", Category);
                    con.Open();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            Movies.Add(new MoviesTable
                            {
                                Movieid = reader.GetInt32(reader.GetOrdinal("Movieid")),
                                Movieposter = reader["Movieposter"].ToString(),
                                Movieposter2 = reader["Movieposter2"].ToString(),
                                Moviename = reader["Moviename"].ToString(),
                                Movietype = reader["Movietype"].ToString(),
                                // Add other fields as needed
                            });
                        }
                    }
                }
            }

            if (_context != null)
            {
                AllMovies = await _context.MoviesTables.ToListAsync();
            }
        }


    }
}
