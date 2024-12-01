using Main_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Main_Project.Pages.User_Profile_manage
{
    public class Manage_profileModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<Manage_profileModel> _logger;
        private readonly string _connectionString;

        public Manage_profileModel(IEmailService emailService, ILogger<Manage_profileModel> logger, IConfiguration configuration)
        {
            _emailService = emailService;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public string UserName { get; set; }
        public string ProfilePic { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public string Dob { get; set; }
        public string Gender { get; set; }
        public int? UserId { get; set; }
        public string id { get; set; }

        public string ErrorMessage { get; set; }

        public Dictionary<string, List<Profile_pic>> GroupedProfilePics { get; private set; }
        public void OnGet()
        {
            // Retrieve the email from session
            string sessionEmail = HttpContext.Session.GetString("email");
            UserId = HttpContext.Session.GetInt32("Id");
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
                                Dob = reader["dob"].ToString();
                                id = reader["id"].ToString();
                                Gender = reader["gender"].ToString();
                                ProfilePic = reader["profilepic"]?.ToString(); // Handle potential null
                                Email = sessionEmail; // Set email from session
                                UserRole = reader["role"].ToString();
                            }
                        }
                    }
                }
            }

            // Retrieve and store grouped profile pics
            GroupedProfilePics = GetProfilePicsGroupedByGroup();
        }

        public Dictionary<string, List<Profile_pic>> GetProfilePicsGroupedByGroup()
        {
            var groupedProfilePics = new Dictionary<string, List<Profile_pic>>();
            string query = "SELECT * FROM Profile_pic";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var profilePic = new Profile_pic
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Pics = reader.IsDBNull(reader.GetOrdinal("Pics")) ? null : reader.GetString(reader.GetOrdinal("Pics")),
                            Groups = reader.GetString(reader.GetOrdinal("Groups"))
                        };

                        // Group by the "Groups" field
                        if (!groupedProfilePics.ContainsKey(profilePic.Groups))
                        {
                            groupedProfilePics[profilePic.Groups] = new List<Profile_pic>();
                        }
                        groupedProfilePics[profilePic.Groups].Add(profilePic);
                    }
                }
            }

            return groupedProfilePics;
        }

        public async Task<IActionResult> OnPostDeleteaccountAsync()
        {
            int? UserId = HttpContext.Session.GetInt32("Id");
            if (UserId == null)
            {
                return RedirectToPage("/Index");
            }

            string email = HttpContext.Session.GetString("email"); // Get email from session
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Update the isactive status in the database
                string updateQuery = "UPDATE User_data SET isactive = 0 WHERE id = @UserId";
                using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                {
                    updateCmd.Parameters.AddWithValue("@UserId", UserId);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                // Delete data from Watch_list where email matches
                string deleteQuery = "DELETE FROM Watch_list WHERE userid = @UserId";
                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@UserId", UserId);
                    await deleteCmd.ExecuteNonQueryAsync();
                }

                con.Close();
            }

            // Clear session and cookies
            HttpContext.Session.Clear();
            Response.Cookies.Delete("deviceUniqueId"); // Delete any specific cookies you are using

            // Redirect to the Index page
            return RedirectToPage("/Index");
        }



        public async Task<IActionResult> OnPostUpdateProfilePicAsync()
        {
            string profilepic = Request.Form["profilepic"];
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string updateQuery = "UPDATE User_data SET profilepic = @ProfilePic WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfilePic", profilepic);
                        cmd.Parameters.AddWithValue("@Email", HttpContext.Session.GetString("email"));

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                TempData["message"] = "Profile picture updated successfully.";
            
            // Reload the updated data
            OnGet();

            return Page();
        }


        public async Task<IActionResult> OnPostUpdateOtherDetailAsync()
        {
            // Fetch the form data
            string newEmail = Request.Form["email"];
            string username = Request.Form["fullname"];
            string dob = Request.Form["dob"];
            string gender = Request.Form["gender"];

            // Debugging: Output values to the console/log
            Console.WriteLine($"Email: {newEmail}, Username: {username}, DOB: {dob}, Gender: {gender}");

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string updateQuery = @"
            UPDATE User_data 
            SET username = @Username, dob = @Dob, gender = @Gender 
            WHERE email = @Email";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Dob", dob);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@Email", HttpContext.Session.GetString("email"));

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    con.Close();

                    // Debugging: Check if the update was successful
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Update successful.");
                    }
                    else
                    {
                        Console.WriteLine("Update failed. No rows affected.");
                    }
                }
            }

            // Reload the updated data
            OnGet();

            TempData["message"] = "Details updated successfully.";
            return Page();
        }


        public async Task<IActionResult> OnPostUpdateEmailAsync()
        {
            string newEmail = Request.Form["email"].ToString();
            string currentEmail = HttpContext.Session.GetString("email");

            if (string.IsNullOrEmpty(currentEmail))
            {
                TempData["message"] = "Failed to retrieve your current email. Please try again.";
                return Page();
            }

            // Generate OTP
            string otp = GenerateOTP();

            // Send OTP via email to the current (old) email
            bool otpSent = await SendOtpToEmail(currentEmail, otp);

            if (otpSent)
            {
                // Store OTP and new email in session and redirect to verification page
                HttpContext.Session.SetString("NewEmail", newEmail);
                HttpContext.Session.SetString("Otp", otp);
                TempData["message"] = "OTP has been sent to your current email. Please verify.";
                return Redirect("/OtpVerification/VerifyOtp_for_change_email");
            }
            else
            {
                TempData["message"] = "Failed to send OTP. Please try again.";
                return Page();
            }
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        private async Task<bool> SendOtpToEmail(string email, string otp)
        {
            try
            {
                var emailBody = new StringBuilder();
                emailBody.AppendLine("Your OTP code is:");
                emailBody.AppendLine(otp);
                emailBody.AppendLine("Please enter this code to verify your email change request.");

                return await _emailService.SendEmailAsync(email, "OTP for Email Change", emailBody.ToString());
            }
            catch
            {
                ErrorMessage = "Failed to send OTP email. Please try again.";
                return false;
            }
        }

    }
}
