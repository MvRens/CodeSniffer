namespace CodeSniffer.Repository.LiteDB
{
    public class BaseLiteDbRepository
    {
        private readonly ILiteDbConnectionPool connectionPool;
        private readonly ILiteDbConnectionString connectionString;


        public BaseLiteDbRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
        {
            this.connectionPool = connectionPool;
            this.connectionString = connectionString;
        }



        protected ValueTask<ILiteDbPooledConnection> GetConnection()
        {
            return connectionPool.Connect(connectionString.ConnectionString);
        }
    }
}