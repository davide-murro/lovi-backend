using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos.Pagination
{
    public class PagedQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }

        [RegularExpression("asc|desc")]
        public string SortOrder { get; set; } = "asc";
        public string? Search { get; set; }
    }
}
