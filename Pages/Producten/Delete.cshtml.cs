using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Lif.Pages.Producten
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DeleteModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Producten.FindAsync(id);

            if (Product == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var product = await _context.Producten.FindAsync(Product.Id);

            if (product == null)
                return NotFound();

            _context.Producten.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
