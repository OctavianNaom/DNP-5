using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class CreateUserView
{
    private readonly IUserRepository userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        await CreateUserAsync();
    }

    private async Task CreateUserAsync()
    {
        while (true)
        {
            Console.WriteLine("You are creating a user.");
            Console.WriteLine("Please insert user name:");
            string? name = null;
            while (string.IsNullOrEmpty(name))
            {
                name = Console.ReadLine();
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("Name cannot be empty.");
                    continue;
                }

                if ("<".Equals(name))
                {
                    Console.WriteLine("User creation cancelled.");
                    return;
                }
            }

            Console.WriteLine("Please insert password");
            string? password = null;
            while (string.IsNullOrEmpty(password))
            {
                password = Console.ReadLine();
                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Password cannot be empty.");
                    continue;
                }

                if ("<".Equals(name))
                {
                    Console.WriteLine("User creation cancelled.");
                    return;
                }
            }

            Console.WriteLine("You are about to create a user with the following information:");
            Console.WriteLine($"User name: {name}");
            Console.WriteLine($"Password: {password}");
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
                case "y": await AddUserAsync(name, password);
                    break;
                case "n":
                {
                    Console.WriteLine("User creation cancelled.");
                    return ;
                }
                default:
                    Console.WriteLine("Invalid option, please try again.\n\n");
                    break;
            }
        }
    }

    private async Task AddUserAsync(string name, string password)
    {
        User user = new(name, password);
        User created = await userRepository.AddAsync(user);
        Console.WriteLine("User created successfully:");
        Console.WriteLine($"ID: {created.Id}");
    }
}