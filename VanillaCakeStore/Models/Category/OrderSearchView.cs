using System.ComponentModel.DataAnnotations;

namespace VanillaCakeStore.Models.Category
{
    public class OrderSearchView
    {
        [DataType(DataType.Date)]
        public DateTime? From { get; set; }
        [DataType(DataType.Date)]
        public DateTime? To { get; set; }
        public int? page { get; set; } = 1;
    }
}
