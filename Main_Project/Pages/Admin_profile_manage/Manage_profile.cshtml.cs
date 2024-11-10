using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using Main_Project.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Text;
using static System.Net.WebRequestMethods;


namespace Main_Project.Pages.Admin_profile_manage
{
    public class Manage_profileModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<Manage_profileModel> _logger; // Add logger for debugging
        private readonly string _connectionString;
        public List<user_regi> userlist = new List<user_regi>();
       
        public Manage_profileModel(IEmailService emailService, ILogger<Manage_profileModel> logger, IConfiguration configuration)
        {
            _emailService = emailService;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public string id { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public string ErrorMessage { get; set; }
        public string OTP { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            string sessionEmail = HttpContext.Session.GetString("email");
            int? userId = HttpContext.Session.GetInt32("Id");
            string role = HttpContext.Session.GetString("UserRole");
            if (!userId.HasValue | role != "admin")
            {
                return Redirect("/");
            }
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string selectQuery = "SELECT * FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                id = reader["id"].ToString();
                                dob = reader["dob"].ToString();
                                gender = reader["gender"].ToString();
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

        public async Task<IActionResult> OnPostUpdateProfilePicAsync()
        {
            var file = Request.Form.Files["profilepic"];
            string profilePic = null;

            if (file != null && file.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile_pic");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                profilePic = Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);
                var fullPath = Path.Combine(uploads, profilePic);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string sessionEmail = HttpContext.Session.GetString("email");

                if (string.IsNullOrEmpty(sessionEmail))
                {
                    TempData["error"] = "User session expired. Please log in again.";
                    return RedirectToPage("/Sign_in");
                }

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string updateQuery = "UPDATE User_data SET profilepic = @profilePic WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@profilePic", profilePic);
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);

                        con.Open();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }

                TempData["message"] = "Profile picture updated successfully.";
            }

            await OnGetAsync(); // Reflect updated changes
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateOtherDetailAsync()
        {
            string sessionEmail = HttpContext.Session.GetString("email");
            string username = Request.Form["fullname"];
            string dob = Request.Form["dob"];
            string gender = Request.Form["gender"];

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string updateQuery = @"
                UPDATE User_data 
                SET username = @Username, dob = @Dob, gender = @Gender 
                WHERE email = @Email";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Dob", dob);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@Email", sessionEmail);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    con.Close();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Update successful.");
                    }
                    else
                    {
                        Console.WriteLine("Update failed. No rows affected.");
                    }
                }
            }

            await OnGetAsync(); // Reload updated data
            TempData["message"] = "Details updated successfully.";
            return Page();
        }

       
    }
}
