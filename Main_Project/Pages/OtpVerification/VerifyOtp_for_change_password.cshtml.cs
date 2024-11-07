using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Main_Project.Pages.OtpVerification
{
    public class VerifyOtp_for_change_passwordModel : PageModel
    {
        [BindProperty]
        public string Otp { get; set; }

        public string Message { get; set; }
        public string NewEmail { get; set; }

        private readonly string _connectionString;
        public VerifyOtp_for_change_passwordModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public void OnGet()
        {
            // Fetch newEmail from session
            NewEmail = HttpContext.Session.GetString("NewEmail");

            // If NewEmail is null, show an error or handle it appropriately
            if (string.IsNullOrEmpty(NewEmail))
            {
                Message = "Email for password change not found.";
            }

            // Set EmailForPasswordChange session variable
            HttpContext.Session.SetString("EmailForPasswordChange", NewEmail);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string storedOtp = HttpContext.Session.GetString("Otp");
            string newEmail = HttpContext.Session.GetString("NewEmail");
            string sessionEmail = HttpContext.Session.GetString("email");

            if (string.IsNullOrEmpty(Otp))
            {
                Message = "OTP is required.";
                return Page();
            }

            if (Otp != storedOtp)
            {
                Message = "Invalid OTP.";
                return Page();
            }

            // If OTP is valid, redirect to the page to set a new password
            return RedirectToPage("/User_Profile_manage/Set_new_password");
        }
    }
}
