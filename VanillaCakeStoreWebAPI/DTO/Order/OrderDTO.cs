namespace VanillaCakeStoreWebAPI.DTO.Order
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public string? ShipAddress { get; set; }
        public virtual ICollection<CartItemDTO> OrderDetails { get; set; }
    }
}
