using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskFlowBackend.DTOs;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IRedisCacheService _redis;
        private readonly IUserRepository _userRepo;

        public TokenService(IConfiguration config, IRedisCacheService redis, IUserRepository userRepo)
        {
            _config = config;
            _redis = redis;
            _userRepo = userRepo;
        }

        // Generating JWT token for the user
        public string GenerateAccessToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiry = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Aud, jwtSettings["Audience"] ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Iss, jwtSettings["Issuer"] ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.UtcNow.AddMinutes(expiry).ToString(), ClaimValueTypes.Integer64),
                new Claim("avatarUrl", user.AvatarUrl ?? string.Empty),
                new Claim("title", user.Title ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Regenerating new Access token upon verifying refresh token
        public async Task<string?> GenerateNewAccessToken(string refreshToken)
        {
            string key = $"refresh_token:{refreshToken}";
            bool isExists = await _redis.ExistsAsync(key);

            // Check if the refreshtoken is still in redis
            if (!isExists)
            {
                return default;
            }

            var value = await _redis.GetAsync<RedisTokenValueDTO>(key);
            Guid userId = value!.UserId;

            // Fetching user from Userid
            var user = await _userRepo.GetUserByIdAsync(userId);

            if(user is null)
            {
                return default;
            }

            string newAccessToken = GenerateAccessToken(user);

            return newAccessToken;
        }

        // Verify the token and return true if verified else false
        public bool VerifyToken(string token)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key
            };

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Generating a random refresh token
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            RandomNumberGenerator.Fill(randomBytes);
            string refreshToken = Convert.ToBase64String(randomBytes);
            return refreshToken;
        }
    }
}
