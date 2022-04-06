using LiteDB;

namespace CodeSniffer.Repository.LiteDB
{
    public delegate ValueTask InitializeDatabaseEvent(ILiteDatabase database, string connectionString);


    /// <summary>
    /// Implements a LiteDbConnectionPool which keeps connections open for at least the specified amount of time after the
    /// last connection is disposed.
    /// </summary>
    /// <remarks>
    /// Connections are reused based on an exact match of the connection string. If you are using the default Direct mode,
    /// make sure you use the same connection string each time for a single database file, and always open a connection
    /// through the pool.
    /// </remarks>
    public class LiteDbConnectionPool : ILiteDbConnectionPool
    {
        private readonly TimeSpan timeout;
        private readonly object poolLock = new();
        private readonly Dictionary<string, Connection> pool = new();
        private readonly Timer? collector;

        public InitializeDatabaseEvent? OnInitializeDatabase { get; set; }


        public LiteDbConnectionPool(TimeSpan timeout)
        {
            this.timeout = timeout;
            if (timeout <= TimeSpan.Zero) 
                return;

            // This does not guarantee an exact timeout, but that is fine
            var collectInterval = Math.Max((int)timeout.TotalMilliseconds / 4, 1000);
            collector = new Timer(CheckClosedConnections, null, collectInterval, collectInterval);
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);

            lock (poolLock)
            {
                foreach (var pair in pool)
                    pair.Value.Dispose();

                pool.Clear();
            }

            collector?.Dispose();
        }


        public async ValueTask<ILiteDbPooledConnection> Connect(string connectionString)
        {
            var requiresInitialization = false;
            ILiteDatabase? database = null;
            Connection? connection;

            // ConcurrentDictionary.GetOrAdd garandeert niet dat valueFactory maar 1x wordt aangeroepen, en dat willen we hier wel.
            // Nadeel is dat bij het aanmaken van een connectie alles gelocked wordt, maar in de praktijk is dat geen issue.
            lock (poolLock)
            {
                if (!pool.TryGetValue(connectionString, out connection))
                {
                    database = new LiteDatabase(connectionString);
                    connection = new Connection(database);
                    pool.Add(connectionString, connection);

                    requiresInitialization = true;
                }
            }

            // ReSharper disable once InvertIf
            if (requiresInitialization)
            {
                if (OnInitializeDatabase != null)
                    await OnInitializeDatabase.Invoke(database!, connectionString);

                connection.Initialize();
            }

            return await connection.Acquire();
        }


        protected void CheckClosedConnections(object? state)
        {
            var closeBefore = DateTime.Now - timeout;

            lock (poolLock)
            {
                foreach (var (connectionString, connection) in pool.Where(p => p.Value.ShouldClose(closeBefore)).ToList())
                {
                    connection.Dispose();
                    pool.Remove(connectionString);
                }
            }
        }


        private class Connection : IDisposable
        {
            private readonly ILiteDatabase database;
            private long referenceCount;
            private DateTime lastReference;

            private readonly TaskCompletionSource initializedCompletionSource = new();


            public Connection(ILiteDatabase database)
            {
                this.database = database;
            }


            public void Dispose()
            {
                GC.SuppressFinalize(this);
                database.Dispose();
            }


            public void Initialize()
            {
                initializedCompletionSource.TrySetResult();
            }


            public async ValueTask<ILiteDbPooledConnection> Acquire()
            {
                if (!initializedCompletionSource.Task.IsCompleted)
                    await initializedCompletionSource.Task;

                Interlocked.Increment(ref referenceCount);
                return new PooledConnection(this);
            }


            private void Release()
            {
                if (Interlocked.Decrement(ref referenceCount) <= 0)
                    lastReference = DateTime.Now;
            }


            public bool ShouldClose(DateTime closeBefore)
            {
                return Interlocked.Read(ref referenceCount) <= 0 && lastReference <= closeBefore;
            }


            private class PooledConnection : ILiteDbPooledConnection
            {
                public ILiteDatabase Database => owner.database;

                private readonly Connection owner;
                private bool disposed;


                public PooledConnection(Connection owner)
                {
                    this.owner = owner;
                }


                public void Dispose()
                {
                    if (disposed)
                        return;

                    GC.SuppressFinalize(this);
                    disposed = true;
                    owner.Release();
                }
            }
        }
    }
}
