using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Main_Project.Pages.OtpVerification
{
    public class VerifyOtp_for_Admin_password_changeModel : PageModel
    {
        [BindProperty]
        public string Otp { get; set; }

        public IActionResult OnPost()
        {
            string sessionOtp = HttpContext.Session.GetString("OTP");

            if (Otp == sessionOtp)
            {
                // OTP verified, redirect to create new password page
                return RedirectToPage("/Admin_profile_manage/Create_new_password");
            }
            else
            {
                ModelState.AddModelError("Otp", "Invalid OTP. Please try again.");
                return Page();
            }
        }
    }
}
