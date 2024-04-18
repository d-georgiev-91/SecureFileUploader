using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SecureFileUploader.Data;
using SecureFileUploader.Services.Exceptions;
using SecureFileUploader.Services.Models;
using System.Security.Authentication;

namespace SecureFileUploader.Services.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private IUnitOfWork _unitOfWork;
        private IPasswordHasher<User> _passwordHasher;
        private UserService _userService;

        [SetUp]
        public void Setup()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _passwordHasher = Substitute.For<IPasswordHasher<User>>();
            _userService = new UserService(_unitOfWork, _passwordHasher);
        }

        [Test]
        public void LoginAsync_NonExistingUser_ThrowsInvalidCredentialException()
        {
            _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).ReturnsNull();
            var user = new User() { Username = "username", Password = "password" };

            Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.LoginAsync(user));
        }

        [Test]
        public async Task LoginAsync_InvalidPassword_ReturnsFalse()
        {
            var userEntity = new Data.Entities.User { Username = "username", PasswordHash = "passwordhash" };
            _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(userEntity);
            _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).Returns(PasswordVerificationResult.Failed);

            var user = new User { Username = "username", Password = "password" };
            var isLogged = await _userService.LoginAsync(user);

            Assert.That(isLogged, Is.False);
        }

        [Test]
        public async Task LoginAsync_ValidPassword_ReturnsTrue()
        {
            var userEntity = new Data.Entities.User { Username = "username", PasswordHash = "passwordhash" };
            _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(userEntity);
            _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).Returns(PasswordVerificationResult.Success);

            var user = new User { Username = "username", Password = "password" };
            var isLogged = await _userService.LoginAsync(user);

            Assert.That(isLogged, Is.True);
        }

        [Test]
        public void RegisterAsync_ForExistingUser_ThrowsUserAlreadyRegisteredException()
        {
            var userEntity = new Data.Entities.User { Username = "username", PasswordHash = "passwordhash" };
            _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(userEntity);

            var user = new User { Username = "username", Password = "password" };

            Assert.ThrowsAsync<UserAlreadyRegisteredException>(() => _userService.RegisterAsync(user));
        }

        [Test]
        public async Task RegisterAsync_ForNewUser_CreatesUserWithHashedPassword()
        {
            var passwordHash = "passwordhash";
            _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).ReturnsNull();
            _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>()).Returns(passwordHash);

            var user = new User { Username = "username", Password = "password" };

            await _userService.RegisterAsync(user);

            _passwordHasher.Received().HashPassword(Arg.Is(user), Arg.Is(user.Password));
            await _unitOfWork.UserRepository.Received()
                .AddAsync(Arg.Is<Data.Entities.User>(u => u.Username == user.Username && u.PasswordHash == passwordHash));
            await _unitOfWork.Received().CommitAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWork.Dispose();
        }
    }
}
