using CodeSniffer.Core.Sniffer;
using CodeSniffer.Repository.Reports;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Reports
{
    public class LiteDbReportRepository : BaseLiteDbRepository, IReportRepository
    {
        public LiteDbReportRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        public static ValueTask Initialize(ILiteDatabase database)
        {
            return default;
        }


        public ValueTask<IReadOnlyDictionary<string, CsReportResult>> GetDefinitionsStatus()
        {
            throw new NotImplementedException();
        }
    }
}