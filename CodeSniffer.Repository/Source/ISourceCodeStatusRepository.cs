namespace CodeSniffer.Repository.Source
{
    public interface ISourceCodeStatusRepository
    {
        ValueTask<IReadOnlyList<RevisionDefinition>> GetRevisionDefinitions(string sourceId, string revisionId);
        ValueTask StoreRevision(string sourceId, string revisionId, IReadOnlyList<RevisionDefinition> definitions);
    }


    public class RevisionDefinition
    {
        public string DefinitionId { get; }
        public int Version { get; }


        public RevisionDefinition(string definitionId, int version)
        {
            DefinitionId = definitionId;
            Version = version;
        }
    }
}
