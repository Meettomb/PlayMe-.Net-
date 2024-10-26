using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;


namespace Main_Project.Pages.Admin_profile_manage
{
    public class Create_new_passwordModel : PageModel
    {
        string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public IActionResult OnPost()
        {
            // Check if the passwords match
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return Page();
            }

            string sessionEmail = HttpContext.Session.GetString("email");
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    // Hash the new password
                    var passwordHasher = new PasswordHasher<object>();
                    string hashedPassword = passwordHasher.HashPassword(null, NewPassword); // Hash the password

                    // Prepare the SQL command to update the password
                    string query = "UPDATE User_data SET password = @Password WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Password", hashedPassword); // Store the hashed password
                        cmd.Parameters.AddWithValue("@Email", sessionEmail); // Set the email parameter

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery(); // Get the number of affected rows

                        if (rowsAffected > 0)
                        {
                            // Password updated successfully
                            return Redirect("/Deshbord"); // Redirect to Dashboard
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to update password. Please try again.");
                        }
                    }
                }
            }

            return Page();
        }
    }
}
