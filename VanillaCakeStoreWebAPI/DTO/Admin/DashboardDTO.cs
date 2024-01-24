namespace VanillaCakeStoreWebAPI.DTO.Admin
{
    public class DashboardDTO
    {
        public decimal WeeklySales { get; set; }
        public decimal TotalOrders { get; set; }
        public int TotalCustomer { get; set; }
        public int TotalGuest { get; set; }
    }
}
