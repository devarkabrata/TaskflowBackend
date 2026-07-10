namespace TaskFlowBackend.Enums
{
    public static class TaskEvents
    {
        public static ReadOnlySpan<char> TaskCreatedEmail => "email.task-created";
        public static ReadOnlySpan<char> MemberAddedEmail => "email.member-added";
        public static ReadOnlySpan<char> TeamCreatedEmail => "email.team-created";
        public static ReadOnlySpan<char> ForgotPasswordEmail => "email.forgot-password";
    }
}
