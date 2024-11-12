using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using Netflix.Models;
using Microsoft.Data.SqlClient;

namespace Main_Project.Pages.Profile_Pic_Manage
{
    public class DeleteModel : PageModel
    {
        private readonly NetflixDataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;

        public DeleteModel(NetflixDataContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; } // Use ProfilePic consistently

        [BindProperty]
        public Profile_pic Profile_pic { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile_pic = await _context.Profile_pic.FirstOrDefaultAsync(m => m.Id == id);

            if (profile_pic == null)
            {
                return NotFound();
            }
            else
            {
                Profile_pic = profile_pic;
            }

            LoadUserDetails();
            return Page();
        }

        private void LoadUserDetails()
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
        }
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile_pic = await _context.Profile_pic.FindAsync(id);
            if (profile_pic != null)
            {
                Profile_pic = profile_pic;
                _context.Profile_pic.Remove(Profile_pic);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Pic_Index");
        }
    }
}
