using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Models;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDBContext _db;
        private readonly ITokenService _tokenService;

        public AuthService(AppDBContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> SignupAsync(SignupRequestDto dto)
        {
            var emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "email", Code = "EMAIL_TAKEN", Message = "An account with this email already exists." }
                });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Title = dto.Title ?? string.Empty,
                AvatarInitials = ComputeInitials(dto.Name),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            return BuildResponse(user);
        }

        private AuthResponseDto BuildResponse(User user)
        {
            return new AuthResponseDto
            {
                Token = _tokenService.GenerateToken(user),
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Title = user.Title,
                    AvatarInitials = user.AvatarInitials,
                    AvatarUrl = user.AvatarUrl,
                }
            };
        }

        private static string ComputeInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }
}
