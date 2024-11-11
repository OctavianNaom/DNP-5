using System.Security.Claims;
using ApiContracts;
using ApiContracts.Auth;
using ApiContracts.User;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository userRepository;

    public AuthController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
    {
        User? user = userRepository.GetMany().SingleOrDefault(u => u.UserName == request.UserName);
        if (user == null)
        {
            return Unauthorized();
        }

        if (user.Password != request.Password)
        {
            return Unauthorized();
        }

        UserDto userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName
        };


        return userDto;
    }
}