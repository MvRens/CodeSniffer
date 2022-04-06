namespace CodeSniffer.ViewModel.Checks
{
    public class ListDefinitionViewModel
    {
        public string Id { get; }
        public string Name { get; }


        public ListDefinitionViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
