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
}
