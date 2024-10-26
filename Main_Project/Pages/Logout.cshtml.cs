using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Main_Project.Pages
{
    public class LogoutModel : PageModel
    {


        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Clear(); // Clear session
            return RedirectToPage("/Index"); // Redirect to Index or Login page
        }


    }
}
