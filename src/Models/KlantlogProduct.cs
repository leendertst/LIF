using System.ComponentModel.DataAnnotations;

namespace Lif.Models
{
    public class KlantlogProduct
    {
        public int Id { get; set; }

        [Required]
        public int KlantlogId { get; set; }
        public Klantlog Klantlog { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        [Range(1, 1000)]
        public int Aantal { get; set; } = 1;

        public DateTime ToegevoegdOp { get; set; } = DateTime.Now;
    }
}