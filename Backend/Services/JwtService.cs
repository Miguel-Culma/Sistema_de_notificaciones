
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PruebaTecnica_MonoLegal.Configuration;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PruebaTecnica_MonoLegal.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;

        public JwtService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.SecretKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user._id.ToString()),
                new Claim(ClaimTypes.Name, user.userName),
                new Claim(ClaimTypes.Role, user.isAdmin ? "Admin" : "Operator"),

            // Identificador único de este token
             new Claim(
            System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,
            Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _settings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
