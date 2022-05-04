namespace CodeSniffer.Repository.Checks
{
    public interface IDefinitionRepository
    {
        ValueTask<IReadOnlyList<CsDefinition>> GetAllDetails();

        ValueTask<IReadOnlyList<ListDefinition>> List();
        ValueTask<CsDefinition> GetDetails(string id);

        ValueTask<string> Insert(CsDefinition newDefinition, string author);
        ValueTask Update(string id, CsDefinition newDefinition, string author);
        ValueTask Remove(string id, string author);
    }


    public class ListDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public int Version { get; }


        public ListDefinition(string id, string name, int version)
        {
            Id = id;
            Name = name;
            Version = version;
        }
    }
}
