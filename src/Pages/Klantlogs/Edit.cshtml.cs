using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lif.Pages.Klantlogs
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Klantlog Klantlog { get; set; } = default!;

        public List<SelectListItem> BeschikbareProducten { get; set; } = new();
        
        [BindProperty]
        public int? ToTeVoegenProductId { get; set; }
        
        [BindProperty]
        public int ToTeVoegenAantal { get; set; } = 1;

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

            // Check autorisatie - user kan alleen eigen logs bewerken
            if (!isAdmin && klantlog.ApplicationUserId != currentUserId)
                return Forbid();

            // Alleen concept klantlogs kunnen bewerkt worden
            if (klantlog.Status != KlantlogStatus.Concept)
            {
                TempData["ErrorMessage"] = "Alleen concept klantlogs kunnen bewerkt worden.";
                return RedirectToPage("Details", new { id });
            }

            Klantlog = klantlog;
            await LaadProductenAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostVoegProductToeAsync()
        {
            if (!ToTeVoegenProductId.HasValue || ToTeVoegenAantal < 1)
            {
                ModelState.AddModelError(string.Empty, "Selecteer een product en vul een geldig aantal in.");
                await LaadKlantlogAsync();
                await LaadProductenAsync();
                return Page();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("admin");

            // Check autorisatie
            if (!isAdmin && Klantlog.ApplicationUserId != currentUserId)
                return Forbid();

            // Check of product al bestaat in klantlog
            var bestaatAl = await _context.KlantlogProducten
                .AnyAsync(kp => kp.KlantlogId == Klantlog.Id && kp.ProductId == ToTeVoegenProductId.Value);

            if (bestaatAl)
            {
                // Update aantal
                var bestaand = await _context.KlantlogProducten
                    .FirstAsync(kp => kp.KlantlogId == Klantlog.Id && kp.ProductId == ToTeVoegenProductId.Value);
                
                bestaand.Aantal += ToTeVoegenAantal;
                TempData["SuccessMessage"] = "Aantal bijgewerkt.";
            }
            else
            {
                // Voeg nieuw product toe
                var klantlogProduct = new KlantlogProduct
                {
                    KlantlogId = Klantlog.Id,
                    ProductId = ToTeVoegenProductId.Value,
                    Aantal = ToTeVoegenAantal
                };

                _context.KlantlogProducten.Add(klantlogProduct);
                TempData["SuccessMessage"] = "Product toegevoegd.";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id = Klantlog.Id });
        }

        public async Task<IActionResult> OnPostVerwijderProductAsync(int productId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("admin");

            await LaadKlantlogAsync();

            // Check autorisatie
            if (!isAdmin && Klantlog.ApplicationUserId != currentUserId)
                return Forbid();

            var klantlogProduct = await _context.KlantlogProducten
                .FirstOrDefaultAsync(kp => kp.KlantlogId == Klantlog.Id && kp.ProductId == productId);

            if (klantlogProduct != null)
            {
                _context.KlantlogProducten.Remove(klantlogProduct);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product verwijderd.";
            }

            return RedirectToPage(new { id = Klantlog.Id });
        }

        public async Task<IActionResult> OnPostUpdateOpmerkingen()
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("admin");

            // Check autorisatie
            if (!isAdmin && Klantlog.ApplicationUserId != currentUserId)
                return Forbid();

            var dbKlantlog = await _context.Klantlogs.FindAsync(Klantlog.Id);
            if (dbKlantlog == null)
                return NotFound();

            dbKlantlog.Opmerkingen = Klantlog.Opmerkingen;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Opmerkingen bijgewerkt.";
            return RedirectToPage(new { id = Klantlog.Id });
        }

        private async Task LaadKlantlogAsync()
        {
            Klantlog = await _context.Klantlogs
                .Include(k => k.User)
                .Include(k => k.KlantlogProducten)
                    .ThenInclude(kp => kp.Product)
                .FirstAsync(k => k.Id == Klantlog.Id);
        }

        private async Task LaadProductenAsync()
        {
            var producten = await _context.Producten
                .OrderBy(p => p.Naam)
                .ToListAsync();

            BeschikbareProducten = producten.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Naam} - {p.Prijs:C}"
            }).ToList();
        }
    }
}