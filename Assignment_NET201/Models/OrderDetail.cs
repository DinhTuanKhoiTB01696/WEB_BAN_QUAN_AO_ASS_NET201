using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_NET201.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ComboId { get; set; }
        public Combo? Combo { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Price at the time of purchase
    }
}
