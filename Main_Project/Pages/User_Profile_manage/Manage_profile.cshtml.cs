using Main_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Main_Project.Pages.User_Profile_manage
{
    public class Manage_profileModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<Manage_profileModel> _logger;
        private readonly string _connectionString;

        public Manage_profileModel(IEmailService emailService, ILogger<Manage_profileModel> logger, IConfiguration configuration)
        {
            _emailService = emailService;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public string UserName { get; set; }
        public string ProfilePic { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public string Dob { get; set; }
        public string Gender { get; set; }

        public string ErrorMessage { get; set; }


        public void OnGet()
        {
            // Retrieve the email from session
            string sessionEmail = HttpContext.Session.GetString("email");

            // If session email is not null, fetch user data from the database
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string selectQuery = "SELECT * FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                Dob = reader["dob"].ToString();
                                Gender = reader["gender"].ToString();
                                ProfilePic = reader["profilepic"].ToString();
                                Email = sessionEmail; // Set email from session
                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }
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
                profilePic = Path.GetFileName(file.FileName);
                var fullPath = Path.Combine(uploads, profilePic);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            if (profilePic != null)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string updateQuery = "UPDATE User_data SET profilepic = @ProfilePic WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfilePic", profilePic);
                        cmd.Parameters.AddWithValue("@Email", HttpContext.Session.GetString("email"));

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                TempData["message"] = "Profile picture updated successfully.";
            }
            // Reload the updated data
            OnGet();

            return Page();
        }


        public async Task<IActionResult> OnPostUpdateOtherDetailAsync()
        {
            // Fetch the form data
            string newEmail = Request.Form["email"];
            string username = Request.Form["fullname"];
            string dob = Request.Form["dob"];
            string gender = Request.Form["gender"];

            // Debugging: Output values to the console/log
            Console.WriteLine($"Email: {newEmail}, Username: {username}, DOB: {dob}, Gender: {gender}");

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
                    cmd.Parameters.AddWithValue("@Email", HttpContext.Session.GetString("email"));

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    con.Close();

                    // Debugging: Check if the update was successful
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

            // Reload the updated data
            OnGet();

            TempData["message"] = "Details updated successfully.";
            return Page();
        }


        public async Task<IActionResult> OnPostUpdateEmailAsync()
        {
            string newEmail = Request.Form["email"].ToString();
            string currentEmail = HttpContext.Session.GetString("email");

            if (string.IsNullOrEmpty(currentEmail))
            {
                TempData["message"] = "Failed to retrieve your current email. Please try again.";
                return Page();
            }

            // Generate OTP
            string otp = GenerateOTP();

            // Send OTP via email to the current (old) email
            bool otpSent = await SendOtpToEmail(currentEmail, otp);

            if (otpSent)
            {
                // Store OTP and new email in session and redirect to verification page
                HttpContext.Session.SetString("NewEmail", newEmail);
                HttpContext.Session.SetString("Otp", otp);
                TempData["message"] = "OTP has been sent to your current email. Please verify.";
                return Redirect("/OtpVerification/VerifyOtp_for_change_email");
            }
            else
            {
                TempData["message"] = "Failed to send OTP. Please try again.";
                return Page();
            }
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        private async Task<bool> SendOtpToEmail(string email, string otp)
        {
            try
            {
                var emailBody = new StringBuilder();
                emailBody.AppendLine("Your OTP code is:");
                emailBody.AppendLine(otp);
                emailBody.AppendLine("Please enter this code to verify your email change request.");

                return await _emailService.SendEmailAsync(email, "OTP for Email Change", emailBody.ToString());
            }
            catch
            {
                ErrorMessage = "Failed to send OTP email. Please try again.";
                return false;
            }
        }

    }
}
