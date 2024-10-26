using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace Main_Project.Pages.OtpVerification
{
    public class VerifyOtp_for_change_Admin_emailModel : PageModel
    {
        private readonly string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPostVerifyOtp()
        {
            string enteredOtp = Request.Form["otp"];
            string sessionOtp = HttpContext.Session.GetString("Otp");
            string newEmail = HttpContext.Session.GetString("NewEmail");

            if (enteredOtp == sessionOtp)
            {
                // Update email in the database
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    string updateQuery = "UPDATE User_data SET email = @NewEmail WHERE email = @OldEmail";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@NewEmail", newEmail);
                        cmd.Parameters.AddWithValue("@OldEmail", HttpContext.Session.GetString("email"));

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                // Update session email
                HttpContext.Session.SetString("email", newEmail);
                TempData["message"] = "Email updated successfully.";
                return RedirectToPage("/Manage_profile");
            }
            else
            {
                ErrorMessage = "Incorrect OTP. Please try again.";
                return Page();
            }
        }
    }
}
