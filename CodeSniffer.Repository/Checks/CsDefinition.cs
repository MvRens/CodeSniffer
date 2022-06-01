namespace CodeSniffer.Repository.Checks
{
    public class CsDefinition
    {
        public string Name { get; }
        public IReadOnlyList<CsDefinitionSource> Sources { get; }
        public IReadOnlyList<CsDefinitionCheck> Checks { get; }


        public CsDefinition(string name, IReadOnlyList<CsDefinitionSource> sources, IReadOnlyList<CsDefinitionCheck> checks)
        {
            Name = name;
            Sources = sources;
            Checks = checks;
        }
    }


    public class CsStoredDefinition : CsDefinition
    {
        public string Id { get; }
        public int Version { get; }
        public string Author { get; }


        public CsStoredDefinition(string id, string name, int version, string author, IReadOnlyList<CsDefinitionSource> sources, IReadOnlyList<CsDefinitionCheck> checks)
            : base(name, sources, checks)
        {
            Id = id;
            Version = version;
            Author = author;
        }
    }
}
