using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SpeedwayTyperApp.Server.Controllers;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;
using System.Threading.Tasks;

namespace SpeedwayTyperApp.Server.Tests
{
    public class AdminControllerTests
    {
        private static Mock<UserManager<UserModel>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<UserModel>>();
            return new Mock<UserManager<UserModel>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenUserIsCreated()
        {
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.CreateAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.AddToRoleAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success);

            var inviteService = new Mock<IInviteService>();
            var userRepository = new Mock<IUserRepository>();
            var controller = new AdminController(userManager.Object, inviteService.Object, userRepository.Object);

            var model = new RegisterModel
            {
                Username = "newadmin",
                Email = "admin@example.com",
                Password = "Password123!",
                Role = "Admin"
            };

            var result = await controller.Register(model);

            Assert.IsInstanceOf<OkResult>(result);
            userManager.Verify(m => m.CreateAsync(It.Is<UserModel>(u => u.UserName == model.Username && u.Email == model.Email), model.Password), Times.Once);
            userManager.Verify(m => m.AddToRoleAsync(It.Is<UserModel>(u => u.UserName == model.Username), model.Role), Times.Once);
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenCreationFails()
        {
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.CreateAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid" }));

            var inviteService = new Mock<IInviteService>();
            var userRepository = new Mock<IUserRepository>();
            var controller = new AdminController(userManager.Object, inviteService.Object, userRepository.Object);

            var model = new RegisterModel
            {
                Username = "user",
                Email = "user@example.com",
                Password = "Password123!",
                Role = "User"
            };

            var result = await controller.Register(model);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            userManager.Verify(m => m.AddToRoleAsync(It.IsAny<UserModel>(), It.IsAny<string>()), Times.Never);
        }
    }
}
