using Main_Project.Models;
using Main_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Netflix.Models;
using System.Data;
using System.Text;

namespace Main_Project.Pages.Renew_Subscription
{
    public class Select_subscriptionModel : PageModel
    {
        public List<subscription> subscription = new List<subscription>();

        public IList<MoviesTable> MoviesTable { get; set; } = new List<MoviesTable>();

        public IList<Movie_category_table> MovieCategories { get; set; } = new List<Movie_category_table>();



        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;


        [BindProperty]
        public user_regi User { get; set; }

        [BindProperty]
        public Revenue Revenue { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorMessagephone { get; set; }
        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public string UserRole { get; set; }


        public Select_subscriptionModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public void OnGet()
        {
            string sessionEmail = HttpContext.Session.GetString("email");

            // Fetch the user's username and profile picture from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                profilepic = reader["profilepic"].ToString();
                                email = sessionEmail; // Set email from session

                                UserRole = reader["role"].ToString();
                            }
                        }
                    }
                }
            }

            // Fetch movie categories
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Movie_category_table";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Movie_category_table category = new Movie_category_table
                            {
                                categoryid = dr.GetInt32(0),
                                moviecategory = dr.GetString(1)
                            };
                            MovieCategories.Add(category);
                        }
                    }
                }
            }

            // Fetch subscriptions
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
                            subscription sub = new subscription
                            {
                                id = reader.GetInt32(0),
                                price = reader.GetInt32(1),
                                planeduration = reader.GetInt32(2),
                                planedetail = reader.GetString(3)
                            };
                            subscription.Add(sub);
                        }
                    }
                }
            }
        }



        public async Task<IActionResult> OnPost()
        {
            // Simulate date and time for the subscription
            User.datetime3 = Request.Form["datetime3"]; // Ensure this is set correctly in JS
            User.price3 = Request.Form["price3"];
            User.duration3 = Request.Form["duration3"];
            User.paymentmethod = Request.Form["paymentMethod"];
            if (int.TryParse(Request.Form["subid"], out int subidValue))
            {
                User.subid = subidValue;
            }
            else
            {
                User.subid = null; // or handle the error as needed
            }

            // Retrieve the current user's email from session
            string sessionEmail = HttpContext.Session.GetString("email");

            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    // Define the query to update the User_data table
                    string updateUserDataQuery = @"
            UPDATE User_data 
            SET price = @Price, 
                datetime = @DateTime, 
                duration = @Duration, 
                paymentmethod = @PaymentMethod,
                subid = @subid,
                subscriptionactive = @SubscriptionActive,
                emailsent = @Emailsent
            WHERE email = @Email";

                    // Define the query to insert data into the Revenue table
                    string insertRevenueQuery = @"
            INSERT INTO Revenue (email, price1, datetime1, duration, paymentmethod, date, subscriptionactive)
            VALUES (@Email, @Price, @DateTime, @Duration, @PaymentMethod, @Date, @SubscriptionActive)";

                    // Send renewal notification email
                    var emailBody = new StringBuilder();
                    emailBody.AppendLine("Dear User,");
                    emailBody.AppendLine("Your subscription has been successfully renewed.");
                    emailBody.AppendLine("Thank you for continuing with us!");

                    try
                    {
                        await _emailService.SendEmailAsync(sessionEmail, "Subscription Renewal Notification", emailBody.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending renewal email: {ex.Message}");
                        // You may want to handle this exception in a user-friendly way
                    }

                    using (SqlCommand cmd = new SqlCommand(updateUserDataQuery, con))
                    {
                        // Update User_data table
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        cmd.Parameters.AddWithValue("@Price", User.price3);
                        cmd.Parameters.AddWithValue("@DateTime", User.datetime3); // Ensure datetime is formatted correctly
                        cmd.Parameters.AddWithValue("@Duration", User.duration3);
                        cmd.Parameters.AddWithValue("@PaymentMethod", User.paymentmethod);
                        cmd.Parameters.AddWithValue("@subid", User.subid);
                        cmd.Parameters.AddWithValue("@SubscriptionActive", true); // Assuming active after renewal
                        cmd.Parameters.AddWithValue("@Emailsent", false); 

                        con.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Insert into Revenue table
                    using (SqlCommand cmd = new SqlCommand(insertRevenueQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        cmd.Parameters.AddWithValue("@Price", User.price3);
                        cmd.Parameters.AddWithValue("@DateTime", User.datetime3);
                        cmd.Parameters.AddWithValue("@Duration", User.duration3);
                        cmd.Parameters.AddWithValue("@PaymentMethod", User.paymentmethod);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now); // Current date for the new transaction
                        cmd.Parameters.AddWithValue("@SubscriptionActive", true);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    con.Close();
                }

                // Redirect to a success page after both operations succeed
                return RedirectToPage("/Renew_Subscription/Success");
            }

            // If something goes wrong, stay on the page
            return Page();
        }



    }
}
