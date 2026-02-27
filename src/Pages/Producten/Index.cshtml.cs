using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lif.Pages.Producten
{
    [Authorize(Roles = "admin")] // Alleen admins kunnen producten beheren
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public IList<Product> Producten { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            Producten = await _context.Producten.ToListAsync();
        }
    }
}
