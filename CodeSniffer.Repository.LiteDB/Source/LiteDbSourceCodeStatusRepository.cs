using CodeSniffer.Repository.Source;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Source
{
    public class LiteDbSourceCodeStatusRepository : BaseLiteDbRepository, ISourceCodeStatusRepository
    {
        //private const string ReportCollection = "Report";


        public LiteDbSourceCodeStatusRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        public static ValueTask Initialize(ILiteDatabase database)
        {
            // TODO

            /*
            var reportCollection = database.GetCollection<ReportRecord>(ReportCollection);
            reportCollection.EnsureIndex(r => r.DefinitionId);

            var reportArchiveCollection = database.GetCollection<ReportRecord>(ReportArchiveCollection);
            reportArchiveCollection.EnsureIndex(r => r.DefinitionId);
            */

            return default;
        }


        public ValueTask<bool> HasRevision(string sourceCodeRepositoryId, string revisionId)
        {
            // TODO
            return new ValueTask<bool>(false);
        }


        public ValueTask StoreRevision(string sourceCodeRepositoryId, string revisionId)
        {
            // TODO
            return default;
        }
    }
}