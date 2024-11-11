using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading.Tasks;
using Main_Project.Services;
using System.Text;

namespace Main_Project
{
    public class Subscription_Middleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public Subscription_Middleware(RequestDelegate next, IConfiguration configuration, IEmailService emailService)
        {
            _next = next;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Session.GetString("email") != null)
            {
                string userEmail = context.Session.GetString("email");
                string role = context.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userEmail) || role != "user")
                {
                    Console.WriteLine("User is not authenticated or not a regular user.");
                    await _next(context);
                    return; // Exit early if no valid user
                }

                bool subscriptionActive = false;
                DateTime registrationDate = DateTime.MinValue;
                int subscriptionDuration = 0;
                bool emailSent = false; // New flag to prevent sending multiple emails
                bool autoRenew = false; // New flag for auto-renew check
                int? subId = null;

                string connectionString = _configuration.GetConnectionString("NetflixDatabase") + ";MultipleActiveResultSets=True";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // First query to get user subscription details
                    string query = @"SELECT subscriptionactive, datetime, duration, emailsent, autorenew, subid 
                                     FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", userEmail);

                        // Use "using" block to ensure the reader is properly disposed of
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                subscriptionActive = Convert.ToBoolean(reader["subscriptionactive"]);
                                emailSent = Convert.ToBoolean(reader["emailsent"]); // Check if email has already been sent
                                autoRenew = reader["autorenew"] != DBNull.Value && Convert.ToBoolean(reader["autorenew"]);
                                subId = reader["subid"] != DBNull.Value ? Convert.ToInt32(reader["subid"]) : (int?)null;
                                string datetimeString = reader["datetime"].ToString();
                                string durationString = reader["duration"].ToString();

                                if (DateTime.TryParse(datetimeString, out DateTime parsedDate))
                                {
                                    registrationDate = parsedDate;
                                }

                                if (int.TryParse(durationString, out int parsedDuration))
                                {
                                    subscriptionDuration = parsedDuration;
                                }
                            }
                        }
                    }

                    if (registrationDate != DateTime.MinValue && subscriptionDuration > 0)
                    {
                        DateTime expirationDate = registrationDate.AddDays(subscriptionDuration);
                        DateTime currentDate = DateTime.Now;

                        if (currentDate > expirationDate)
                        {
                            // If subscription is expired and autorenew is enabled, renew subscription
                            if (autoRenew)
                            {
                                // Second query to fetch subscription details
                                string subQuery = "SELECT price, planeduration FROM subscription WHERE id = @SubId";
                                using (SqlCommand subCmd = new SqlCommand(subQuery, con))
                                {
                                    subCmd.Parameters.AddWithValue("@SubId", subId);

                                    // Ensure the previous DataReader is closed before starting another one
                                    using (SqlDataReader subReader = await subCmd.ExecuteReaderAsync())
                                    {
                                        if (await subReader.ReadAsync())
                                        {
                                            int newDuration = Convert.ToInt32(subReader["planeduration"]);
                                            int newPrice = Convert.ToInt32(subReader["price"]);

                                            // Update User_data table with new subscription details
                                            string updateQuery = @"UPDATE User_data SET 
                                                                    subscriptionactive = 1, 
                                                                    datetime = @CurrentDate, 
                                                                    subid = @SubId, 
                                                                    duration = @Duration, 
                                                                    price = @Price 
                                                                    WHERE email = @Email";
                                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                                            {
                                                // Format the datetime in the desired format
                                                string formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");

                                                updateCmd.Parameters.AddWithValue("@CurrentDate", formattedDateTime);
                                                updateCmd.Parameters.AddWithValue("@SubId", subId);
                                                updateCmd.Parameters.AddWithValue("@Duration", newDuration);
                                                updateCmd.Parameters.AddWithValue("@Price", newPrice);
                                                updateCmd.Parameters.AddWithValue("@Email", userEmail);
                                                await updateCmd.ExecuteNonQueryAsync();
                                            }

                                            // Send renewal confirmation email
                                            await SendSubscriptionRenewedEmail(userEmail);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // If autorenew is not enabled, expire the subscription
                                subscriptionActive = false;

                                using (SqlCommand updateCmd = new SqlCommand("UPDATE User_data SET subscriptionactive = 0 WHERE email = @Email", con))
                                {
                                    updateCmd.Parameters.AddWithValue("@Email", userEmail);
                                    await updateCmd.ExecuteNonQueryAsync();
                                }

                                // Send subscription expired email if not already sent
                                if (!emailSent)
                                {
                                    await SendSubscriptionExpirationEmail(userEmail);

                                    // Update database to set the emailSent flag to true
                                    using (SqlCommand emailSentCmd = new SqlCommand("UPDATE User_data SET emailsent = 1 WHERE email = @Email", con))
                                    {
                                        emailSentCmd.Parameters.AddWithValue("@Email", userEmail);
                                        await emailSentCmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }

                        context.Response.Headers.Add("Subscription-Status", subscriptionActive ? "active" : "expired");
                    }
                }
            }

            await _next(context);
        }

        private async Task SendSubscriptionExpirationEmail(string userEmail)
        {
            try
            {
                var emailBody = new StringBuilder();
                emailBody.AppendLine("Dear User,");
                emailBody.AppendLine("Your subscription has expired. Please renew your subscription to continue enjoying our services.");
                emailBody.AppendLine("Thank you for being with us!");

                await _emailService.SendEmailAsync(userEmail, "Subscription Expired", emailBody.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending subscription expired email: {ex.Message}");
            }
        }

        private async Task SendSubscriptionRenewedEmail(string userEmail)
        {
            try
            {
                var emailBody = new StringBuilder();
                emailBody.AppendLine("Dear User,");
                emailBody.AppendLine("Your subscription has been successfully renewed. You can continue enjoying our services.");
                emailBody.AppendLine("Thank you for being with us!");

                await _emailService.SendEmailAsync(userEmail, "Subscription Renewed", emailBody.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending subscription renewal email: {ex.Message}");
            }
        }
    }

    public static class Subscription_MiddlewareExtensions
    {
        public static IApplicationBuilder UseSubscription_Middleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Subscription_Middleware>();
        }
    }
}
