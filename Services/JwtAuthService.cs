using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RestaurantOrderSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace RestaurantOrderSystem.Services
{
    public class JwtOptions
    {
        public string Secret { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int ExpirationMinutes { get; set; } = 120;
    }

    public interface IJwtAuthService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
        Task SignInWithCookieAsync(HttpContext httpContext, User user);
    }

    public class JwtAuthService : IJwtAuthService
    {
        private readonly JwtOptions _options;

        public JwtAuthService(IOptions<JwtOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.Secret))
            {
                _options.Secret = "fallback-secret-key-for-development-12345";
            }

            if (_options.ExpirationMinutes <= 0)
            {
                _options.ExpirationMinutes = 120;
            }
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: string.IsNullOrWhiteSpace(_options.Issuer) ? null : _options.Issuer,
                audience: string.IsNullOrWhiteSpace(_options.Audience) ? null : _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
                ValidateIssuer = !string.IsNullOrWhiteSpace(_options.Issuer),
                ValidIssuer = _options.Issuer,
                ValidateAudience = !string.IsNullOrWhiteSpace(_options.Audience),
                ValidAudience = _options.Audience,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, parameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task SignInWithCookieAsync(HttpContext httpContext, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );
        }
    }
}
