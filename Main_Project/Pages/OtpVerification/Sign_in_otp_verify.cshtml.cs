using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Main_Project.Pages.OtpVerification
{
    public class Sign_in_otp_verifyModel : PageModel
    {
        [BindProperty]
        public string Otp { get; set; }
        public string ModelState { get; set; }

        private readonly IConfiguration _configuration;

        public Sign_in_otp_verifyModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Otp))
            {
                ModelState = "OTP is required.";
                return Page();
            }

            string sessionOtp = HttpContext.Session.GetString("Otp");
            if (sessionOtp != Otp)
            {
                ModelState = "Invalid OTP.";
                return Page();
            }

            // Clear the OTP from session after successful verification
            HttpContext.Session.Remove("Otp");

            // Get user ID from session
            int? userId = HttpContext.Session.GetInt32("Id");
            if (userId == null)
            {
                ModelState = "User ID not found in session.";
                return Page();
            }

            // Update logintime field to false in the database
            string connectionString = _configuration.GetConnectionString("NetflixDatabase");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string updateQuery = "UPDATE user_data SET logintime = @Logintime WHERE id = @Id";
                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Logintime", false);
                    cmd.Parameters.AddWithValue("@Id", userId);

                    con.Open();
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }

            // Redirect based on user role
            string role = HttpContext.Session.GetString("UserRole");
            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Deshbord");
            }
            else if (role.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Home");
            }
            else
            {
                ModelState = "Invalid role.";
                return Page();
            }
        }
    }
}
