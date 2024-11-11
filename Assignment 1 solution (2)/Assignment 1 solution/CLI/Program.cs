using CLI.UI;
using FileRepositories;
using RepositoryContracts;

Console.WriteLine("Starting CLI app...");
// Repositories are instantiated here. Notice this version uses the FileRepositories instead of the InMemoryRepositories. That doesn't matter for you.

IUserRepository userRepository = new UserFileRepository(); // old: UserInMemoryRepository();
ICommentRepository commentRepository = new CommentFileRepository(); // old: CommentInMemoryRepository();
IPostRepository postRepository = new PostFileRepository(); // old:  PostInMemoryRepository();

CliApp cliApp = new CliApp(userRepository, commentRepository, postRepository);
await cliApp.StartAsync(); // everything is async, so we await it.