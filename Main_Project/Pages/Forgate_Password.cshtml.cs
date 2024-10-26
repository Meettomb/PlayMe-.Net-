using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Main_Project.Pages
{
    public class Forgate_PasswordModel : PageModel
    {
        private readonly string _connectionString = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string UserEmail { get; set; }

        [TempData]
        public string Message { get; set; }

        public void OnGet()
        {
            UserEmail = HttpContext.Session.GetString("UserEmailForReset");
        }

        public IActionResult OnPost()
        {
            UserEmail = HttpContext.Session.GetString("UserEmailForReset");
            if (string.IsNullOrEmpty(UserEmail))
            {
                Message = "No email found in session. Please try again.";
                return RedirectToPage("/Index");
            }

            // Check if passwords match
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return Page();
            }

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    // Hash the new password using PasswordHasher
                    var passwordHasher = new PasswordHasher<object>();
                    string hashedPassword = passwordHasher.HashPassword(null, NewPassword);

                    // Update the password in the database
                    string query = "UPDATE User_data SET password = @NewPassword WHERE email = @UserEmail";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@NewPassword", hashedPassword); // Store the hashed password
                        cmd.Parameters.AddWithValue("@UserEmail", UserEmail);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        con.Close();

                        if (rowsAffected > 0)
                        {
                            // Password updated successfully
                            TempData["Message"] = "Password updated successfully.";
                            return RedirectToPage("/Sign_in");
                        }
                        else
                        {
                            TempData["Message"] = "Password updated successfully.";
                            ModelState.AddModelError(string.Empty, "Failed to update password.");
                            return Page();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return Page();
            }
        }
    }
}
