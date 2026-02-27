using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Lif.Pages.Klantlogs
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool IsAdmin { get; set; }
        public List<ApplicationUser> AllUsers { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Datum is verplicht")]
            [Display(Name = "Datum")]
            public DateTime Datum { get; set; } = DateTime.Now;

            [Display(Name = "Opmerkingen")]
            public string? Opmerkingen { get; set; }

            // Alleen voor admin - kan klant selecteren
            [Display(Name = "Klant")]
            public string? ApplicationUserId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            IsAdmin = User.IsInRole("admin");
            
            if (IsAdmin)
            {
                AllUsers = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IsAdmin = User.IsInRole("admin");
            var currentUserId = _userManager.GetUserId(User)!;

            // Als admin moet een klant geselecteerd zijn
            if (IsAdmin && string.IsNullOrEmpty(Input.ApplicationUserId))
            {
                ModelState.AddModelError("Input.ApplicationUserId", "Selecteer een klant voor deze klantlog.");
                AllUsers = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
                return Page();
            }

            // Bepaal voor wie de klantlog is
            var userId = IsAdmin ? Input.ApplicationUserId! : currentUserId;

            // Bereken week en jaar
            var calendar = CultureInfo.CurrentCulture.Calendar;
            var weekRule = CalendarWeekRule.FirstFourDayWeek;
            var firstDayOfWeek = DayOfWeek.Monday;
            var weekNummer = calendar.GetWeekOfYear(Input.Datum, weekRule, firstDayOfWeek);
            var jaar = Input.Datum.Year;

            // Check of er al een klantlog bestaat voor deze week/jaar/user
            var bestaatAl = await _context.Klantlogs
                .AnyAsync(k => k.ApplicationUserId == userId 
                            && k.WeekNummer == weekNummer 
                            && k.Jaar == jaar);

            if (bestaatAl)
            {
                ModelState.AddModelError(string.Empty, 
                    $"Er bestaat al een klantlog voor week {weekNummer} van {jaar}.");
                
                if (IsAdmin)
                {
                    AllUsers = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
                }
                return Page();
            }

            var klantlog = new Klantlog
            {
                ApplicationUserId = userId,
                Datum = Input.Datum,
                WeekNummer = weekNummer,
                Jaar = jaar,
                Opmerkingen = Input.Opmerkingen,
                Status = KlantlogStatus.Concept
            };

            _context.Klantlogs.Add(klantlog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Klantlog voor week {weekNummer} is aangemaakt.";
            return RedirectToPage("Edit", new { id = klantlog.Id });
        }
    }
}