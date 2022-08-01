namespace CodeSniffer.API.Admin
{
    public class RoleViewModel
    {
        public string Id { get; }
        public string Name { get; }


        public RoleViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
