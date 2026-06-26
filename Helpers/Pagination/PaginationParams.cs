namespace TaskFlowBackend.Helpers.Pagination
{
    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        public int Skip => (Page - 1) * Limit;
    }
}
