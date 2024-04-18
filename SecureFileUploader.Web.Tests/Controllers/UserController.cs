using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using SecureFileUploader.Services;
using SecureFileUploader.Web.Controllers;
using SecureFileUploader.Web.Settings;

namespace SecureFileUploader.Web.Tests.Controllers
{
    public class UsersControllerTests
    {
        private IUserService _userService;
        private IOptions<JwtConfig> _jwtConfig;
        private IMapper _mapper;
        private UsersController _usersController;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _jwtConfig = Options.Create(new JwtConfig
            {
                Secret = "ThisIsAVerySecureKeyThatIsAtLeast128BitsLong",
                Issuer = "Issuer",
                Audience = "Audience",
                ExpirationInMinutes = 60
            });
            _mapper = Substitute.For<IMapper>();

            _usersController = new UsersController(_userService, _jwtConfig, _mapper);
        }

        [Test]
        public async Task Login_WhenCalledAndUserIsValid_ShouldReturnOkObjectWithToken()
        {
            var user = new Models.User { Username = "testuser", Password = "testpass" };
            _userService.LoginAsync(Arg.Any<Services.Models.User>()).Returns(Task.FromResult(true));
            _mapper.Map<Services.Models.User>(Arg.Any<Models.User>())
                .Returns(new Services.Models.User { Username = user.Username, Password = string.Empty });

            var result = await _usersController.Login(user);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value as dynamic, Is.Not.Null);
            Assert.That(okResult.Value, Has.Property("Token").Not.Null);
        }

        [Test]
        public async Task Login_WhenCalledAndUserIsNotValid_ShouldReturnUnauthorized()
        {
            var user = new Models.User { Username = "testuser", Password = "wrongpass" };
            _userService.LoginAsync(Arg.Any<Services.Models.User>()).Returns(Task.FromResult(false));

            var result = await _usersController.Login(user);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Register_WhenCalled_ShouldReturnOkResult()
        {
            var user = new Models.User { Username = "newuser", Password = "newpass" };
            _userService.RegisterAsync(Arg.Any<Services.Models.User>()).Returns(Task.CompletedTask);
            _mapper.Map<Services.Models.User>(Arg.Any<Models.User>())
                .Returns(new Services.Models.User { Username = user.Username, Password = string.Empty});

            var result = await _usersController.Register(user);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}