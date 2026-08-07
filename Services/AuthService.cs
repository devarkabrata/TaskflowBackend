using TaskFlowBackend.DTOs;
using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Events;
using TaskFlowBackend.Helpers;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepo;
        private readonly IUserService _userService;
        private readonly IRedisCacheService _redisCache;
        private readonly IEventPublisherService _eventPublisher;

        public AuthService(ITokenService tokenService, IUserRepository userRepo, IUserService userService, IRedisCacheService redisCache, IEventPublisherService eventPublisher)
        {
            _tokenService = tokenService;
            _userRepo = userRepo;
            _userService = userService;
            _redisCache = redisCache;
            _eventPublisher = eventPublisher;
        }

        public async Task<User?> SignupAsync(SignupRequestDto dto)
        {
            var emailTaken = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (emailTaken != null)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "email", Code = "EMAIL_TAKEN", Message = "An account with this email already exists." }
                });

            var newUser = new CreateUserRequestDto
            {
                Name = dto.Name,
                Email = dto.Email,
                Title = dto.Title,
                Password = dto.Password
            };

            var resp = await _userService.CreateUserWithWorkspace(newUser, dto.WorkspaceName);

            await _eventPublisher.PublishAsync(RoutingKeys.WelcomeEmail, new WelcomeEmailEvent
            {
                To = resp.createdUser!.Email,
                UserName = resp.createdUser.Name,
                WelcomeMessage = $"Welcome to TaskFlow, {resp.createdUser.Name}! We're excited to have you on board. Your workspace is ready for you to start, and we can't wait to see what you'll accomplish. If you have any questions or need assistance, our support team is here to help. Enjoy your journey with TaskFlow!"
            });

            return resp.createdUser;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);

            if(user == null)
            {
                throw new UnauthorizedException("Email does not exist.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Password is incorrect. Please try with another Password");
            }

            // Generate JWT Access token
            string accessToken = _tokenService.GenerateAccessToken(user);

            // Generate Refresh Token
            string refreshToken = _tokenService.GenerateRefreshToken();

            // Store refresh token in redis
            string key = $"refresh_token:{refreshToken}";
            RedisTokenValueDTO value = new RedisTokenValueDTO
            {
                UserId = user.Id,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow,
                DeviceInfo = "desktop"
            };
            await _redisCache.SetAsync<RedisTokenValueDTO>(key, value, TimeSpan.FromDays(7));

            var result = new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken
            };

            return result;
        }

        public async Task<string?> RefreshAsync(string refreshToken)
        {
            string newAccessToken = await _tokenService.GenerateNewAccessToken(refreshToken) ?? "";
            return newAccessToken;
        }
    }
}
