using System.ComponentModel.DataAnnotations;

namespace CodeSniffer.API.Admin
{
    public class ListUserViewModel
    {
        public string Id { get; }
        public string Username { get; }
        public string DisplayName { get; }


        public ListUserViewModel(string id, string username, string displayName)
        {
            Id = id;
            Username = username;
            DisplayName = displayName;
        }
    }


    public class UserViewModel
    {
        public string Username { get; }
        public string DisplayName { get; }
        public string Email { get; }
        public string Role { get; }
        public bool Notifications { get; }


        public UserViewModel(string username, string displayName, string email, string role, bool notifications)
        {
            Username = username;
            DisplayName = displayName;
            Email = email;
            Role = role;
            Notifications = notifications;
        }
    }


    public class BaseUserUpdateViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        public string DisplayName { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        public string Role { get; set; } = null!;

        public bool Notifications { get; set; }
    }


    public class UserInsertViewModel : BaseUserUpdateViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; } = null!;
    }


    public class UserUpdateViewModel : BaseUserUpdateViewModel
    {
        public string? NewPassword { get; set; }
    }
}
