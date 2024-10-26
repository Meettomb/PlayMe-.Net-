using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace Main_Project.Pages.OtpVerification
{
    public class VerifyOtpModel : PageModel
    {
        [BindProperty]
        public string Otp { get; set; }

        public string Message { get; set; }

        public string UserEmail { get; set; }

        public IActionResult OnGet()
        {
            // Retrieve email from session
            UserEmail = HttpContext.Session.GetString("UserEmailForReset");

            // No need to initialize or refresh OTP creation time
            HttpContext.Session.SetString("OTPCreationTime", DateTime.Now.ToString());
            return Page();
        }

        public IActionResult OnPost()
        {
            var storedOtp = HttpContext.Session.GetString("OTP");

            if (string.IsNullOrEmpty(storedOtp))
            {
                Message = "OTP session has expired. Please request a new OTP.";
                return Page();
            }

            if (Otp == storedOtp)
            {
                // OTP is correct, redirect to the reset password page
                return RedirectToPage("/Forgate_Password"); // Replace with your actual reset password page
            }
            else
            {
                // OTP is incorrect
                Message = "Invalid OTP. Please try again.";
                return Page();
            }
        }
    }
}
