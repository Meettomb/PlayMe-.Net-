using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Data;

namespace Main_Project.Pages.Subscription
{
    public class Delete_subscriptionModel : PageModel
    {

        public subscription subscription = new subscription();

        string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";
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
                using (SqlConnection con = new SqlConnection(connectionstring))
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
                                profilepic = reader["username"].ToString();
                                gender = reader["username"].ToString();
                                dob = reader["username"].ToString();
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

            using (SqlConnection connection = new SqlConnection(connectionstring))
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
            }


        }


        public IActionResult OnPost()
        {
            // Retrieve the id from the form and convert it to an integer
            if (!int.TryParse(Request.Form["id"], out int id) || id <= 0)
            {
                return RedirectToPage("/Subscription/View_subscription");
            }

            string connectionString = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "DELETE FROM subscription WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        // Log or handle the case where no rows were affected
                        Console.WriteLine($"No rows were deleted for FeedbackId: {id}");
                    }
                }

                con.Close();
            }

            return RedirectToPage("/Subscription/View_subscription");
        }


    }
}
