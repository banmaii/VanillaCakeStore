namespace VanillaCakeStoreWebAPI.DTO.Product
{
    public class PagingProductDTO
    {
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public ICollection<ProductDTO>? Values { get; set; }

    }
}
