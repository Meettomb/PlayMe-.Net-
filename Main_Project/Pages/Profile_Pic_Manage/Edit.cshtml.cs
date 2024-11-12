using Main_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Netflix.Models;


namespace Main_Project.Pages.Profile_Pic_Manage
{
    public class EditModel : PageModel
    {
        private readonly NetflixDataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;

        public EditModel(NetflixDataContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public List<user_regi> userlist = new List<user_regi>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; } // Use ProfilePic consistently

        [BindProperty]
        public IFormFile Uploade { get; set; }

        [BindProperty]
        public Profile_pic Profile_pic { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            string role = HttpContext.Session.GetString("UserRole");
            if (!userId.HasValue | role != "admin")
            {
                return Redirect("/");
            }
            if (id == null) return NotFound();

            Profile_pic = await _context.Profile_pic.FirstOrDefaultAsync(m => m.Id == id);
            if (Profile_pic == null) return NotFound();

            LoadUserDetails();
            return Page();
        }

        private void LoadUserDetails()
        {
            string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
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
                            }
                        }
                        con.Close();
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if a new file was uploaded
            if (Uploade != null && Uploade.Length > 0)
            {
                // Save the new file and update Profile_pic.Pics
                var filePath = Path.Combine(_environment.WebRootPath, "profile_pic", Uploade.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Uploade.CopyToAsync(fileStream);
                }

                // Update Profile_pic.Pics with the new file name
                Profile_pic.Pics = Uploade.FileName;
            }
            else
            {
                // If no new file was uploaded, keep the existing value of Profile_pic.Pics
                ModelState.Remove("Profile_pic.Pics"); // This is to avoid validation errors on the hidden field
            }

            // Explicitly mark the entity as modified
            _context.Entry(Profile_pic).State = EntityState.Modified;

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Profile_picExists(Profile_pic.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Redirect after successful update
            return RedirectToPage("/Profile_Pic_Manage/Pic_Index");
        }

        private bool Profile_picExists(int id)
        {
            return _context.Profile_pic.Any(e => e.Id == id);
        }
    }
}