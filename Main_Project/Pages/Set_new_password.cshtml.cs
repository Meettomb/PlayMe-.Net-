using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Main_Project.Pages
{
    public class Set_new_passwordModel : PageModel
    {
        public string Message { get; set; }

        private readonly string connectionString = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string sessionEmail = HttpContext.Session.GetString("EmailForPasswordChange");

            if (string.IsNullOrEmpty(sessionEmail) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                Message = "All fields are required.";
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                Message = "Passwords do not match.";
                return Page();
            }

            // Hash the new password
            var passwordHasher = new PasswordHasher<object>();
            string hashedPassword = passwordHasher.HashPassword(null, NewPassword);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string updateQuery = "UPDATE User_data SET password = @NewPassword WHERE email = @Email";
                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@NewPassword", hashedPassword);
                    cmd.Parameters.AddWithValue("@Email", sessionEmail);

                    try
                    {
                        con.Open();
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            TempData["Message"] = "Your password has been changed successfully.";
                            HttpContext.Session.Remove("EmailForPasswordChange");
                            HttpContext.Session.Remove("Otp");
                            return RedirectToPage("/home");
                        }
                        else
                        {
                            Message = "Failed to update password. Email may not exist.";
                        }
                    }
                    catch (SqlException ex)
                    {
                        Message = $"An error occurred: {ex.Message}";
                    }
                }
            }

            return Page();
        }



    }
}
