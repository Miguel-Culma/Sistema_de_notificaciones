
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Services.Interfaces;

namespace PruebaTecnica_MonoLegal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly IRevokedTokenService _revokedTokenService;

        public AuthController(
            IAuthService authService,
            IJwtService jwtService,
            IRevokedTokenService revokedTokenService)
        {
            _authService = authService;
            _jwtService = jwtService;
            _revokedTokenService = revokedTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.userName) ||
                string.IsNullOrWhiteSpace(request.password))
            {
                return BadRequest(new
                {
                    message = "El usuario y la contraseña son obligatorios."
                });
            }

            var user = await _authService.ValidateCredentialsAsync(
                request.userName,
                request.password
            );

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Usuario o contraseña incorrectos."
                });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                accessToken = token,
                tokenType = "Bearer",
                expiresIn = 3600,
                user = new
                {
                    id = user._id.ToString(),
                    userName = user.userName,
                    isAdmin = user.isAdmin
                }
            });
        }


        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var tokenId = User.FindFirst(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return Unauthorized(new
                {
                    message = "El token no contiene un identificador válido."
                });
            }

            var expirationValue = User.FindFirst(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp)?.Value;

            if (!long.TryParse(expirationValue, out var expirationUnix))
            {
                return Unauthorized(new
                {
                    message = "No se pudo determinar la expiración del token."
                });
            }

            var expiration = DateTimeOffset
                .FromUnixTimeSeconds(expirationUnix)
                .UtcDateTime;

            _revokedTokenService.RevokeToken(tokenId, expiration);

            return Ok(new
            {
                message = "Sesión cerrada correctamente."
            });
        }

    }
}
