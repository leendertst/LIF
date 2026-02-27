using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lif.Models
{
    public class Klantlog
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public DateTime Datum { get; set; } = DateTime.Now;

        [Required]
        public int WeekNummer { get; set; }

        [Required]
        public int Jaar { get; set; }

        public KlantlogStatus Status { get; set; } = KlantlogStatus.Concept;

        public DateTime? DefinitiefGemaaktOp { get; set; }
        
        public string? DefinitiefGemaaktDoor { get; set; }

        public string? Opmerkingen { get; set; }

        // Relatie met producten
        public ICollection<KlantlogProduct> KlantlogProducten { get; set; } = new List<KlantlogProduct>();

        // Computed property voor totaalprijs
        public decimal TotaalPrijs => KlantlogProducten.Sum(kp => kp.Aantal * kp.Product.Prijs);
    }

    public enum KlantlogStatus
    {
        Concept,
        Definitief,
        Afgerond,
        Geannuleerd
    }
}
