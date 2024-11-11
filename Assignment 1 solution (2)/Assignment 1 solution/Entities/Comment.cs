namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; } // This has a setter, because a comment can be edited after creation.
    public int PostId { get; } // this property has no setter, because it does not make sense to modify it after creation.
    public int UserId { get; }

    // This constructor is not strictly necessary.
    // You could instead instantiate a Comment using object initializer.
    // I find that a constructor lets me instantiate an object with less code.
    // I don't set the ID, because that is done at a different time.
    public Comment(string body, int postId, int userId)
    {
        Body = body;
        PostId = postId;
        UserId = userId;
    }
}