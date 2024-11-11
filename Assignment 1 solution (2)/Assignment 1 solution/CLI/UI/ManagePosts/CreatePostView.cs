using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository postRepository;

    public CreatePostView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public Task ShowAsync()
    {
        Console.WriteLine();
        return CreatePostAsync(); // this method returns a Task. I don't need to await the task here, so I just pass it on to the caller to await it instead.
    }

    // I don't need to await anything in this method, so it's not async. But it does return a Task, which can be awaited elsewhere, when needed.
    private Task CreatePostAsync()
    {
        while (true)
        {
            Console.WriteLine("You are creating a post.");
            Console.WriteLine("Please insert post title:");
            string? title = null;
            
            // keep asking for input, until it's not empty.
            // if < is entered, cancel the post creation.
            while (string.IsNullOrEmpty(title))
            {
                title = Console.ReadLine();
                if (string.IsNullOrEmpty(title))
                {
                    Console.WriteLine("Title cannot be empty.");
                    continue;
                }

                if ("<".Equals(title))
                {
                    Console.WriteLine("Post creation cancelled.");
                    
                    // return a completed task, to indicate that the post creation was cancelled. Normally I would just "return;", but this is a Task-returning method, so I need to return a Task.
                    return Task.CompletedTask; 
                }
            }

            Console.WriteLine("Please insert post content:");
            string? content = null;
            
            // again, keep asking for input, until it's not empty. And check for exit.
            while (string.IsNullOrEmpty(content))
            {
                content = Console.ReadLine();
                if (string.IsNullOrEmpty(content))
                {
                    Console.WriteLine("Content cannot be empty.");
                    continue;
                }

                if ("<".Equals(content))
                {
                    Console.WriteLine("Post creation cancelled.");
                    return Task.CompletedTask;
                }
            }

            Console.WriteLine("Please insert the ID of the user that created the post:");

            int userId;

            // You should recognize this pattern by now.
            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("ID cannot be empty.");
                    continue;
                }

                if ("<".Equals(input))
                {
                    Console.WriteLine("User creation cancelled.");
                    return Task.CompletedTask;
                }

                if (int.TryParse(input, out userId))
                {
                    // TODO check if user exists
                    break;
                }
                else
                {
                    Console.WriteLine("Could not parse the ID, please try again.");
                }
            }

            // Then print out the information, and ask for confirmation.
            
            Console.WriteLine("You are about to create a post.");
            Console.WriteLine("Do you want to proceed? (y/n)");
            string? confirmation = null;
            while (true)
            {
                confirmation = Console.ReadLine();
                if (string.IsNullOrEmpty(confirmation))
                {
                    Console.WriteLine("Please select an option.\n\n");
                    continue;
                }

                confirmation = confirmation.ToLower();
                if (confirmation != "y" && confirmation != "n")
                {
                    Console.WriteLine("Please select a valid option.\n\n");
                    continue;
                }

                break;
            }

            switch (confirmation.ToLower())
            {
                case "y": return AddPostAsync(title, content, userId); // This method returns a Task. I don't need to await it here, as the return will exit the method, so I just return it to the caller to await it.
                case "n":
                {
                    Console.WriteLine("User creation cancelled.");
                    return Task.CompletedTask;
                }
                default:
                    Console.WriteLine("Invalid option, please try again.\n\n");
                    break;
            }
        }
    }

    // add post to the repository.
    private async Task AddPostAsync(string title, string content, int userId)
    {
        Post post = new(title, content, userId);
        Post added = await postRepository.AddAsync(post);
        Console.WriteLine("Post created successfully, with Id: " + added.Id);
    }
}