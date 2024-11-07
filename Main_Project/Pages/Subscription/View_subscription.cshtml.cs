using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;

namespace Main_Project.Pages.Subscription
{
    public class View_subscriptionModel : PageModel
    {
        public List<subscription> subscription = new List<subscription>();

        private readonly string _connectionString;
        public View_subscriptionModel(IConfiguration configuration)
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
                                profilepic = reader["profilepic"].ToString();
                                gender = reader["profilepic"].ToString();
                                dob = reader["profilepic"].ToString();
                                id = reader["id"].ToString();
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }

            // Get Subdcriptoin data
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string selectquery = "SELECT * FROM subscription";
                using (SqlCommand cmd = new SqlCommand(selectquery, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subscription sub = new subscription();
                            sub.id = reader.GetInt32(0);
                            sub.price = reader.GetInt32(1);
                            sub.planeduration = reader.GetInt32(2);
                            sub.planedetail = reader.GetString(3);
                            sub.planename = reader.GetString(4);
                            subscription.Add(sub);
                        }
                    }
                    connection.Close();
                }
            }

        }
    }
}
