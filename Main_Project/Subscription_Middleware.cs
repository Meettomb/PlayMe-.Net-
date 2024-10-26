using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Text;
using Main_Project.Services;

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

                string connectionString = _configuration.GetConnectionString("NetflixDatabase");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    string query = @"SELECT subscriptionactive, datetime, duration, emailsent FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", userEmail);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                subscriptionActive = Convert.ToBoolean(reader["subscriptionactive"]);
                                emailSent = Convert.ToBoolean(reader["emailsent"]); // Check if email has already been sent
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
                            subscriptionActive = false;

                            using (SqlCommand updateCmd = new SqlCommand("UPDATE User_data SET subscriptionactive = 0 WHERE email = @Email", con))
                            {
                                updateCmd.Parameters.AddWithValue("@Email", userEmail);
                                await updateCmd.ExecuteNonQueryAsync();
                            }

                            if (!emailSent) // Only send email if it hasn't been sent yet
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
    }

    public static class Subscription_MiddlewareExtensions
    {
        public static IApplicationBuilder UseSubscription_Middleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Subscription_Middleware>();
        }
    }
}
