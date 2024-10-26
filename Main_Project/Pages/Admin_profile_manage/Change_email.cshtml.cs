using Main_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace Main_Project.Pages.Admin_profile_manage
{
    public class Change_emailModel : PageModel
    {
        private readonly IEmailService _emailService;
        private string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

        public Change_emailModel(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public string id { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public string OTP { get; set; }

        public async Task OnGetAsync()
        {
            await PopulateUserDataAsync();
        }

        public async Task<IActionResult> OnPostUpdateEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "Email is required.");
                await PopulateUserDataAsync();
                return Page();
            }

            // Generate OTP
            OTP = GenerateOtp();

            // Send OTP to the new email
            bool emailSent = await _emailService.SendOtpAsync(email, OTP);

            if (!emailSent)
            {
                ModelState.AddModelError("", "Failed to send OTP. Please try again.");
                await PopulateUserDataAsync();
                return Page();
            }

            // Store the new email and OTP in TempData for use in the verification page
            TempData["NewEmail"] = email;
            TempData["OTP"] = OTP;

            // Redirect to the OTP verification page
            return RedirectToPage("VerifyOtp_for_change_Admin_email");
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Generates a 6-digit OTP
        }

        private async Task PopulateUserDataAsync()
        {
            string sessionEmail = HttpContext.Session.GetString("email");
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
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
        }
    }
}
