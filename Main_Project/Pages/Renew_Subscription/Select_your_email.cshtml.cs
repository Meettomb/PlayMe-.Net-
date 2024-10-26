using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient; // Updated namespace for SQL Client
using System.Data;

namespace Main_Project.Pages.Renew_Subscription
{
    public class Select_your_emailModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        // Constructor to inject IConfiguration and set the connection string
        public Select_your_emailModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("NetflixDatabase");
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return Page();
            }

            // Check if email exists in the User_data table
            bool userExists = CheckIfEmailExists(Email);

            if (userExists)
            {
                // Redirect to Select_subscription page with the email as a query parameter
                return RedirectToPage("/Renew_Subscription/Select_subscription", new { email = Email });
            }
            else
            {
                // If email not found, show error
                ModelState.AddModelError(string.Empty, "Email not found. Please enter a valid email.");
                return Page();
            }
        }

        private bool CheckIfEmailExists(string email)
        {
            bool exists = false;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM User_data WHERE email = @Email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    int count = (int)command.ExecuteScalar();
                    exists = count > 0;
                }
            }

            return exists;
        }
    }
}
