namespace VanillaCakeStore.Models.Home
{
    public class HomeFilterView
    {
        public int CategoryId { get; set; }
        public int? pageHot { get; set; } = 1;
        public int? pageNew { get; set; } = 1;
        public int? pageSale { get; set; } = 1;
    }
}
