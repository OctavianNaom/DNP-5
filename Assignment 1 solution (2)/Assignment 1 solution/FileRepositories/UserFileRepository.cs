using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : IUserRepository
{
    private const string FilePath = "users.json";

    public UserFileRepository()
    {
        if (!File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, "[]");
        }
    }

    public async Task<User> AddAsync(User user)
    {
        List<User> users = await LoadUsers();
        user.Id = users.Count > 0 ? users.Max(c => c.Id) + 1 : 1;
        users.Add(user);
        await SaveList(users);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        List<User> users = await LoadUsers();
        User? existingUser = users.SingleOrDefault(c => c.Id == user.Id);
        if (existingUser is null)
        {
            throw new NotFoundException($"User with ID '{user.Id}' not found");
        }
        
        users.Remove(existingUser);
        users.Add(user);
        
        await SaveList(users);
    }

    public async Task DeleteAsync(int id)
    {
        List<User> users = await LoadUsers();
        User? userToRemove = users.SingleOrDefault(c => c.Id == id);
        if (userToRemove is null)
        {
            throw new NotFoundException($"User with ID '{id}' not found");
        }
        
        users.Remove(userToRemove);
        await SaveList(users);
    }

    public async Task<User> GetSingleAsync(int id)
    {
        List<User> users = await LoadUsers();
        User? user = users.SingleOrDefault(c => c.Id == id);

        if (user is null) throw new NotFoundException($"User with ID '{id}' not found");
        
        return user;
    }

    public IQueryable<User> GetMany()
        => LoadUsers().Result.AsQueryable();

    private static Task SaveList(List<User> users)
    {
        string usersAsJson = ListToJson(users);
        return JsonToFileAsync(usersAsJson);
    }

    private static Task JsonToFileAsync(string json)
        => File.WriteAllTextAsync(FilePath, json);

    private static string ListToJson(List<User> list)
        => JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });

    private static async Task<List<User>> LoadUsers()
    {
        string usersAsJson = await ReadJsonAsync();
        return JsonToUserList(usersAsJson);
    }

    private static List<User> JsonToUserList(string usersAsJson)
        => JsonSerializer.Deserialize<List<User>>(usersAsJson)!;

    private static Task<string> ReadJsonAsync()
        => File.ReadAllTextAsync(FilePath);
}