namespace Assignment_NET201.Models
{
    public class ComboProduct
    {
        public int ComboId { get; set; }
        public Combo Combo { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
