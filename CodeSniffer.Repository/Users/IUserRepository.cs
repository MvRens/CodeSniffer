namespace CodeSniffer.Repository.Users
{
    public interface IUserRepository
    {
        ValueTask<IReadOnlyList<ListUser>> List();
        ValueTask<CsStoredUser> GetDetails(string id);

        ValueTask<LoginUser?> ValidateLogin(string username, string password);

        ValueTask<string> Insert(CsUser newUser, string password, string author);
        ValueTask Update(string id, CsUser newUser, string? password, string author);
        ValueTask Remove(string id, string author);
    }


    public class ListUser
    {
        public string Id { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public string Email { get; }


        public ListUser(string id, string username, string displayName, string email)
        {
            Id = id;
            Username = username;
            DisplayName = displayName;
            Email = email;
        }
    }


    public class LoginUser
    {
        public string Username { get; }
        public string DisplayName { get; }
        public string Role { get; }


        public LoginUser(string username, string displayName, string role)
        {
            Username = username;
            DisplayName = displayName;
            Role = role;
        }
    }


    public class CsUser
    {
        public string Username { get; }
        public string DisplayName { get; }
        public string Email { get; }

        public string Role { get; }
        public bool Notifications { get; }


        public CsUser(string username, string displayName, string email, string role, bool notifications)
        {
            Username = username;
            DisplayName = displayName;
            Email = email;
            Role = role;
            Notifications = notifications;
        }
    }


    public class CsStoredUser : CsUser
    {
        public string Id { get; }
        public string Author { get; }


        public CsStoredUser(string id, string username, string displayName, string email, string role, bool notifications, string author) 
            : base(username, displayName, email, role, notifications)
        {
            Id = id;
            Author = author;
        }
    }
}
