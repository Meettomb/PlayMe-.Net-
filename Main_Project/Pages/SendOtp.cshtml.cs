using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Main_Project.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace Main_Project.Pages
{
    public class SendOtpModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly string _connectionString;
    

        public SendOtpModel(IEmailService emailService, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
            _emailService = emailService;
        }

        [BindProperty]
        public string Email { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return Page();
            }

            if (!IsEmailExist(Email))
            {
                ModelState.AddModelError(string.Empty, "Email not found."); // Set error message for email not found
                return Page();
            }

            var otp = new Random().Next(100000, 999999).ToString();

            try
            {
                var emailSent = await _emailService.SendEmailAsync(Email, "Your OTP Code", $"Your OTP code is {otp}");

                if (!emailSent)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send OTP. Please try again later.");
                    return Page();
                }

                HttpContext.Session.SetString("UserEmailForReset", Email);
                HttpContext.Session.SetString("OTP", otp);

                return RedirectToPage("/OtpVerification/VerifyOtp");
            }
            catch (Exception ex)
            {
                // Log the exception or debug it
                ModelState.AddModelError(string.Empty, $"Failed to send OTP: {ex.Message}");
                return Page();
            }
        }


        private bool IsEmailExist(string email)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(1) FROM User_data WHERE email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    con.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                    return count > 0;
                }
            }
        }
    }
}
