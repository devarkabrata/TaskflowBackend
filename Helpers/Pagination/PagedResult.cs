namespace TaskFlowBackend.Helpers.Pagination
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages => Limit > 0 ? (int)Math.Ceiling((double)TotalCount / Limit) : 0;
    }
}
