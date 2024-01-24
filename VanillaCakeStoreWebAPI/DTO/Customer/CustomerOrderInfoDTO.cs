using VanillaCakeStoreWebAPI.DTO.Order;

namespace VanillaCakeStoreWebAPI.DTO.Customer
{
    public class CustomerOrderInfoDTO
    {
        public string? CompanyName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactTitle { get; set; }
        public string? Address { get; set; }
        public DateTime? RequiredDate { get; set; }
        public List<CartItemDTO> Items { get; set; }
    }
}
