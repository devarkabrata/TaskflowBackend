using System.Security.Cryptography;
using TaskFlowBackend.DTOs.Events;
using TaskFlowBackend.DTOs.Otp;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Helpers;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class OtpService : IOtpService
    {
        private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(10);

        private readonly IRedisCacheService _redisCache;
        private readonly IEventPublisherService _eventPublisher;

        public OtpService(IRedisCacheService redisCache, IEventPublisherService eventPublisher)
        {
            _redisCache = redisCache;
            _eventPublisher = eventPublisher;
        }

        public async Task<OtpGeneratedResponseDto> GenerateOtpAsync(GenerateOtpRequestDto dto)
        {
            string otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            string key = BuildKey(dto.Email, dto.Event);

            var cacheValue = new OtpCacheValueDto
            {
                Otp = otp,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _redisCache.SetAsync(key, cacheValue, OtpExpiry);

            await _eventPublisher.PublishAsync(RoutingKeys.OTPSendEmail, new OTPEvent
            {
                From = RoutingKeys.FromEmail,
                To = dto.Email,
                OTP = otp,
                Description = dto.Description,
                For = dto.Event.ToString(),
                Ttl = (int)OtpExpiry.TotalMinutes
            });

            return new OtpGeneratedResponseDto
            {
                Email = dto.Email,
                Event = dto.Event,
                ExpiresInMinutes = (int)OtpExpiry.TotalMinutes
            };
        }

        public async Task<OtpVerifiedResponseDto> VerifyOtpAsync(VerifyOtpRequestDto dto)
        {
            string key = BuildKey(dto.Email, dto.Event);
            var cacheValue = await _redisCache.GetAsync<OtpCacheValueDto>(key);

            if (cacheValue is null)
                throw new NotFoundException("OTP not found or has expired.");

            if (cacheValue.Otp != dto.Otp)
                throw new UnauthorizedException("Invalid OTP.");

            await _redisCache.DeleteAsync(key);

            return new OtpVerifiedResponseDto
            {
                Verified = true,
                Event = dto.Event
            };
        }

        private static string BuildKey(string email, OtpEventType eventType) =>
            $"otp:{email.Trim().ToLowerInvariant()}:{eventType}";
    }
}
