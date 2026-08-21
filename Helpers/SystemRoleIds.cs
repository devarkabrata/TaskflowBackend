namespace TaskFlowBackend.Helpers
{
    // Fixed ids for the seeded placeholder roles — referenced by the seed migration
    // and by TeamService when assigning a default role. Replace/extend once the
    // real role list is finalized.
    public static class SystemRoleIds
    {
        public static readonly Guid Admin = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid Pm = new("22222222-2222-2222-2222-222222222222");
        public static readonly Guid Tl = new("33333333-3333-3333-3333-333333333333");
        public static readonly Guid Developer = new("44444444-4444-4444-4444-444444444444");
    }
}
