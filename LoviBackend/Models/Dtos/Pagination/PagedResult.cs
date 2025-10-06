namespace LoviBackend.Models.Dtos.Pagination
{
    public class PagedResult<T>
    {
        public PagedQuery PagedQuery { get; set; } = new();
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PagedQuery.PageSize);
    }
}
