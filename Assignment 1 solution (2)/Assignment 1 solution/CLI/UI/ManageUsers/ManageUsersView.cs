using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ManageUsersView
{
    private readonly IUserRepository userRepository;

    public ManageUsersView(IUserRepository userRepository)
    {
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
                    await new CreateUserView(userRepository).ShowAsync();
                    break;
                case "4":
                    new ListUsersView(userRepository).Show();
                    break;
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
                                   1) Create new user
                                   2) Update user
                                   3) Delete user
                                   4) View users
                                   <) Back
                                   """;
        Console.WriteLine(menuOptions);
    }
}