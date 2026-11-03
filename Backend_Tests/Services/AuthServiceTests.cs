using Microsoft.AspNetCore.Identity;
using Moq;
using MongoDB.Bson;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;
using PruebaTecnica_MonoLegal.Services;
using Xunit;

namespace PruebaTecnica_MonoLegal.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Strict: cualquier llamada sin Setup falla en lugar de devolver null.
            _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            _authService = new AuthService(_userRepository.Object);
        }

        // ---------------------------------------------------------------
        // Método auxiliar: crea un usuario con la contraseña hasheada igual
        // que lo hace AuthService (PasswordHasher<object>).
        // ---------------------------------------------------------------

        private static User CreateUser(string userName, string plainPassword)
        {
            var hasher = new PasswordHasher<object>();

            return new User
            {
                _id = ObjectId.GenerateNewId(),
                userName = userName,
                password = hasher.HashPassword(new object(), plainPassword),
                isAdmin = false
            };
        }

        // ---------------------------------------------------------------
        // Prueba 1: credenciales correctas
        // ---------------------------------------------------------------

        [Fact]
        public async Task ValidateCredentialsAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var user = CreateUser("admin", "Secreta123");

            _userRepository
                .Setup(r => r.GetByUserNameAsync("admin"))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.ValidateCredentialsAsync("admin", "Secreta123");

            // Assert: devuelve exactamente el usuario encontrado
            Assert.NotNull(result);
            Assert.Same(user, result);

            _userRepository.Verify(
                r => r.GetByUserNameAsync("admin"),
                Times.Once);
        }

        // ---------------------------------------------------------------
        // Prueba 2: usuario inexistente
        // ---------------------------------------------------------------

        [Fact]
        public async Task ValidateCredentialsAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            _userRepository
                .Setup(r => r.GetByUserNameAsync("fantasma"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.ValidateCredentialsAsync("fantasma", "Secreta123");

            // Assert
            Assert.Null(result);

            _userRepository.Verify(
                r => r.GetByUserNameAsync("fantasma"),
                Times.Once);
        }

        // ---------------------------------------------------------------
        // Prueba 3: contraseña incorrecta (varios casos)
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("incorrecta")]      // totalmente distinta
        [InlineData("secreta123")]      // mismas letras, distinta capitalización
        [InlineData("Secreta123 ")]     // con espacio al final
        [InlineData("")]                // vacía
        public async Task ValidateCredentialsAsync_WrongPassword_ReturnsNull(string wrongPassword)
        {
            // Arrange: la contraseña real es "Secreta123"
            var user = CreateUser("admin", "Secreta123");

            _userRepository
                .Setup(r => r.GetByUserNameAsync("admin"))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.ValidateCredentialsAsync("admin", wrongPassword);

            // Assert
            Assert.Null(result);

            _userRepository.Verify(
                r => r.GetByUserNameAsync("admin"),
                Times.Once);
        }

        // ---------------------------------------------------------------
        // Prueba 4: la contraseña de OTRO usuario no sirve
        // ---------------------------------------------------------------

        [Fact]
        public async Task ValidateCredentialsAsync_PasswordOfAnotherUser_ReturnsNull()
        {
            // Arrange: "admin" tiene una contraseña y otro usuario tiene otra
            var admin = CreateUser("admin", "ClaveDeAdmin1");
            var passwordDeOtro = "ClaveDeOtro2";

            _userRepository
                .Setup(r => r.GetByUserNameAsync("admin"))
                .ReturnsAsync(admin);

            // Act
            var result = await _authService.ValidateCredentialsAsync("admin", passwordDeOtro);

            // Assert
            Assert.Null(result);
        }
    }
}