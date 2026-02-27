using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lif.Pages.Klantlogs
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Klantlog Klantlog { get; set; } = default!;
        public bool CanEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var klantlog = await _context.Klantlogs
                .Include(k => k.User)
                .Include(k => k.KlantlogProducten)
                    .ThenInclude(kp => kp.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (klantlog == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("admin");

            // Check autorisatie
            if (!isAdmin && klantlog.ApplicationUserId != currentUserId)
                return Forbid();

            Klantlog = klantlog;
            CanEdit = isAdmin || klantlog.ApplicationUserId == currentUserId;

            return Page();
        }

        public async Task<IActionResult> OnPostVerwijderProductAsync(int klantlogId, int productId)
        {
            var klantlogProduct = await _context.KlantlogProducten
                .FirstOrDefaultAsync(kp => kp.KlantlogId == klantlogId && kp.ProductId == productId);

            if (klantlogProduct == null)
                return NotFound();

            var klantlog = await _context.Klantlogs.FindAsync(klantlogId);
            if (klantlog == null || klantlog.Status != KlantlogStatus.Concept)
                return BadRequest("Producten kunnen alleen verwijderd worden uit concept klantlogs.");

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && klantlog.ApplicationUserId != currentUserId)
                return Forbid();

            _context.KlantlogProducten.Remove(klantlogProduct);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = klantlogId });
        }
    }
}