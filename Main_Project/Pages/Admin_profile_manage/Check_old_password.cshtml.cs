using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using static System.Net.WebRequestMethods;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Main_Project.Services;
using Microsoft.AspNetCore.Identity;

namespace Main_Project.Pages.Admin_profile_manage
{
    public class Check_old_passwordModel : PageModel
    {
        public List<user_regi> userlist = new List<user_regi>();
        string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";
        private readonly IEmailService _emailService; 

        public Check_old_passwordModel(IEmailService emailService) 
        {
            _emailService = emailService;
        }
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public string id { get; set; }
        public string OTP { get; set; } 
        [BindProperty]
        public string Oldpassword { get; set; }
        public void OnGet()
        {
            string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    string query = "SELECT id, username, dob, gender, profilepic FROM User_data WHERE email = @Email";
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
                                id = reader["id"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }
        }


        public async Task<IActionResult> OnPost()
        {
            string sessionEmail = HttpContext.Session.GetString("email");
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    string query = "SELECT password FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();

                        // Retrieve the hashed password from the database
                        string hashedPassword = cmd.ExecuteScalar()?.ToString();

                        // Ensure password is retrieved
                        if (!string.IsNullOrEmpty(hashedPassword))
                        {
                            var passwordHasher = new PasswordHasher<object>();

                            // Verify the old password using the password hasher
                            var result = passwordHasher.VerifyHashedPassword(null, hashedPassword, Oldpassword);

                            if (result == PasswordVerificationResult.Success)
                            {
                                // Passwords match, generate OTP and redirect
                                OTP = GenerateOTP();
                                HttpContext.Session.SetString("OTP", OTP);

                                bool isOtpSent = await SendOtpToEmail(sessionEmail, OTP);
                                if (isOtpSent)
                                {
                                    return RedirectToPage("/OtpVerification/VerifyOtp_for_Admin_password_change");
                                }
                                else
                                {
                                    ModelState.AddModelError("Email", "Failed to send OTP. Please try again.");
                                }
                            }
                            else
                            {
                                // Add error message if password doesn't match
                                ModelState.AddModelError("Oldpassword", "Incorrect old password.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("Email", "Failed to retrieve password from database.");
                        }
                    }
                }
            }
            return Page();
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
                emailBody.AppendLine("Please enter this code to complete your password change.");

                // Use your email service to send the email
                return await _emailService.SendEmailAsync(email, "OTP for Password Change", emailBody.ToString());
            }
            catch
            {
                return false;
            }
        }

    }
}
