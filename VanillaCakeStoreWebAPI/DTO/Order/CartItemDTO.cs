namespace VanillaCakeStoreWebAPI.DTO.Order
{
    public class CartItemDTO
    {
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public decimal? UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
