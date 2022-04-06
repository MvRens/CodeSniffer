using CodeSniffer.Repository.LiteDB.Checks;
using CodeSniffer.Repository.LiteDB.Reports;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB
{
    public static class LiteDbRepositoryInitialization
    {
        public static async ValueTask Perform(ILiteDatabase database, string _)
        {
            await LiteDbDefinitionRepository.Initialize(database);
            await LiteDbReportRepository.Initialize(database);
        }
    }
}
