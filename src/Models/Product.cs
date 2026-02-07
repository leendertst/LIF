using System.ComponentModel.DataAnnotations;

namespace Lif.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Naam { get; set; }

        [Required]
        public decimal Prijs { get; set; }

        public string Beschrijving { get; set; }
    }
}
