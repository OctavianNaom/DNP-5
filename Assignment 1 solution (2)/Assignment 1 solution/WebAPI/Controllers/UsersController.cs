using ApiContracts;
using ApiContracts.User;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public UsersController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        await VerifyUserNameIsAvailableAsync(request.UserName);

        User user = new(request.UserName, request.Password);
        User created = await userRepo.AddAsync(user);
        UserDto dto = new()
        {
            Id = created.Id,
            UserName = created.UserName
        };
        return Created($"/Users/{dto.Id}", dto); 
    }

    // This method will eventually use async when database is added.
    // You could keep it synchronous for now and update later without problems.
    private async Task VerifyUserNameIsAvailableAsync(string userName)
    {
        bool usernameIsTaken = userRepo.GetMany()
            .Any(u => u.UserName.ToLower().Equals(userName.ToLower()));
        if (usernameIsTaken)
        {
            throw new ValidationException($"Username '{userName}' is already taken");
        }
    }

    [HttpPut("{id:int}")] // putting int constraint on the route parameter. Not strictly necessary, but can be useful.
    public async Task<ActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        User existing = await userRepo.GetSingleAsync(id);

        // could validate incoming data here, or in a business logic layer
        existing.UserName = request.UserName;
        existing.Password = request.Password;
        await userRepo.UpdateAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        await userRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser([FromRoute] int id)
    {
        User user = await userRepo.GetSingleAsync(id);
        return Ok(user);
    }

    // This method takes a nullable string indicated with the question mark,
    // i.e. I explicitly state the value can be null. I also assign the parameter to null as a default value. 
    // This is not strictly necessary, but I find it adds to the readability of the code.
    // The point is that the client can optionally apply this query parameter.
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers([FromQuery] string? userNameContains = null)
    {
        // Here I chain the Where clauses together to filter the users
        IQueryable<User> users = userRepo.GetMany()
            .Where(
                u => userNameContains == null ||
                     u.UserName.ToLower().Contains(userNameContains.ToLower())
            );

        return Ok(users);
    }
    
    /*
     * Below endpoints are not strictly necessary. I can retrieve the same data from other endpoints.
     * But I want to illustrate that you can make multiple endpoints for the same data, if you want to.
     * This approach creates more dedicated, specialized endpoints, which can be easier to understand and use.
     */
    

    // I include this endpoint as an alternate example. My current Get-Many-Posts endpoint can take query parameters, to e.g. filter by user id.
    // But we could also create a dedicated endpoint for returning all posts written by a specific user.
    [HttpGet("{userId:int}/posts")]
    public async Task<ActionResult<IEnumerable<Post>>> GetPostsForUser(
        [FromRoute] int userId,
        [FromServices] IPostRepository postRepo)
    {
        List<Post> posts = postRepo.GetMany()
            .Where(p => p.UserId == userId)
            .ToList();
        return Ok(posts);
    }

    // Here is another example, for getting all comments written by a user.
    [HttpGet("{userId:int}/comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsForUser(
        [FromRoute] int userId,
        [FromServices] ICommentRepository commentRepo)
    {
        List<Comment> comments = commentRepo.GetMany()
            .Where(c => c.UserId == userId)
            .ToList();
        return Ok(comments);
    }
}