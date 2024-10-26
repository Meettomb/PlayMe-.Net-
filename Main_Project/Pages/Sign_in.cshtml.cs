using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace Netflix.Pages
{
    public class Sign_inModel : PageModel
    {
        [BindProperty]
        public user_regi User { get; set; }

        private readonly IConfiguration _configuration;
        private static Random random = new Random();

        public Sign_inModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int? UserId { get; set; } // Property to hold the User ID

        public IActionResult OnGet()
        {
            // Retrieve user ID from session and store it in the UserId property
            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {
                // If the user is logged in (UserId exists), redirect to the Home page
                return RedirectToPage("/Home");
            }

            // If the user is not logged in (UserId does not exist), stay on the Index page
            return Page();
        }



        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(User.email) || string.IsNullOrEmpty(User.password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("NetflixDatabase");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT id, dob, gender, username, role, profilepic, email, isactive, logintime, 
                subscriptionactive, datetime, duration, password 
                FROM user_data 
                WHERE email = @Email AND isactive = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", User.email);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();

                    con.Open();
                    da.Fill(ds);
                    con.Close();

                    if (ds.Tables[0].Rows.Count >= 1)
                    {
                        DataRow row = ds.Tables[0].Rows[0];
                        string storedHashedPassword = row["password"].ToString();

                        // Use PasswordHasher to verify the password
                        var passwordHasher = new PasswordHasher<object>();
                        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, storedHashedPassword, User.password);

                        if (passwordVerificationResult == PasswordVerificationResult.Success)
                        {
                            int id = Convert.ToInt32(row["id"]);
                            string username = row["username"].ToString();
                            string profilepic = row["profilepic"].ToString();
                            string dob = row["dob"].ToString();
                            string gender = row["gender"].ToString();
                            string email = row["email"].ToString();
                            string role = row["role"].ToString();
                            bool isActive = row["isactive"] != DBNull.Value && Convert.ToBoolean(row["isactive"]);
                            bool logintime = row["logintime"] != DBNull.Value && Convert.ToBoolean(row["logintime"]);
                            bool subscriptionActive = row["subscriptionactive"] != DBNull.Value && Convert.ToBoolean(row["subscriptionactive"]);

                            if (!isActive)
                            {
                                ModelState.AddModelError(string.Empty, "User not found.");
                                return Page();
                            }

                            // Proceed with login based on logintime
                            if (!logintime)
                            {
                                HttpContext.Session.SetInt32("Id", id);
                                HttpContext.Session.SetString("Username", username);
                                HttpContext.Session.SetString("profilepic", profilepic);
                                HttpContext.Session.SetString("email", email);
                                HttpContext.Session.SetString("dob", dob);
                                HttpContext.Session.SetString("gender", gender);
                                HttpContext.Session.SetString("UserRole", role);
                                HttpContext.Session.SetString("SubscriptionActive", subscriptionActive.ToString());

                                // Redirect based on user role
                                if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                                {
                                    return RedirectToPage("/Deshbord");
                                }
                                else if (role.Equals("user", StringComparison.OrdinalIgnoreCase))
                                {
                                    return RedirectToPage("/Home");
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Invalid role.");
                                    return Page();
                                }
                            }
                            else
                            {
                                // Generate and send OTP
                                string otp = GenerateOtp();
                                SendOtpToEmail(User.email, otp);

                                HttpContext.Session.SetString("Otp", otp);
                                HttpContext.Session.SetInt32("Id", id);
                                HttpContext.Session.SetString("Username", username);
                                HttpContext.Session.SetString("profilepic", profilepic);
                                HttpContext.Session.SetString("email", email);
                                HttpContext.Session.SetString("dob", dob);
                                HttpContext.Session.SetString("gender", gender);
                                HttpContext.Session.SetString("UserRole", role);

                                return RedirectToPage("/OtpVerification/Sign_in_otp_verify");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid password.");
                            return Page();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email not found.");
                        return Page();
                    }
                }
            }
        }



        private string GenerateOtp()
        {
            return random.Next(100000, 999999).ToString();
        }

        private void SendOtpToEmail(string email, string otp)
        {
            var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = _configuration.GetValue<int>("EmailSettings:SmtpPort"),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderName"]),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is {otp}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);
        }
    }
}
