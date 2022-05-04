namespace CodeSniffer.Repository.Checks
{
    public class CsDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public int Version { get; }
        public string Author { get; }
        public IReadOnlyList<CsDefinitionSource> Sources { get; }
        public IReadOnlyList<CsDefinitionCheck> Checks { get; }


        public CsDefinition(string id, string name, int version, string author, IReadOnlyList<CsDefinitionSource> sources, IReadOnlyList<CsDefinitionCheck> checks)
        {
            Id = id;
            Name = name;
            Version = version;
            Author = author;
            Sources = sources;
            Checks = checks;
        }
    }
}
