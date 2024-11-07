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
    public class DeleteModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;
        private readonly string _connectionString;

        public DeleteModel(Main_Project.Models.NetflixDataContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        [BindProperty]
        public MoviesTable MoviesTable { get; set; } = default!;
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moviestable = await _context.MoviesTables.FindAsync(id);
            if (moviestable != null)
            {
                MoviesTable = moviestable;
                _context.MoviesTables.Remove(MoviesTable);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
