using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lif.Pages.Admin.Users
{
    [Authorize(Roles = "admin")] // Alleen admins
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public List<ApplicationUser> Users { get; set; } = new();
        public UserManager<ApplicationUser> UserManager => _userManager;

        public void OnGet()
        {
            Users = _userManager.Users.ToList();
        }
    }
}
