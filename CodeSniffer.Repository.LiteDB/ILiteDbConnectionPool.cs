namespace CodeSniffer.Repository.LiteDB
{
    public interface ILiteDbConnectionPool : IDisposable
    {
        ValueTask<ILiteDbPooledConnection> Connect(string connectionString);
    }
}
