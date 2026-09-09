namespace elite.Models
{
    public class MembershipType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMonths { get; set; }
        public decimal TotalHours { get; set; }      
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
