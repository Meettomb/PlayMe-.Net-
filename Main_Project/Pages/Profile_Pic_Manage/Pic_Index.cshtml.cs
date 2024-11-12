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
    public class Pic_IndexModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;

        public List<user_regi> userlist = new List<user_regi>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }

        private readonly string _connectionString;
        public Pic_IndexModel(Main_Project.Models.NetflixDataContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public IList<Profile_pic> Profile_pic { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            string role = HttpContext.Session.GetString("UserRole");
            if (!userId.HasValue | role != "admin")
            {
                return Redirect("/");
            }
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

            Profile_pic = await _context.Profile_pic.ToListAsync();
            return Page();
        }

    }
}
