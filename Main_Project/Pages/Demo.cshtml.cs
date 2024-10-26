using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Main_Project.Pages
{
    public class DemoModel : PageModel
    {
        private readonly NetflixDataContext _context;

        public DemoModel(NetflixDataContext context)
        {
            _context = context;
        }
        public string UserName { get; set; }
        public string profilepic { get; set; }
        public string email { get; set; }
        public IList<MoviesTable> Movies { get; set; } = new List<MoviesTable>();
        public async Task OnGetAsync(string searchKeyword)
        {
            UserName = HttpContext.Session.GetString("Username");
            profilepic = HttpContext.Session.GetString("profilepic");
            email = HttpContext.Session.GetString("email");

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                // Fetch movies matching the search keyword (case-insensitive)
                Movies = await _context.MoviesTables
                    .Where(m => EF.Functions.Like(m.Moviename.ToLower(), $"%{searchKeyword.ToLower()}%"))
                    .ToListAsync();
            }
        }
    }
}
