namespace VanillaCakeStoreWebAPI.DTO.Order
{
    public class PagingOrderDTO
    {
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public ICollection<OrderAdminDTO>? Values { get; set; }
    }
}
