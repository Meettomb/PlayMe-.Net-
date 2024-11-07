using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Netflix.Models;
using System.Data;

namespace Main_Project.Pages.Subscription
{
    public class Edit_subscriptionModel : PageModel
    {
        public subscription subscription = new subscription();

        private readonly string _connectionString;
        public Edit_subscriptionModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public string UserName { get; set; }
        public string id { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public void OnGet()
        {
            string sessionEmail = HttpContext.Session.GetString("email");

            // Declare id variable before using it
            int id = 0;  // or you can use nullable int like int? id = null;

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
                                profilepic = reader["profilepic"].ToString();
                                gender = reader["profilepic"].ToString();
                                dob = reader["profilepic"].ToString();
                                id = Convert.ToInt32(reader["id"]);  // Ensure id is set as an integer
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }



            // Fetch Data for Edit Subscription

            int SubscriptionId = Convert.ToInt32(Request.Query["id"].ToString());
            DataSet ds = new DataSet();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string fetchquery = "SELECT * FROM subscription WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(fetchquery, connection))
                {
                    cmd.Parameters.AddWithValue("@id", SubscriptionId);  // Correct variable used here

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        connection.Open();
                        da.Fill(ds);
                        connection.Close();
                    }
                }
            }

            // Populate the subscription object if data is fetched
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                subscription.id = Convert.ToInt32(row["id"]);
                subscription.price = Convert.ToInt32(row["price"]);
                subscription.planeduration = Convert.ToInt32(row["planeduration"]);
                subscription.planedetail = row["planedetail"].ToString();
                subscription.planename = row["planename"].ToString();
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

                // Ensure the id is being set from the hidden field
                if (!int.TryParse(Request.Form["id"], out int id))
                {
                    ModelState.AddModelError(string.Empty, "Invalid subscription ID.");
                    return Page();
                }

                // SQL Query
                string updateQuery = "UPDATE subscription SET price = @price, planeduration = @planeduration, planedetail = @planedetail, planename = @planename WHERE id = @id";

                // Using SqlConnection
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();  // Open the connection

                    // Using SqlCommand
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        // Add parameters with the values from form inputs
                        cmd.Parameters.AddWithValue("@price", subscription.price);
                        cmd.Parameters.AddWithValue("@planeduration", subscription.planeduration);
                        cmd.Parameters.AddWithValue("@planedetail", subscription.planedetail);
                        cmd.Parameters.AddWithValue("@planename", subscription.planename);
                        cmd.Parameters.AddWithValue("@id", id);  // Use the parsed id

                        // Execute the SQL query
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();  // Perform the update operation

                        if (rowsAffected == 0)
                        {
                            ModelState.AddModelError(string.Empty, "No record was updated. Please check if the subscription exists.");
                            return Page();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                ModelState.AddModelError(string.Empty, "An error occurred while saving the data: " + ex.Message);
                return Page();  // Return the page to display the error
            }

            return RedirectToPage("/Subscription/View_subscription");  // Redirect to a page that lists subscriptions after successful update
        }


    }
}
