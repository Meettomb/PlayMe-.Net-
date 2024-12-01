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
        public string subid { get; set; }
        public string UserRole { get; set; }
        public string userPlanName { get; set; }
        public bool subscriptionactive { get; set; }

        public Select_subscriptionModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public IActionResult OnGet()
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            if (!userId.HasValue)
            {
                return Redirect("/");
            }
            string sessionEmail = HttpContext.Session.GetString("email");

            // Fetch the user's username, profile picture, and subid from the database using their email
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
                                subid = reader["subid"].ToString();
                                subscriptionactive = Convert.ToBoolean(reader["subscriptionactive"]);

                            }
                        }
                    }
                }
            }

            // Fetch the plan name for the user's current subscription
            string userPlanName = string.Empty;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string selectQuery = "SELECT planename FROM subscription WHERE id = @Subid";
                using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Subid", subid);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userPlanName = reader["planename"].ToString();
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

            // Fetch and filter subscriptions based on the user's current plan
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string selectquery = "SELECT * FROM subscription"; // Fetch all subscriptions
                using (SqlCommand cmd = new SqlCommand(selectquery, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string planName = reader.GetString(4); // planename is in the 5th column (index 4)

                            if (subscriptionactive)
                            {
                                // Logic to filter subscriptions based on the user's current plan
                                if (userPlanName == "Regular" && planName != "Regular")
                                {
                                    subscription sub = new subscription
                                    {
                                        id = reader.GetInt32(0),
                                        price = reader.GetInt32(1),
                                        planeduration = reader.GetInt32(2),
                                        planedetail = reader.GetString(3), // planedetail is in the 4th column (index 3)
                                        planename = planName // Using the correct planename
                                    };
                                    subscription.Add(sub);
                                }
                                else if (userPlanName == "Gold" && planName != "Regular" && planName != "Gold")
                                {
                                    subscription sub = new subscription
                                    {
                                        id = reader.GetInt32(0),
                                        price = reader.GetInt32(1),
                                        planeduration = reader.GetInt32(2),
                                        planedetail = reader.GetString(3),
                                        planename = planName
                                    };
                                    subscription.Add(sub);
                                }
                                else if (userPlanName == "Premium" && planName != "Regular" && planName != "Gold" && planName != "Premium")
                                {
                                    subscription sub = new subscription
                                    {
                                        id = reader.GetInt32(0),
                                        price = reader.GetInt32(1),
                                        planeduration = reader.GetInt32(2),
                                        planedetail = reader.GetString(3),
                                        planename = planName
                                    };
                                    subscription.Add(sub);
                                }
                            }
                            else
                            {
                                subscription sub = new subscription
                                {
                                    id = reader.GetInt32(0),
                                    price = reader.GetInt32(1),
                                    planeduration = reader.GetInt32(2),
                                    planedetail = reader.GetString(3),
                                    planename = planName
                                };
                                subscription.Add(sub);
                            }
                        }
                    }
                }
            }


            return Page();
        }




        public async Task<IActionResult> OnPost()
        {

            string sessionEmail = HttpContext.Session.GetString("email");

            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    // Check if auto-renew is enabled for the user
                    string checkAutoRenewQuery = "SELECT autorenew, subid FROM User_data WHERE email = @Email";
                    using (SqlCommand checkAutoRenewCmd = new SqlCommand(checkAutoRenewQuery, con))
                    {
                        checkAutoRenewCmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (var reader = await checkAutoRenewCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                bool autoRenewStatus = Convert.ToBoolean(reader["autorenew"]);
                                int currentSubId = Convert.ToInt32(reader["subid"]);

                                if (autoRenewStatus)
                                {
                                    TempData["Message"] = "Your subscription is set to auto-renew. Please disable auto-renew to proceed.";
                                    return RedirectToPage("/User_Profile_manage/Subscription_detail");
                                }

                                /*if (int.TryParse(Request.Form["subid"], out int selectedSubId) && selectedSubId == currentSubId)
                                {
                                    TempData["Message"] = "You are already subscribed to this plan. Please select a different plan to upgrade.";
                                    return RedirectToPage("/User_Profile_manage/Subscription_detail");
                                }*/
                            }
                        }
                        con.Close();
                    }

                    // Continue with the rest of the code if auto-renew is not enabled and selected plan is different
                    User.datetime3 = Request.Form["datetime3"];
                    User.price3 = Request.Form["price3"];
                    User.duration3 = Request.Form["duration3"];
                    User.paymentmethod = Request.Form["paymentMethod"];

                    if (int.TryParse(Request.Form["subid"], out int subidValue))
                    {
                        User.subid = subidValue;
                    }
                    else
                    {
                        User.subid = null; // handle as needed
                    }

                    // Define the query to update User_data table
                    string updateUserDataQuery = @"
            UPDATE User_data 
            SET price = @Price, 
                datetime = @DateTime, 
                duration = @Duration, 
                paymentmethod = @PaymentMethod,
                subid = @subid,
                subscriptionactive = @SubscriptionActive,
                emailsent = @Emailsent,
                autorenew = @AutoRenew
            WHERE email = @Email";

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
                    }

                    using (SqlCommand cmd = new SqlCommand(updateUserDataQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        cmd.Parameters.AddWithValue("@Price", User.price3);
                        cmd.Parameters.AddWithValue("@DateTime", User.datetime3);
                        cmd.Parameters.AddWithValue("@Duration", User.duration3);
                        cmd.Parameters.AddWithValue("@PaymentMethod", User.paymentmethod);
                        cmd.Parameters.AddWithValue("@subid", User.subid);
                        cmd.Parameters.AddWithValue("@SubscriptionActive", true);
                        cmd.Parameters.AddWithValue("@Emailsent", false);
                        cmd.Parameters.AddWithValue("@AutoRenew", true);

                        con.Open();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand(insertRevenueQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        cmd.Parameters.AddWithValue("@Price", User.price3);
                        cmd.Parameters.AddWithValue("@DateTime", User.datetime3);
                        cmd.Parameters.AddWithValue("@Duration", User.duration3);
                        cmd.Parameters.AddWithValue("@PaymentMethod", User.paymentmethod);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@SubscriptionActive", true);

                        con.Open();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }

                    return RedirectToPage("/Renew_Subscription/Success");
                }
            }

            // If something goes wrong, stay on the page
            return Page();
        }



    }
}
