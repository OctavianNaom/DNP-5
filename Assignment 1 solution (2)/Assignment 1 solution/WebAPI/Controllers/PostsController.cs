using ApiContracts;
using ApiContracts.Comment;
using ApiContracts.Post;
using ApiContracts.User;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepo;

    public PostsController(IPostRepository postRepo)
    {
        this.postRepo = postRepo;
    }

    [HttpPost]
    public async Task<IResult> AddPost(
            [FromBody] CreatePostDto request,
            [FromServices] IUserRepository userRepo)
        // I can request a service from the DI container straight into the method, instead of through the constructor.
    {
        await VerifyAuthorExists(request.AuthorUserId, userRepo);
        // could validate incoming data here, or in a business logic layer

        Post post = new(request.Title, request.Body, request.AuthorUserId);
        Post created = await postRepo.AddAsync(post);
        return Results.Created($"/Posts/{created.Id}", created);
    }

    private static async Task VerifyAuthorExists(int userId, IUserRepository userRepo)
    {
        _ = await userRepo.GetSingleAsync(userId);
        // if no user is found, and exception is thrown from the repository
        // the underscore means we don't care about the result. The compiler will optimize it away.
    }

    [HttpPut("{id:int}")]
    public async Task<IResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        Post postToUpdate = await postRepo.GetSingleAsync(id);
        // could validate incoming data here, or in a business logic layer

        postToUpdate.Title = request.Title;
        postToUpdate.Body = request.Body;
        await postRepo.UpdateAsync(postToUpdate);
        return Results.NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        await postRepo.DeleteAsync(id);
        return NoContent();
    }

    // This endpoint is called with GET /Posts/{id}?includeComments=true, if comments should be included.
    // Or just GET /Posts/{id} if comments should not be included.


    [HttpGet("{id:int}")]
    public async Task<IResult> GetPost(
        [FromServices] IUserRepository userRepo,
        [FromServices] ICommentRepository commentRepo,
        [FromRoute] int id,
        [FromQuery] bool includeAuthor, // I don't make these two booleans nullable, because the default value of false makes sense here.
        [FromQuery] bool includeComments)
    {
        Post post = await postRepo.GetSingleAsync(id);

        // Initially my PostDto just consisted of Id, Title, Body, UserId. No Author object or list of Comments. 
        // I thought to keep it simple. The client would then have to make additional requests to get the author and comments.

        // I then changed my mind, and have added two query parameters,
        // so the client can optionally specify if they want the author and/or comments included.
        // This is a common pattern in REST APIs. It's a way to avoid over-fetching.
        // If the client doesn't need the author or comments, they don't have to get them.

        PostDto dto = new()
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId
        };

        dto.Author = await IncludeAuthorIfRequested(userRepo, includeAuthor, post.UserId);

        dto.Comments = IncludeCommentsIfRequested(commentRepo, id, includeComments);

        return Results.Ok(dto);
    }

    private static async Task<UserDto?> IncludeAuthorIfRequested(IUserRepository userRepo, bool includeAuthor, int authorId)
    {
        if (!includeAuthor) return null;
        User author = await userRepo.GetSingleAsync(authorId);
        return new UserDto
        {
            Id = author.Id,
            UserName = author.UserName
        };
    }

    private static List<CommentDto> IncludeCommentsIfRequested(ICommentRepository commentRepo, int id, bool includeComments)
    {
        if (!includeComments) return [];
        return commentRepo.GetMany()
            .Where(c => c.PostId == id)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                AuthorUserId = c.UserId,
            })
            .ToList();
    }


    [HttpGet]
    public async Task<IResult> GetPosts([FromQuery] string? titleContains = null, [FromQuery] int? userId = null)
    {
        // This is another filtering approach than in the Users Controller.
        // Either is fine. It's just to show you two different ways.
        IQueryable<Post> queryablePosts = postRepo.GetMany();
        if (titleContains != null)
        {
            queryablePosts = queryablePosts.Where(p => p.Title.Contains(titleContains));
        }

        if (userId != null)
        {
            queryablePosts = queryablePosts.Where(p => p.UserId == userId);
        }

        // using Select() here. It's a simpler way to convert a list of objects to a list of other objects.
        // See the method above, where I convert Comment to CommentDto in "old fashion approach". 
        List<PostDto> posts = queryablePosts.Select(post => new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                UserId = post.UserId
            })
            .ToList();
        return Results.Ok(posts);
    }

    // Below I have a single endpoint focused on comments.
    // This is because comments are a child entity of posts.
    // The other actions on comments, I will put in a dedicated CommentsController, to keep the code organized.
    // Adding a comment is something we do "to a post", but the other actions are more about the comment itself.
    // This is just a design choice on my part, there are different equally valid approaches.

    [HttpPost("{postId:int}/comments")] // This will result in this route: /Posts/{postId}/Comments
    public async Task<ActionResult<CommentDto>> AddComment(
        [FromRoute] int postId,
        [FromBody] CreateCommentDto request,
        [FromServices] IUserRepository userRepo,
        [FromServices] ICommentRepository commentRepo)
    {
        await VerifyPostExists(postId);
        await VerifyAuthorExists(request.AuthorUserId, userRepo);

        // could validate incoming data here, or in a business logic layer

        Comment comment = new(request.Body, request.AuthorUserId, postId);
        Comment created = await commentRepo.AddAsync(comment);
        CommentDto dto = new()
        {
            Id = created.Id,
            Body = created.Body,
            AuthorUserId = created.UserId,
            PostId = created.PostId
        };
        return Created($"/Comments/{created.Id}", dto);
        // Here we could either create an endpoint at /posts/{postId}/comments/{commentId} to get a single comment
        // or we could create an endpoint at /comments/{commentId} to get a single comment for a simpler route.
        // I have opted for the second approach. Either is fine.
    }

    private async Task VerifyPostExists(int postId)
    {
        _ = await postRepo.GetSingleAsync(postId);
        // if no post is found, and exception is thrown from the repository
        // the underscore means we don't care about the result. The compiler will optimize it away.
    }

    // This endpoint returns the comments of a specific post
    [HttpGet("{postId:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(
        [FromRoute] int postId,
        [FromServices] ICommentRepository commentRepo)
    {
        await VerifyPostExists(postId);
        IEnumerable<CommentDto> comments = commentRepo.GetMany()
            .Where(c => c.PostId == postId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                AuthorUserId = c.UserId,
                PostId = c.PostId
            })
            .AsEnumerable();
        return Ok(comments);
    }

    [HttpGet("{postId:int}/author")]
    public async Task<ActionResult<UserDto>> GetAuthorOfPost(
        [FromRoute] int postId,
        [FromServices] IUserRepository userRepo)
    {
        Post post = await postRepo.GetSingleAsync(postId);
        User author = await userRepo.GetSingleAsync(post.UserId);
        UserDto dto = new()
        {
            Id = author.Id,
            UserName = author.UserName,
        };
        return Ok(dto);
    }
}