using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Main_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SubscriptionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult CheckSubscriptionStatus()
        {
            if (HttpContext.Session.GetString("email") != null)
            {
                string userEmail = HttpContext.Session.GetString("email");

                using (var con = new SqlConnection(_configuration.GetConnectionString("NetflixDatabase")))
                {
                    con.Open();
                    var cmd = new SqlCommand("SELECT subscriptionactive FROM User_data WHERE email = @Email", con);
                    cmd.Parameters.AddWithValue("@Email", userEmail);

                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return Ok(new { status = Convert.ToBoolean(result) ? "active" : "expired" });
                    }
                }
            }

            return Ok(new { status = "inactive" }); // User not authenticated
        }
    }
}
