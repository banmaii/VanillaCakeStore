namespace VanillaCakeStoreWebAPI.DTO.Order
{
    public class OrderAdminDTO
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public decimal? Freight { get; set; }
    }
}
