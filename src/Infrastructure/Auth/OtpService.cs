using Application.Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Auth
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private const int OtpLength = 6;
        private const int ExpiryMinutes = 5;
        private const int MaxAttempts = 3;

        public OtpService(IMemoryCache cache) => _cache = cache;

        public Task<string> GenerateAndStoreOtpAsync(string phoneNumber)
        {
            var otp = GenerateOtp();

            var entry = new OtpEntry(otp, Attempts: 0, ExpiresAt: DateTime.UtcNow.AddMinutes(ExpiryMinutes));

            _cache.Set(CacheKey(phoneNumber), entry,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpiryMinutes)));

            return Task.FromResult(otp);
        }

        public Task<bool> ValidateOtpAsync(string phoneNumber, string otp)
        {
            var key = CacheKey(phoneNumber);

            if (!_cache.TryGetValue(key, out OtpEntry? entry) || entry is null)
                return Task.FromResult(false); // expiré ou inexistant

            if (entry.Attempts >= MaxAttempts)
            {
                _cache.Remove(key); // bloquer après trop de tentatives
                return Task.FromResult(false);
            }

            if (entry.Otp != otp)
            {
                // Incrémenter les tentatives
                _cache.Set(key, entry with { Attempts = entry.Attempts + 1 },
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(entry.ExpiresAt - DateTime.UtcNow));
                return Task.FromResult(false);
            }

            _cache.Remove(key); // OTP consommé
            return Task.FromResult(true);
        }

        private static string GenerateOtp()
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return number.ToString($"D{OtpLength}");
        }

        private static string CacheKey(string phone) => $"otp:{phone}";
    }
    // record interne
    internal record OtpEntry(string Otp, int Attempts, DateTime ExpiresAt);
}
