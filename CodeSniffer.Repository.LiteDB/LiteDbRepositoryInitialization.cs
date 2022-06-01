using CodeSniffer.Repository.LiteDB.Checks;
using CodeSniffer.Repository.LiteDB.Reports;
using CodeSniffer.Repository.LiteDB.Source;
using CodeSniffer.Repository.LiteDB.Users;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB
{
    public static class LiteDbRepositoryInitialization
    {
        public static async ValueTask Perform(ILiteDatabase database, string _)
        {
            await LiteDbDefinitionRepository.Initialize(database);
            await LiteDbReportRepository.Initialize(database);
            await LiteDbSourceCodeStatusRepository.Initialize(database);
            await LiteDbUserRepository.Initialize(database);
        }
    }
}
