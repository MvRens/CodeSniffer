namespace CodeSniffer.Repository.Checks
{
    public interface IDefinitionRepository
    {
        ValueTask<IReadOnlyList<CsStoredDefinition>> GetAllDetails();

        ValueTask<IReadOnlyList<ListDefinition>> List();
        ValueTask<CsStoredDefinition> GetDetails(string id);

        ValueTask<CsStoredDefinition> Insert(CsDefinition newDefinition, string author);
        ValueTask<CsStoredDefinition> Update(string id, CsDefinition newDefinition, string author);
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


    public class CsDefinition
    {
        public string Name { get; }
        public string SourceGroupId { get; }
        public IReadOnlyList<CsDefinitionCheck> Checks { get; }


        public CsDefinition(string name, string sourceGroupId, IReadOnlyList<CsDefinitionCheck> checks)
        {
            Name = name;
            SourceGroupId = sourceGroupId;
            Checks = checks;
        }
    }


    public class CsStoredDefinition : CsDefinition
    {
        public string Id { get; }
        public int Version { get; }
        public string Author { get; }


        public CsStoredDefinition(string id, string name, int version, string author, string sourceGroupId, IReadOnlyList<CsDefinitionCheck> checks)
            : base(name, sourceGroupId, checks)
        {
            Id = id;
            Version = version;
            Author = author;
        }
    }
}
