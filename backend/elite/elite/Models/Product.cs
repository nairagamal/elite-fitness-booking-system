namespace elite.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; } // Apparel/Supplements

        public int StockQuantity { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }

}
