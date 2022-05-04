using System.ComponentModel.DataAnnotations;
using CodeSniffer.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Authentication
{
    [ApiController]
    [Route("/api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationProvider authenticationProvider;


        public AuthenticationController(IAuthenticationProvider authenticationProvider)
        {
            this.authenticationProvider = authenticationProvider;
        }


        [HttpPost("login")]
        public ActionResult<LoginResponse> Login(LoginRequest request)
        {
            if (!authenticationProvider.Validate(request.Username, request.Password, out var token))
                return Forbid();

            return Ok(new LoginResponse(token));
        }


        [HttpPost("login/validate")]
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
