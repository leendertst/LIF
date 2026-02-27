using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lif.Pages.Admin.Users
{
    [Authorize(Roles = "admin")] //  Alleen admins
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> AvailableRoles { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Email is verplicht")]
            [EmailAddress(ErrorMessage = "Ongeldig email adres")]
            [Display(Name = "E-mailadres")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Wachtwoord is verplicht")]
            [StringLength(100, ErrorMessage = "Het {0} moet minimaal {2} en maximaal {1} tekens lang zijn.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Wachtwoord")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Bevestig wachtwoord")]
            [Compare("Password", ErrorMessage = "Het wachtwoord en bevestigingswachtwoord komen niet overeen.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Rol is verplicht")]
            [Display(Name = "Rol")]
            public string Role { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAvailableRolesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableRolesAsync();
                return Page();
            }

            // Check of de rol bestaat
            if (!await _roleManager.RoleExistsAsync(Input.Role))
            {
                ModelState.AddModelError("Input.Role", $"Rol '{Input.Role}' bestaat niet.");
                await LoadAvailableRolesAsync();
                return Page();
            }

            // Check of email al bestaat
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Email", "Een gebruiker met dit email adres bestaat al.");
                await LoadAvailableRolesAsync();
                return Page();
            }

            var user = new ApplicationUser 
            { 
                UserName = Input.Email, 
                Email = Input.Email,
                EmailConfirmed = true // Auto-confirm voor admin-created users
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Input.Role);
                TempData["SuccessMessage"] = $"Gebruiker '{Input.Email}' is succesvol aangemaakt met rol '{Input.Role}'.";
                return RedirectToPage("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadAvailableRolesAsync();
            return Page();
        }

        private async Task LoadAvailableRolesAsync()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => r.Name!)
                .ToListAsync();

            AvailableRoles = roles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r.ToUpper() == "ADMIN" ? "Administrator" : "Gebruiker"
            }).ToList();
        }
    }
}
