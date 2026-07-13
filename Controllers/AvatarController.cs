using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users/avatar")]
    public class AvatarController : ControllerBase
    {
        private readonly IUserService _userService;

        public AvatarController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ApiResponse<AvatarResponseDto>> UploadAvatar(IFormFile file)
        {
            var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var updated = await _userService.UpdateAvatarAsync(callerId, file);

            var response = new AvatarResponseDto { AvatarUrl = updated.AvatarUrl , AvatarPublicId = updated.AvatarPublicId };
            return ApiResponse<AvatarResponseDto>.Success(response, "Avatar uploaded successfully.");
        }

        [HttpDelete]
        public async Task<ApiResponse<object>> DeleteAvatar()
        {
            var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var AvatarInitials = await _userService.DeleteAvatarAsync(callerId);

            var response = new { Initials = AvatarInitials };

            return ApiResponse<object>.Success(response, "Avatar deleted successfully.");
        }
    }
}
