using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;

    public CliApp(IUserRepository userRepository, ICommentRepository commentRepository, IPostRepository postRepository)
    {
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
    }

    public async Task StartAsync()
    {
        await StartMainMenu();

        Console.WriteLine("Exiting app...");
    }

    // This is responsible for printing out the main menu and handling the user's choice.
    private async Task StartMainMenu()
    {
        while (true)
        {
            PrintMainMenu();

            string? selectedOption = Console.ReadLine();

            // read the selected option, and either instantiate the view and show it, or use "<" to exit (return, which exists to the main method, and terminates the program).
            switch (selectedOption)
            {
                case "1":
                    // instantiate the view for manage posts, and show it.
                    ManagePostsView managePostsView = new (postRepository, commentRepository, userRepository);
                    await managePostsView.ShowAsync();
                    break;
                case "2":
                    ManageUsersView manageUsersView = new (userRepository);
                    await manageUsersView.ShowAsync();
                    break;
                case "<": return;
                default:
                    // in case the input was not matched, try again.
                    Console.WriteLine("Invalid option, please try again.\n\n");
                    break;
            }
        }
    }

    private static void PrintMainMenu()
    {
        // The three double quotes are used to write a multiline string in C#, which keeps the formatting.
        const string menuOptions = """
                                   Please select:
                                   1) Manage posts
                                   2) Manage users
                                   <) Exit application
                                   """;
        Console.WriteLine(menuOptions);
    }
}