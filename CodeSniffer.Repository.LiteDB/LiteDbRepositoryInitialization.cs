using CodeSniffer.Repository.LiteDB.Checks;
using CodeSniffer.Repository.LiteDB.Reports;
using CodeSniffer.Repository.LiteDB.Source;
using CodeSniffer.Repository.LiteDB.Users;
using LiteDB;
using Serilog;

namespace CodeSniffer.Repository.LiteDB
{
    public class LiteDbRepositoryInitialization
    {
        private readonly ILogger logger;


        public LiteDbRepositoryInitialization(ILogger logger)
        {
            this.logger = logger;
        }


        public async ValueTask Perform(ILiteDatabase database, string _)
        {
            await LiteDbDefinitionRepository.Initialize(database);
            await LiteDbSourceRepository.Initialize(database);
            await LiteDbReportRepository.Initialize(database);
            await LiteDbSourceCodeStatusRepository.Initialize(database);
            await LiteDbUserRepository.Initialize(database, logger);
        }
    }
}
