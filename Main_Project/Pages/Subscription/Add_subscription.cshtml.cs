using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Data;

namespace Main_Project.Pages.Subscription
{
    public class Add_subscriptionModel : PageModel
    {
        public subscription subscription = new subscription();

        private readonly string _connectionString;
        public Add_subscriptionModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public string UserName { get; set; }
        public string id { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public void OnGet()
        {
            // Retrieve the email from session
            string sessionEmail = HttpContext.Session.GetString("email");

            // If session email is not null, fetch user data from the database
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string selectQuery = "SELECT * FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
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


        public async Task<IActionResult> OnPost()
        {
            try
            {
                // Parsing form values
                subscription.price = int.Parse(Request.Form["price"]);
                subscription.planeduration = int.Parse(Request.Form["planeduration"]);
                subscription.planedetail = Request.Form["planedetail"];
                subscription.planename = Request.Form["planename"];

                // SQL Query
                string insertquery = "INSERT INTO subscription (price, planeduration, planedetail, planename) VALUES (@price, @planeduration, @planedetail, @planename)";

                // Using SqlConnection
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();  // Open the connection

                    // Using SqlCommand
                    using (SqlCommand cmd = new SqlCommand(insertquery, connection))
                    {
                        // Add parameters with the values from form inputs
                        cmd.Parameters.AddWithValue("@price", subscription.price);
                        cmd.Parameters.AddWithValue("@planeduration", subscription.planeduration);
                        cmd.Parameters.AddWithValue("@planedetail", subscription.planedetail);
                        cmd.Parameters.AddWithValue("@planename", subscription.planename);

                        // Execute the SQL query
                        await cmd.ExecuteNonQueryAsync();  // Perform the insert operation
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                ModelState.AddModelError(string.Empty, "An error occurred while saving the data: " + ex.Message);
                return Page();  // Return the page to display the error
            }

            return Page();  // Redirect to a page that lists subscriptions after successful insert
        }

    }
}
