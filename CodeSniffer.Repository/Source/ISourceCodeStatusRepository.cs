namespace CodeSniffer.Repository.Source
{
    public interface ISourceCodeStatusRepository
    {
        ValueTask<bool> HasRevision(string sourceCodeRepositoryId, string revisionId);
        ValueTask StoreRevision(string sourceCodeRepositoryId, string revisionId);
    }
}
