using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lif.Pages.Klantlogs
{
    [Authorize] // Alle ingelogde gebruikers
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Klantlog> Klantlogs { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public KlantlogStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchUserId { get; set; }

        public bool IsAdmin { get; set; }
        public List<ApplicationUser> AllUsers { get; set; } = new();

        public async Task OnGetAsync()
        {
            IsAdmin = User.IsInRole("admin");
            var currentUserId = _userManager.GetUserId(User);

            var query = _context.Klantlogs
                .Include(k => k.User)
                .Include(k => k.KlantlogProducten)
                    .ThenInclude(kp => kp.Product)
                .AsQueryable();

            // Als niet admin, toon alleen eigen klantlogs
            if (!IsAdmin)
            {
                query = query.Where(k => k.ApplicationUserId == currentUserId);
            }
            else
            {
                // Admin kan filteren op specifieke gebruiker
                if (!string.IsNullOrEmpty(SearchUserId))
                {
                    query = query.Where(k => k.ApplicationUserId == SearchUserId);
                }
                
                // Laad alle users voor admin dropdown
                AllUsers = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            }

            // Filter op status
            if (StatusFilter.HasValue)
            {
                query = query.Where(k => k.Status == StatusFilter.Value);
            }

            Klantlogs = await query
                .OrderByDescending(k => k.Jaar)
                .ThenByDescending(k => k.WeekNummer)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostMaakDefinitiefAsync(int id)
        {
            var klantlog = await _context.Klantlogs.FindAsync(id);
            
            if (klantlog == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("admin");

            // Check autorisatie - user kan alleen eigen logs definitief maken
            if (!isAdmin && klantlog.ApplicationUserId != currentUserId)
                return Forbid();

            if (klantlog.Status != KlantlogStatus.Concept)
            {
                TempData["ErrorMessage"] = "Alleen concept klantlogs kunnen definitief gemaakt worden.";
                return RedirectToPage();
            }

            klantlog.Status = KlantlogStatus.Definitief;
            klantlog.DefinitiefGemaaktOp = DateTime.Now;
            klantlog.DefinitiefGemaaktDoor = currentUserId;

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Klantlog week {klantlog.WeekNummer} is definitief gemaakt.";
            return RedirectToPage();
        }
    }
}