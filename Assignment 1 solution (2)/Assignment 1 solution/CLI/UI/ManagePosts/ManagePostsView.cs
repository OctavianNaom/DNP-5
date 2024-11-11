using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ManagePostsView
{
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IUserRepository userRepository;

    public ManagePostsView(IPostRepository postRepository, ICommentRepository commentRepository, IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        while (true)
        {
            PrintOptions();
            string input = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Please select an option.\n\n");
                continue;
            }

            if ("<".Equals(input))
            {
                return;
            }

            switch (input)
            {
                case "1":
                    await new CreatePostView(postRepository).ShowAsync();
                    break;
                case "4":
                    await new ListPostsView(postRepository, commentRepository, userRepository).ShowAsync();
                    break;
                case "<": return;
                default:
                    Console.WriteLine("Invalid option, please try again.\n\n");
                    break;
            }
        }
    }

    private static void PrintOptions()
    {
        Console.WriteLine();
        const string menuOptions = """
                                   Please select:
                                   1) Create new post
                                   2) Update post
                                   3) Delete post
                                   4) View posts
                                   <) Back
                                   """;
        Console.WriteLine(menuOptions);
    }
}