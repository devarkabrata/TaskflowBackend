namespace TaskFlowBackend.Helpers.Pagination
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int Count => Data.Count;
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages => Limit > 0 ? (int)Math.Ceiling((double)Total / Limit) : 0;
    }
}
