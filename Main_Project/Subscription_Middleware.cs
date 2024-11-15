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
                    return;
                }

                bool subscriptionActive = false;
                DateTime registrationDate = DateTime.MinValue;
                int subscriptionDuration = 0;
                bool emailSent = false;
                bool autoRenew = false;
                int? subId = null;

                string connectionString = _configuration.GetConnectionString("NetflixDatabase") + ";MultipleActiveResultSets=True";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // Get user subscription details
                    string query = @"SELECT subscriptionactive, datetime, duration, emailsent, autorenew, subid 
                                     FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", userEmail);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                subscriptionActive = Convert.ToBoolean(reader["subscriptionactive"]);
                                emailSent = Convert.ToBoolean(reader["emailsent"]);
                                autoRenew = reader["autorenew"] != DBNull.Value && Convert.ToBoolean(reader["autorenew"]);
                                subId = reader["subid"] != DBNull.Value ? Convert.ToInt32(reader["subid"]) : (int?)null;
                                registrationDate = DateTime.TryParse(reader["datetime"].ToString(), out DateTime parsedDate) ? parsedDate : DateTime.MinValue;
                                subscriptionDuration = int.TryParse(reader["duration"].ToString(), out int parsedDuration) ? parsedDuration : 0;
                            }
                        }
                    }

                    if (registrationDate != DateTime.MinValue && subscriptionDuration > 0)
                    {
                        DateTime expirationDate = registrationDate.AddDays(subscriptionDuration);
                        DateTime currentDate = DateTime.Now;

                        if (currentDate > expirationDate)
                        {
                            if (autoRenew)
                            {
                                string planename = "";
                                string subQuery = "SELECT price, planeduration, planename FROM subscription WHERE id = @SubId";
                                using (SqlCommand subCmd = new SqlCommand(subQuery, con))
                                {
                                    subCmd.Parameters.AddWithValue("@SubId", subId);
                                    using (SqlDataReader subReader = await subCmd.ExecuteReaderAsync())
                                    {
                                        if (await subReader.ReadAsync())
                                        {
                                            int newDuration = Convert.ToInt32(subReader["planeduration"]);
                                            int newPrice = Convert.ToInt32(subReader["price"]);
                                            planename = subReader["planename"].ToString();

                                            // Disable auto-renew if plan name matches "1 Time in a Month" or "Regular"
                                            if (planename.Contains("1 Time in a Month") || planename.Equals("Regular", StringComparison.OrdinalIgnoreCase))
                                            {
                                                autoRenew = false;

                                                // Update the User_data table to set autorenew = 0 while keeping subscriptionactive = 1
                                                string disableAutoRenewQuery = @"UPDATE User_data 
                                                 SET autorenew = 0, subscriptionactive = 1 
                                                 WHERE email = @Email";
                                                using (SqlCommand disableAutoRenewCmd = new SqlCommand(disableAutoRenewQuery, con))
                                                {
                                                    disableAutoRenewCmd.Parameters.AddWithValue("@Email", userEmail);
                                                    await disableAutoRenewCmd.ExecuteNonQueryAsync();
                                                }
                                            }

                                            if (autoRenew)
                                            {
                                                // Update User_data table
                                                string updateQuery = @"UPDATE User_data SET 
                                                    subscriptionactive = 1, 
                                                    datetime = @CurrentDate, 
                                                    subid = @SubId, 
                                                    duration = @Duration, 
                                                    price = @Price 
                                                    WHERE email = @Email";
                                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                                                {
                                                    string formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");

                                                    updateCmd.Parameters.AddWithValue("@CurrentDate", formattedDateTime);
                                                    updateCmd.Parameters.AddWithValue("@SubId", subId);
                                                    updateCmd.Parameters.AddWithValue("@Duration", newDuration);
                                                    updateCmd.Parameters.AddWithValue("@Price", newPrice);
                                                    updateCmd.Parameters.AddWithValue("@Email", userEmail);
                                                    await updateCmd.ExecuteNonQueryAsync();
                                                }

                                                // Retrieve the last payment method
                                                string lastPaymentMethodQuery = "SELECT TOP 1 paymentmethod FROM Revenue WHERE email = @Email ORDER BY date DESC";
                                                string lastPaymentMethod = "Previsa"; // Default method

                                                using (SqlCommand lastPaymentMethodCmd = new SqlCommand(lastPaymentMethodQuery, con))
                                                {
                                                    lastPaymentMethodCmd.Parameters.AddWithValue("@Email", userEmail);
                                                    using (SqlDataReader paymentReader = await lastPaymentMethodCmd.ExecuteReaderAsync())
                                                    {
                                                        if (await paymentReader.ReadAsync())
                                                        {
                                                            lastPaymentMethod = paymentReader["paymentmethod"].ToString();
                                                        }
                                                    }
                                                }

                                                // Insert into Revenue table
                                                string insertRevenueQuery = @"INSERT INTO Revenue 
                                                  (email, price1, datetime1, duration, paymentmethod, date, subscriptionactive) 
                                                  VALUES (@Email, @Price, @CurrentDate, @Duration, @PaymentMethod, @Date, @SubscriptionActive)";
                                                using (SqlCommand insertRevenueCmd = new SqlCommand(insertRevenueQuery, con))
                                                {
                                                    string formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");

                                                    insertRevenueCmd.Parameters.AddWithValue("@Email", userEmail);
                                                    insertRevenueCmd.Parameters.AddWithValue("@Price", newPrice);
                                                    insertRevenueCmd.Parameters.AddWithValue("@CurrentDate", formattedDateTime);
                                                    insertRevenueCmd.Parameters.AddWithValue("@Duration", newDuration);
                                                    insertRevenueCmd.Parameters.AddWithValue("@PaymentMethod", lastPaymentMethod);
                                                    insertRevenueCmd.Parameters.AddWithValue("@Date", DateTime.Now.Date);
                                                    insertRevenueCmd.Parameters.AddWithValue("@SubscriptionActive", true);

                                                    await insertRevenueCmd.ExecuteNonQueryAsync();
                                                }

                                                // Send renewal confirmation email
                                                await SendSubscriptionRenewedEmail(userEmail);
                                            }
                                            else
                                            {
                                                // Directly expire subscription if auto-renew is disabled
                                                await ExpireSubscription(con, userEmail, emailSent);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Expire subscription if auto-renew is not enabled
                                await ExpireSubscription(con, userEmail, emailSent);
                            }
                        }

                        context.Response.Headers.Add("Subscription-Status", subscriptionActive ? "active" : "expired");
                    }
                }
            }

            await _next(context);
        }

        private async Task ExpireSubscription(SqlConnection con, string userEmail, bool emailSent)
        {
            using (SqlCommand updateCmd = new SqlCommand("UPDATE User_data SET subscriptionactive = 0 WHERE email = @Email", con))
            {
                updateCmd.Parameters.AddWithValue("@Email", userEmail);
                await updateCmd.ExecuteNonQueryAsync();
            }

            if (!emailSent)
            {
                await SendSubscriptionExpirationEmail(userEmail);

                using (SqlCommand emailSentCmd = new SqlCommand("UPDATE User_data SET emailsent = 1 WHERE email = @Email", con))
                {
                    emailSentCmd.Parameters.AddWithValue("@Email", userEmail);
                    await emailSentCmd.ExecuteNonQueryAsync();
                }
            }
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
                emailBody.AppendLine("Your subscription has been successfully renewed.");
                emailBody.AppendLine("Thank you for being with us!");

                await _emailService.SendEmailAsync(userEmail, "Subscription Renewed", emailBody.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending subscription renewed email: {ex.Message}");
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
