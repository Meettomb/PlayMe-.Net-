using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Authorization;

namespace Main_Project.Pages.Movies
{
    public class IndexModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;
        private readonly string _connectionString;

        public IndexModel(Main_Project.Models.NetflixDataContext context, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
            _context = context;
        }

        public IList<MoviesTable> MoviesTable { get;set; } = default!;
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public async Task OnGetAsync()
        {
           
            string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
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
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }
            MoviesTable = await _context.MoviesTables.ToListAsync();
        }
    }
}
