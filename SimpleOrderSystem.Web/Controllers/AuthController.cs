using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SimpleOrderSystem.Data.Services;
using SimpleOrderSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleOrderSystem.Web.Security;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService service;
        private readonly JwtHelper jwtHelper;
        private readonly ICurrentUser currentUser;

        public AuthController(UserService service, JwtHelper jwtHelper, ICurrentUser currentUser)
        {
            this.service = service;
            this.jwtHelper = jwtHelper;
            this.currentUser = currentUser;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await service.AuthenticateAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Email or password is incorrect." });
            }
            return Ok(jwtHelper.CreateToken(user));
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            return Ok(await service.GetActiveUserAsync(currentUser.UserId));
        }
    }

    public class LoginRequest
    {
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Required, StringLength(128)]
        public string Password { get; set; }
    }
}
