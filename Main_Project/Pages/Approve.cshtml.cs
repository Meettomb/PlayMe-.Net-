using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Main_Project.Pages
{
    public class ApproveModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        public bool IsApproved { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(Email))
            {
                IsApproved = false;
                return Page();
            }

            // Mark user as approved
            HttpContext.Session.SetString("IsApproved", "true");

            IsApproved = true;
            return Page();
        }
    }
}
