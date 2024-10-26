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

namespace Main_Project.Pages.Movies
{
    public class DetailsModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;

        public DetailsModel(Main_Project.Models.NetflixDataContext context)
        {
            _context = context;
        }
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public MoviesTable MoviesTable { get; set; } = default!;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

            string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
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

            if (id == null)
            {
                return NotFound();
            }

            var moviestable = await _context.MoviesTables.FirstOrDefaultAsync(m => m.Movieid == id);
            
            if (moviestable == null)
            {
                return NotFound();
            }
            else
            {
                MoviesTable = moviestable;
            }
            return Page();
        }
    }
}
