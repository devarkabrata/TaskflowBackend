using TaskFlowBackend.Enums;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(Guid userId, Guid teamId, PermissionType permission);
        Task EnsureHasPermissionAsync(Guid userId, Guid teamId, PermissionType permission);
    }
}
