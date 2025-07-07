using Ecommerce.Core.Controllers;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc.ViewFeatures; 
using Moq;
using Xunit;
using System.Security.Claims;
using System.Text;

/*
    unit thesting for Login controller
    - unit testing works for test single unit (method) which is independent from dependencies or infrastructure - thats what integration testing are for
    - doesn't communicate with file system or data base
    - works only for the test of the code

    Mocking methods
    - it means creating fake implementation of methods to simulate their behavior in 
    tests, isolating the code being tested from real dependencies or sessions

    [Fact] : Attribute that is applied to a method to indicate that it is a fact that should be 
             run by the test runner. 
             It can also be extended to support a customized definition of a test method.

    how does this methods works 

    Arrange:
        - set up the test environment, including mocks, objects, and initial conditions
        -- for mock services need to manage by .Setup and .Returns methods of mock<ISession>
    Act: 
        - Execute the method or action being tested
    Assert:
        - Verify the outcomes matches the expected result or not
*/


namespace Ecommerce.Tests.Controllers
{
    public class LoginControllerTests
    {
        private readonly Mock<ILogger<LoginController>> _mockLogger;
        private readonly Mock<IUserService> _mockUserService;
        private readonly LoginController _controller;

        public LoginControllerTests()
        {
            _mockLogger = new Mock<ILogger<LoginController>>();
            _mockUserService = new Mock<IUserService>();

            // Set up controller with a mocked HttpContext
            DefaultHttpContext httpContext = new DefaultHttpContext();

            // Mock ISession
            Mock<ISession> mockSession = new Mock<ISession>();
            httpContext.Session = mockSession.Object;

            _controller = new LoginController(_mockLogger.Object, _mockUserService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            // Set up TempData
            _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
        }


        /// <summary>
        /// Index get method testing : case
        /// when there is no auth token available it should 
        /// redirect with tempdata{error message} to the index view
        /// </summary>
        [Fact]
        public void Index_Get_NoAuthToken_ReturnsView()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Mock session to return no token
            Mock<ISession> mockSession = new Mock<ISession>();
            byte[] sessionToken = null!;
            mockSession.Setup(s => s.TryGetValue("auth_token", out sessionToken!)).Returns(false);
            _controller.ControllerContext.HttpContext.Session = mockSession.Object;

            // Act
            IActionResult result = _controller.Index();

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        /// <summary>
        /// index get method testing : case
        /// if the authentication is successfull and there is user available 
        /// then it should redirect to dashboard/usersDashboard
        /// </summary>
        [Fact]
        public void Index_Get_AuthenticatedAdmin_RedirectsToBuyerDashboard()
        {
            // Arrange
            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.Role, "Seller")
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            // Mock session 
            Mock<ISession> mockSession = new Mock<ISession>();
            byte[] sessionToken = Encoding.UTF8.GetBytes("valid_token");
            mockSession.Setup(s => s.TryGetValue("auth_token", out sessionToken!)).Returns(true);
            _controller.ControllerContext.HttpContext.Session = mockSession.Object;

            // Act
            IActionResult result = _controller.Index();

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserDashboard", redirectResult.ActionName);
            Assert.Equal("Dashboard", redirectResult.ControllerName);
        }

        /// <summary>
        /// index post method : case
        /// if loginviewmodel is null then it should showcase tempdata(error)
        /// redirect to index with model data
        /// </summary>
        [Fact]
        public void Index_Post_NullModel_ReturnsViewWithError()
        {
            // Arrange
            LoginViewModel model = null!;

            // Act
            IActionResult result = _controller.Index(model);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.Equal("Email and Password are required.", _controller.TempData["ErrorMessage"]);
        }

        /// <summary>
        /// index post method : case
        /// if the model state is invalid then also throw error and tempdata(error)
        /// and redirect to index view with model data
        /// </summary>
        [Fact]
        public void Index_Post_InvalidModelState_ReturnsViewWithError()
        {
            // Arrange
            LoginViewModel model = new LoginViewModel { Email = "test@example.com", Password = "password123" };
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            IActionResult result = _controller.Index(model);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("Invalid user credentials", _controller.TempData["ErrorMessage"]);
        }

        /// <summary>
        /// index post method : case
        /// if login happens successfully then should redirect to Login/index
        /// </summary>
        [Fact]
        public void Index_Post_ValidModel_SuccessfulLogin_RedirectsToLogin()
        {
            // Arrange
            LoginViewModel model = new LoginViewModel { Email = "test@example.com", Password = "password123" };
            ResponseTokenViewModel responseToken = new ResponseTokenViewModel
            {
                token = "jwt_token",
                isPersistent = true
            };
            _mockUserService.Setup(s => s.UserLogin(model)).Returns(responseToken);

            // Mock session
            Mock<ISession> mockSession = new Mock<ISession>();
            _controller.ControllerContext.HttpContext.Session = mockSession.Object;

            // Mock session Set method to avoid exceptions
            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

            // Act
            IActionResult result = _controller.Index(model);

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Login", redirectResult.ControllerName);
            Assert.Equal("User logged in successfully!", _controller.TempData["SuccessMessage"]);
        }

        /// <summary>
        /// index post method : case
        /// if there are invalid credentials then it should pass tempdata(error)
        /// return to view of index with model
        /// </summary>
        [Fact]
        public void Index_Post_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            LoginViewModel model = new LoginViewModel { Email = "test@example.com", Password = "wrong" };
            _mockUserService.Setup(s => s.UserLogin(model)).Returns(new ResponseTokenViewModel { token = null });

            // Act
            IActionResult result = _controller.Index(model);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("Invalid user credentials, please try again!", _controller.TempData["ErrorMessage"]);
        }

        /// <summary>
        /// index post method : case
        /// if there is exception then it should pass tempdata(Error) with error result
        /// and redirect to the view of index with model
        /// </summary>
        [Fact]
        public void Index_Post_Exception_ReturnsViewWithError()
        {
            // Arrange
            LoginViewModel model = new LoginViewModel { Email = "test@example.com", Password = "password123" };
            _mockUserService.Setup(s => s.UserLogin(model)).Throws(new Exception("Login failed"));

            // Act
            IActionResult result = _controller.Index(model);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("Login failed", _controller.TempData["ErrorMessage"]);
        }


        
    }
}
