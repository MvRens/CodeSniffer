using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CodeSniffer.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Authentication
{
    [ApiController]
    [Route("/api/login")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationProvider authenticationProvider;


        public AuthenticationController(IAuthenticationProvider authenticationProvider)
        {
            this.authenticationProvider = authenticationProvider;
        }


        [HttpPost]
        public async ValueTask<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var token = await authenticationProvider.Validate(request.Username, request.Password);
            if (token == null)
                return Forbid();

            return Ok(new LoginResponse(token));
        }


        [HttpPost("validate")]
        [Authorize]
        public ActionResult Validate()
        {
            return Ok();
        }
    }


    public class LoginRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; } = null!;

        [Required(AllowEmptyStrings = false)] 
        public string Password { get; set; } = null!;
    }


    public class LoginResponse
    {
        public string Token { get; }


        public LoginResponse(string token)
        {
            Token = token;
        }
    }
}
