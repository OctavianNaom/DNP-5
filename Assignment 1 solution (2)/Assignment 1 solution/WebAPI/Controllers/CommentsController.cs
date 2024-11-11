using ApiContracts;
using ApiContracts.Comment;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;

    public CommentsController(ICommentRepository commentRepo)
    {
        this.commentRepo = commentRepo;
    }

    // This endpoint exists in the PostsController instead.
    // [HttpPost]
    // public async Task<ActionResult<Comment>> AddComment([FromBody] Comment comment)
    // {
    //     throw new NotImplementedException();
    // }

    [HttpPut]
    public async Task<ActionResult> UpdateComment([FromBody] UpdateCommentDto request)
    {
        Comment commentToUpdate = await commentRepo.GetSingleAsync(request.Id);
        // could validate incoming data here, or in a business logic layer

        commentToUpdate.Body = request.Body;

        await commentRepo.UpdateAsync(commentToUpdate);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        await commentRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetComment([FromRoute] int id)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);

        // I could probably just send back the Comment,
        // but generally it is not recommended to send the domain entities directly.
        // You might want to add some fields to the DTO, or remove some fields from the entity.
        // Or you might want to add some validation or transformation logic.
        // Or you might want to add some metadata to the response.
        // But either can work, as long as you make a well-informed/-considered decision.

        CommentDto dto = new()
        {
            Id = comment.Id,
            Body = comment.Body,
            AuthorUserId = comment.UserId,
            PostId = comment.PostId
        };
        return Ok(dto);
    }


    // You could argue that this endpoint should be in the PostsController instead.
    // In that case you "get the comments belonging to a post", with: /posts/{postId}/comments
    // But you might also want to see all comments made by a specific user.
    // Or top-10 highest rated comments of all time.
    // Or... you get the point.
    // So I consider this endpoint slightly more generic, and therefore it is in the CommentsController.
    // It's a design choice, there are many valid options.
    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments([FromQuery] int? userId = null, [FromQuery] int? postId = null)
    {
        // This time I illustrate the usage of Select().
        // This is a LINQ method that allows you to transform the data. We will see it again later in the course,
        // when we get to the database part.
        // It takes in a Comment, the 'c' variable, and returns a new CommentDto.
        // This is then done for each Comment in the list, so we get a list of CommentDtos.

        List<CommentDto> comments = commentRepo.GetMany()
            .Where(c => userId == null || c.UserId == userId)
            .Where(c => postId == null || c.PostId == postId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                AuthorUserId = c.UserId,
                PostId = c.PostId
            })
            .ToList();

        return Ok(comments);
    }
}