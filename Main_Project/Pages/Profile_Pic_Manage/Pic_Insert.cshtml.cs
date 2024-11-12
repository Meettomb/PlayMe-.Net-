using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Main_Project.Models;
using Netflix.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Main_Project.Pages.Profile_Pic_Manage
{
    public class Pic_InsertModel : PageModel
    {

        private readonly NetflixDataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;
        public List<user_regi> userlist = new List<user_regi>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }



        [BindProperty]
        public Profile_pic Profile_pic { get; set; } = default!;
        public Pic_InsertModel(NetflixDataContext context, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }



        [BindProperty]
        public IFormFile Upload { get; set; } // Property name matches input field's asp-for


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

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Upload != null)
            {
                var filePath = Path.Combine(_environment.WebRootPath, "profile_pic", Upload.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty); // Ensure directory exists

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(fileStream);
                }

                // Set the filename to the model property
                Profile_pic.Pics = Upload.FileName;

                // Save to database
                _context.Profile_pic.Add(Profile_pic);
                await _context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError("Upload", "Please upload a profile picture.");
                return Page();
            }

            return RedirectToPage("./Pic_Insert");
        }
    }
}
