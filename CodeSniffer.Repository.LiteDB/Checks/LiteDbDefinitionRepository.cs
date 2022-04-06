using CodeSniffer.Repository.Checks;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Checks
{
    public class LiteDbDefinitionRepository : BaseLiteDbRepository, IDefinitionRepository
    {
        private const string DefinitionCollection = "Definition";

        public LiteDbDefinitionRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        
        public static ValueTask Initialize(ILiteDatabase database)
        {
            database.GetCollection(DefinitionCollection);
            return default;
        }


        public ValueTask<IReadOnlyList<CsDefinition>> GetAllDetails()
        {
            throw new NotImplementedException();
        }


        public ValueTask<IReadOnlyList<ListDefinition>> List()
        {
            throw new NotImplementedException();
        }


        public ValueTask<CsDefinition> GetDetails(string id)
        {
            throw new NotImplementedException();
        }
    }
}