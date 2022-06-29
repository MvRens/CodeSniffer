namespace CodeSniffer.Repository.Source
{
    public interface ISourceCodeStatusRepository
    {
        ValueTask<bool> HasRevision(string sourceId, string revisionId);
        ValueTask StoreRevision(string sourceId, string revisionId);
    }
}
