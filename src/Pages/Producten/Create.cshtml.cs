using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lif.Pages.Producten
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Product Product { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            _context.Producten.Add(Product);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
