using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Main_Project.Models;
using Main_Project.Services;
using Microsoft.Extensions.Options;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using Netflix.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Main_Project.Pages.OtpVerification;

namespace Netflix.Pages
{
    public class Sign_upModel : PageModel
    {
        public List<subscription> subscription = new List<subscription>();

        private readonly EmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;
        private static Random random = new Random();

        public Sign_upModel(EmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        [BindProperty]
        public user_regi User { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorMessagephone { get; set; }

        public int? UserId { get; set; } // Property to hold the User ID

        public IActionResult OnGet()
        {

            

            // If the user is not logged in (UserId does not exist), stay on the Index page
            
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
                    connection.Close();
                }
            }

            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {
                // If the user is logged in (UserId exists), redirect to the Home page
                return RedirectToPage("/Home");
            }

            return Page();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            User = new user_regi();
            User.fullname = Request.Form["username"];
            User.email = Request.Form["email"];
            User.phone = Request.Form["phone"];
            User.password = Request.Form["password"];
            User.isactive = true;
            User.price3 = Request.Form["price3"];
            User.datetime3 = Request.Form["datetime3"];
            User.duration3 = Request.Form["duration3"];
            if (int.TryParse(Request.Form["subid"], out int subidValue))
            {
                User.subid = subidValue;
            }
            else
            {
                User.subid = null; // or handle the error as needed
            }

            User.paymentmethod = Request.Form["paymentMethod"];
            User.role = Request.Form["role"];
            User.profilepic = Request.Form["profilepic"];
            User.dob = Request.Form["dob"];
            User.logintime = true;
            User.subscriptionactive = true;
            User.emailsent = false;
            User.date = DateOnly.FromDateTime(DateTime.Now);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Check if email already exists
                string checkQuery = "SELECT COUNT(*) FROM User_data WHERE email = @Email AND isactive = 1";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@Email", User.email);
                    con.Open();
                    int count = (int)checkCmd.ExecuteScalar();
                    con.Close();

                    if (count > 0)
                    {
                        ErrorMessage = "Email already in use.";
                        return Page();
                    }
                }

                // Check if phone already exists
                string checkphoneQuery = "SELECT COUNT(*) FROM User_data WHERE phone = @Phone AND isactive = 1";
                using (SqlCommand checkCmd = new SqlCommand(checkphoneQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@Phone", User.phone);
                    con.Open();
                    int count = (int)checkCmd.ExecuteScalar();
                    con.Close();

                    if (count > 0)
                    {
                        ErrorMessagephone = "Phone number already in use.";
                        return Page();
                    }
                }

                // Generate OTP
                string otp = GenerateOtp();

                // Send OTP email
                try
                {
                    await _emailService.SendEmailAsync(User.email, "Verify your email", "Your OTP is: " + otp);
                    // Store OTP and user data in TempData
                   
                    TempData["Otp"] = otp;
                    TempData["User"] = JsonConvert.SerializeObject(User);

                    return RedirectToPage("/OtpVerification/VerrifyOtp_for_SignUp"); 
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error sending email. Please try again later.";
                    return Page();
                }
            }
        }

        private string GenerateOtp()
        {
            int otp = random.Next(100000, 999999);
            return otp.ToString();
        }
    }
}
