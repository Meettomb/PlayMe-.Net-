using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Main_Project.Models;
using Microsoft.Extensions.Options;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Netflix.Models;
using System.Text;
using Main_Project.Services;
using Microsoft.AspNetCore.Identity;

namespace Main_Project.Pages.OtpVerification
{
    public class VerrifyOtp_for_SignUpModel : PageModel
    {
        private readonly string _connectionString;
        private readonly IEmailService _emailService;
        private readonly PasswordHasher<user_regi> _passwordHasher;

        public VerrifyOtp_for_SignUpModel(IConfiguration configuration, IEmailService emailService)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
            _emailService = emailService; // Properly initialize the service
            _passwordHasher = new PasswordHasher<user_regi>(); // Initialize PasswordHasher
        }

        [BindProperty]
        public string Otp { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Retrieve the user from TempData
            var userJson = TempData["User"] as string;

            // Check if userJson is null or empty
            if (string.IsNullOrEmpty(userJson))
            {
                ErrorMessage = "User data not found. Please try again.";
                Console.WriteLine(ErrorMessage); // Print to console
                return Page();
            }

            var user = JsonConvert.DeserializeObject<user_regi>(userJson);
            var storedOtp = TempData["Otp"] as string;

            // Check if the OTP is valid
            if (string.IsNullOrEmpty(storedOtp))
            {
                ErrorMessage = "OTP data not found. Please try again.";
                Console.WriteLine(ErrorMessage); // Print to console
                return Page();
            }

            if (Otp != storedOtp)
            {
                ErrorMessage = "Invalid OTP. Please try again.";
                Console.WriteLine(ErrorMessage); // Print to console
                return Page();
            }

            // Check for required user data
            if (user == null || string.IsNullOrEmpty(user.fullname) || string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                ErrorMessage = "User data is incomplete. Please check your details.";
                Console.WriteLine(ErrorMessage); // Print to console
                return Page();
            }

            // Hash the password before saving to the database
            user.password = _passwordHasher.HashPassword(user, user.password); // Hash the password

            Console.WriteLine($"User Data: Full Name: {user.fullname}, Email: {user.email}, Phone: {user.phone}, " +
                              $"DOB: {user.dob}, Price: {user.price3}, Subscription Active: {user.subscriptionactive}");

            // Check if connection string is valid
            if (string.IsNullOrEmpty(_connectionString))
            {
                ErrorMessage = "Database connection string not found.";
                Console.WriteLine(ErrorMessage); // Print to console
                return Page();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // Insert User_data
                    string insertUserQuery = @"
                INSERT INTO User_data (username, email, phone, password, isactive, price, datetime, 
                                       duration, paymentmethod, role, logintime, profilepic, dob, 
                                       subscriptionactive, date, emailsent, subid, autorenew)
                VALUES (@fullname, @Email, @Phone, @Password, @isactive, @Price, @Datetime, @Duration, 
                        @Paymentmethod, @Role, @Logintime, @Profilepic, @Dob, @Subscriptionactive, 
                        @Date, @Emailsent, @subid, @autorenew)";

                    using (SqlCommand cmd = new SqlCommand(insertUserQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@fullname", user.fullname);
                        cmd.Parameters.AddWithValue("@Email", user.email);
                        cmd.Parameters.AddWithValue("@Phone", user.phone);
                        cmd.Parameters.AddWithValue("@Password", user.password);
                        cmd.Parameters.AddWithValue("@isactive", user.isactive);
                        cmd.Parameters.AddWithValue("@Price", user.price3);
                        cmd.Parameters.AddWithValue("@Datetime", user.datetime3);
                        cmd.Parameters.AddWithValue("@Duration", user.duration3);
                        cmd.Parameters.AddWithValue("@Paymentmethod", user.paymentmethod);
                        cmd.Parameters.AddWithValue("@Role", user.role);
                        cmd.Parameters.AddWithValue("@Logintime", user.logintime);
                        cmd.Parameters.AddWithValue("@Profilepic", user.profilepic);
                        cmd.Parameters.AddWithValue("@Dob", user.dob);
                        cmd.Parameters.AddWithValue("@Subscriptionactive", user.subscriptionactive);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Emailsent", user.emailsent);
                        cmd.Parameters.AddWithValue("@subid", user.subid);
                        cmd.Parameters.AddWithValue("@autorenew", user.autorenew);

                        Console.WriteLine("Executing Insert User Query with parameters:");
                        foreach (SqlParameter parameter in cmd.Parameters)
                        {
                            Console.WriteLine($"{parameter.ParameterName}: {parameter.Value}");
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Insert into Revenue table
                    string insertRevenueQuery = @"
                INSERT INTO Revenue (email, price1, datetime1, duration, paymentmethod, date, subscriptionactive) 
                VALUES (@Email, @Price, @Datetime, @Duration, @Paymentmethod, @Date, @Subscriptionactive)";

                    using (SqlCommand cmd = new SqlCommand(insertRevenueQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.email);
                        cmd.Parameters.AddWithValue("@Price", user.price3);
                        cmd.Parameters.AddWithValue("@Datetime", user.datetime3);
                        cmd.Parameters.AddWithValue("@Duration", user.duration3);
                        cmd.Parameters.AddWithValue("@Paymentmethod", user.paymentmethod);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Subscriptionactive", user.subscriptionactive);

                        Console.WriteLine("Executing Insert Revenue Query with parameters:");
                        foreach (SqlParameter parameter in cmd.Parameters)
                        {
                            Console.WriteLine($"{parameter.ParameterName}: {parameter.Value}");
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Commit transaction
                    transaction.Commit();
                    Console.WriteLine("Transaction committed successfully.");

                    // Send invoice email
                    var emailBody = new StringBuilder();
                    emailBody.AppendLine("Congratulations! Your Registration is Confirmed. Here is your Invoice:");
                    emailBody.AppendLine("");
                    emailBody.AppendLine($"Full Name: {user.fullname}");
                    emailBody.AppendLine($"Email: {user.email}");
                    emailBody.AppendLine($"Phone: {user.phone}");
                    emailBody.AppendLine($"Date Of Birth: {user.dob}");
                    emailBody.AppendLine($"Price: ₹{user.price3}");
                    emailBody.AppendLine($"Date of Active Subscription: {user.datetime3}");
                    emailBody.AppendLine($"Subscription Duration: {user.duration3}");
                    emailBody.AppendLine($"Payment Method: {user.paymentmethod}");
                    emailBody.AppendLine("Thank You");

                    try
                    {
                        await _emailService.SendEmailAsync(user.email, "Registration Confirmation & Invoice", emailBody.ToString());
                        Console.WriteLine("Invoice email sent successfully.");
                    }
                    catch (Exception ex)
                    {
                        // Log error or handle failure to send email
                        ErrorMessage = "Failed to send confirmation email: " + ex.Message;
                        Console.WriteLine(ErrorMessage); // Print to console
                        return Page();
                    }

                    // Redirect to success page
                    return RedirectToPage("/Success_page");
                }
                catch (SqlException ex)
                {
                    // Rollback transaction and log error
                    transaction.Rollback();
                    ErrorMessage = "An error occurred while saving data to the database: " + ex.Message;
                    Console.WriteLine(ErrorMessage); // Print to console
                    return Page();
                }
                finally
                {
                    await con.CloseAsync();
                }
            }
        }
    }
}
