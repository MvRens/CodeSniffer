namespace CodeSniffer.Repository.LiteDB
{
    public class StaticLiteDbConnectionString : ILiteDbConnectionString
    {
        public string ConnectionString { get; }


        public StaticLiteDbConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
