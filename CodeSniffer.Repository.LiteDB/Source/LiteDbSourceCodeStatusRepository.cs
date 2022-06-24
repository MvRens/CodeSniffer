using CodeSniffer.Repository.Source;
using JetBrains.Annotations;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Source
{
    public class LiteDbSourceCodeStatusRepository : BaseLiteDbRepository, ISourceCodeStatusRepository
    {
        private const string RevisionCollection = "Revision";


        public LiteDbSourceCodeStatusRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        public static ValueTask Initialize(ILiteDatabase database)
        {
            var revisionCollection = database.GetCollection<RevisionRecord>(RevisionCollection);
            revisionCollection.EnsureIndex(r => r.RevisionId);

            return default;
        }


        public async ValueTask<bool> HasRevision(string sourceCodeRepositoryId, string revisionId)
        {
            using var connection = await GetConnection();
            var revisionCollection = connection.Database.GetCollection<RevisionRecord>(RevisionCollection);

            return revisionCollection.Exists(r => r.RevisionId == revisionId && r.RepositoryId == sourceCodeRepositoryId);
        }


        public async ValueTask StoreRevision(string sourceCodeRepositoryId, string revisionId)
        {
            using var connection = await GetConnection();
            var revisionCollection = connection.Database.GetCollection<RevisionRecord>(RevisionCollection);

            if (revisionCollection.Exists(r => r.RevisionId == revisionId && r.RepositoryId == sourceCodeRepositoryId))
                return;

            revisionCollection.Insert(new RevisionRecord(ObjectId.NewObjectId(), sourceCodeRepositoryId, revisionId));
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class RevisionRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string RepositoryId { get; }
            public string RevisionId{ get; }


            [BsonCtor]
            public RevisionRecord(ObjectId id, string repositoryId, string revisionId)
            {
                Id = id;
                RepositoryId = repositoryId;
                RevisionId = revisionId;
            }
        }
    }
}