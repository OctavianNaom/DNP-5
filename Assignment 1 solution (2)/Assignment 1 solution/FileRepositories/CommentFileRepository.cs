using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class CommentFileRepository : ICommentRepository
{
    private const string FilePath = "comments.json";
    
    public CommentFileRepository()
    {
        // if no file exist, create a new with an empty list in it.
        if (!File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, "[]"); // this is the "symbol" for an empty list in json. Alternatively, you can create a new list and serialize it to json. It's just more work.
        }
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        List<Comment> comments = await LoadCommentsAsync(); // calling helper method below. All my methods need the same way of loading and saving from/to file.
        comment.Id = comments.Count > 0 ? comments.Max(c => c.Id) + 1 : 1;
        comments.Add(comment);
        await SaveCommentsAsync(comments); // again, a helper method. See below.
        return comment;
    }

    // This is just a helper method. I did the same code in all public methods. I didn't want to duplicate that all over.
    // It is static, because it does not depend on any instance variables (the FilePath field is const).
    // Making it static makes the intent and usage clearer. Sometimes there is also a slight performance boost.
    // Rider will suggest to make it static, by putting five (green?) dots in under the method name. You can press Alt+Enter to get the suggestion.
    private static Task SaveCommentsAsync(List<Comment> comments)
    {
        string commentsAsJson = JsonSerializer.Serialize(comments, new JsonSerializerOptions { WriteIndented = true }); // The options make the json better formatted in the file, for easier readability.
        return File.WriteAllTextAsync(FilePath, commentsAsJson); // instead of awaiting the task, I can just return it. The caller of this method can await it instead. E.g. see the above method.
    }

    // This is just a helper method. I did the same code in all public methods. I didn't want to duplicate that all over.
    // See comment above.
    private static async Task<List<Comment>> LoadCommentsAsync()
    {
        string commentsAsJson = await File.ReadAllTextAsync(FilePath);
        List<Comment> comments = JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)!; // The exclamation mark at the end is to suppress the nullable warning. Basically it converts from List<Comment>? to List<Comment>. I can use it when I am certain I don't read null from the file.
        return comments;
    }

    public async Task UpdateAsync(Comment comment)
    {
        List<Comment> comments = await LoadCommentsAsync();
        Comment existingComment = await GetSingleAsync(comment.Id); // I am using the GetSingleAsync method below, to get the existing comment. It throws an exception if the comment is not found. This is, again, to reduce code duplication.

        comments.Remove(existingComment);
        comments.Add(comment);
        
        await SaveCommentsAsync(comments); // Instead of await, I could replace it with return, and let the caller await it. E.g. see the above SaveCommentsAsync method.
    }

    public async Task DeleteAsync(int id)
    {
        List<Comment> comments = await LoadCommentsAsync();

        // As above, I could call GetSingleAsync(). But I also want to show the naive way of doing it. By duplicating code.
        Comment? commentToRemove = comments.SingleOrDefault(c => c.Id == id);
        if (commentToRemove is null)
        {
            throw new NotFoundException($"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);

        await SaveCommentsAsync(comments);
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        List<Comment> comments = await LoadCommentsAsync();
        Comment? comment = comments.SingleOrDefault(c => c.Id == id);
        if (comment is null)
        {
            throw new NotFoundException($"Comment with ID '{id}' not found");
        }

        return comment;
    }

    public IQueryable<Comment> GetMany()
        => LoadCommentsAsync().Result.AsQueryable();
    // feeling fancy with "expression body" syntax. This can be done with one-line-methods.
    // This method is not async, so I can't await the LoadCommentsAsync() call. Instead I can call Result on the Task, which blocks the thread until the task is done. This is generally not recommended, as you loose the asynchronous benefit, but it's fine for this example. When the DB is added, it will not be a problem.
    // Have a look in another *FileRepo for another approach, which you may be more comfortable with.
}