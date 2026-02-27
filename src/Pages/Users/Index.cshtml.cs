using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lif.Pages.Admin.Users
{
    [Authorize(Roles = "admin")] // Alleen admins
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<ApplicationUser> Users { get; set; } = new();
        public UserManager<ApplicationUser> UserManager => _userManager;
        public List<string> AvailableRoles { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        public async Task OnGetAsync()
        {
            // Haal alle beschikbare rollen op
            AvailableRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(r => r)
                .ToListAsync();

            var usersQuery = _userManager.Users.AsQueryable();

            // Zoeken op email of username
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                usersQuery = usersQuery.Where(u => 
                    u.Email!.Contains(SearchQuery) || 
                    u.UserName!.Contains(SearchQuery));
            }

            var allUsers = await usersQuery.OrderBy(u => u.Email).ToListAsync();

            // Filteren op rol
            if (!string.IsNullOrWhiteSpace(RoleFilter))
            {
                var filteredUsers = new List<ApplicationUser>();
                foreach (var user in allUsers)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Contains(RoleFilter))
                    {
                        filteredUsers.Add(user);
                    }
                }
                Users = filteredUsers;
            }
            else
            {
                Users = allUsers;
            }
        }
    }
}
