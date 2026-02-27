using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lif.Pages.Admin.Users
{
    [Authorize(Roles = "admin")]
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUser ApplicationUser { get; set; } = default!;
        public IList<string> Roles { get; set; } = new List<string>();
        public IList<string> AllRoles { get; set; } = new List<string> { "admin", "user" };

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ApplicationUser = user;
            Roles = await _userManager.GetRolesAsync(user);

            return Page();
        }
    }
}