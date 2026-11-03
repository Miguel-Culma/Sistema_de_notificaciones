using Microsoft.AspNetCore.Identity;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;
using PruebaTecnica_MonoLegal.Services.Interfaces;

namespace PruebaTecnica_MonoLegal.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<object> _passwordHasher;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<object>();
        }


        public async Task<User?> ValidateCredentialsAsync(
            string userName,
            string password)
        {
            var user = await _userRepository.GetByUserNameAsync(userName);

            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                new object(),
                user.password,
                password
            );

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

    }
}