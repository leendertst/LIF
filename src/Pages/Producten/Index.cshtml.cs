using Lif.Data;
using Lif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lif.Pages.Producten
{
    [Authorize] // Alle ingelogde gebruikers kunnen producten bekijken
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public IList<Product> Producten { get; set; } = new List<Product>();
        public bool IsAdmin { get; set; }

        public async Task OnGetAsync()
        {
            IsAdmin = User.IsInRole("admin");
            Producten = await _context.Producten.OrderBy(p => p.Naam).ToListAsync();
        }
    }
}
