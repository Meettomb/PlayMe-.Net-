using Main_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace Main_Project.Pages
{
    public class Change_password_pageModel : PageModel
    {
        private readonly IEmailService _emailService;

        private readonly ILogger<Change_password_pageModel> _logger;
        private readonly string _connectionString;

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Message { get; set; }
        public string UserName { get; set; }
        public string id { get; set; }
        public string UserRole { get; set; }


        public Change_password_pageModel(IEmailService emailService, ILogger<Change_password_pageModel> logger, IConfiguration configuration)
        {
            _emailService = emailService;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
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
                                id = reader["id"].ToString();
                                Email = sessionEmail; // Set email from session

                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                Message = "Email or password cannot be empty.";
                return Page();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string selectQuery = "SELECT password FROM User_data WHERE email = @Email";
                using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPasswordHash = reader["password"].ToString();
                            // Use PasswordHasher to verify the password
                            var passwordHasher = new PasswordHasher<object>();
                            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, storedPasswordHash, Password);


                            if (passwordVerificationResult == PasswordVerificationResult.Success)
                            {
                                // Generate OTP
                                string otp = GenerateOTP();

                                // Send OTP via email
                                bool otpSent = await SendOtpToEmail(Email, otp);

                                if (otpSent)
                                {
                                    // Store OTP and redirect to verification page
                                    HttpContext.Session.SetString("Otp", otp);
                                    HttpContext.Session.SetString("NewEmail", Email);
                                    Message = "OTP has been sent to your email. Please verify.";
                                    return RedirectToPage("/OtpVerification/VerifyOtp_for_change_password");
                                }
                                else
                                {
                                    Message = "Failed to send OTP. Please try again.";
                                    return Page();
                                }
                            }
                            else
                            {
                                Message = "Incorrect password. Please try again.";
                                return Page();
                            }
                        }
                        else
                        {
                            Message = "Email not found.";
                            return Page();
                        }
                    }
                }
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
                emailBody.AppendLine("Please enter this code to complete your password change.");

                return await _emailService.SendEmailAsync(email, "OTP for Password Change", emailBody.ToString());
            }
            catch
            {
                return false;
            }
        }
    }
}
