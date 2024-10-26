using Main_Project.Pages.Mage_subscription;
using Microsoft.AspNetCore.Mvc;
using Netflix.Models;
using System;
using Microsoft.Data.SqlClient; // Ensure correct namespace
using System.Threading.Tasks;

public class SubscriptionController : Controller
{
    private readonly UserSessionRepository _repository;
    private readonly string _connectionString;

    public SubscriptionController(UserSessionRepository repository, string connectionString)
    {
        _repository = repository;
        _connectionString = connectionString;
    }

    public async Task<IActionResult> Login(string userId, int planId)
    {
        var plan = await GetSubscriptionPlanById(planId);
        if (plan == null)
        {
            return BadRequest("Invalid subscription plan.");
        }

        int maxLogins = int.Parse(plan.planedetail.Split('+')[1].Split(' ')[1]);
        int currentLogins = _repository.GetCurrentLogins(int.Parse(userId)); // Convert userId to int

        if (currentLogins >= maxLogins)
        {
            return BadRequest("Login limit reached for this subscription plan.");
        }

        _repository.AddSession(new UserSessions // Ensure correct type name
        {
            UserId = int.Parse(userId), // Convert userId to int
            SubscriptionPlanId = planId,
            LastLogin = DateTime.Now,
            IsWatching = false
        });

        HttpContext.Session.SetString("UserId", userId);
        HttpContext.Session.SetInt32("SubscriptionPlanId", planId);

        return Ok("Logged in successfully.");
    }

    public async Task<IActionResult> Watch(string userId, int planId)
    {
        var plan = await GetSubscriptionPlanById(planId);
        if (plan == null)
        {
            return BadRequest("Invalid subscription plan.");
        }

        int maxWatchers = int.Parse(plan.planedetail.Split('+')[0].Split(' ')[0]);
        int currentWatchers = _repository.GetCurrentWatchers(int.Parse(userId)); // Convert userId to int

        if (currentWatchers >= maxWatchers)
        {
            return BadRequest("Watch limit reached for this subscription plan.");
        }

        _repository.UpdateWatchStatus(int.Parse(userId), true); // Convert userId to int

        return Ok("You can watch now.");
    }

    public IActionResult StopWatching(string userId)
    {
        _repository.UpdateWatchStatus(int.Parse(userId), false); // Convert userId to int
        return Ok("Stopped watching.");
    }

    public IActionResult Logout(string userId)
    {
        _repository.RemoveSession(int.Parse(userId)); // Convert userId to int
        HttpContext.Session.Remove("UserId");
        HttpContext.Session.Remove("SubscriptionPlanId");
        return Ok("Logged out successfully.");
    }

    private async Task<subscription> GetSubscriptionPlanById(int planId)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM subscription WHERE id = @planId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@planId", planId);

            conn.Open();
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new subscription
                    {
                        id = (int)reader["id"],
                        price = (int)reader["price"],
                        planeduration = (int)reader["planeduration"],
                        planedetail = reader["planedetail"].ToString(),
                        planename = reader["planename"].ToString()
                    };
                }
            }
        }
        return null;
    }
}
