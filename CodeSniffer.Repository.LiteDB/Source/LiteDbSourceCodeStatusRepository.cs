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


        public async ValueTask<bool> HasRevision(string sourceId, string revisionId)
        {
            using var connection = await GetConnection();
            var revisionCollection = connection.Database.GetCollection<RevisionRecord>(RevisionCollection);

            return revisionCollection.Exists(r => r.RevisionId == revisionId && r.SourceId == sourceId);
        }


        public async ValueTask StoreRevision(string sourceId, string revisionId)
        {
            using var connection = await GetConnection();
            var revisionCollection = connection.Database.GetCollection<RevisionRecord>(RevisionCollection);

            if (revisionCollection.Exists(r => r.RevisionId == revisionId && r.SourceId == sourceId))
                return;

            revisionCollection.Insert(new RevisionRecord(ObjectId.NewObjectId(), sourceId, revisionId));
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class RevisionRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string SourceId { get; }
            public string RevisionId{ get; }


            [BsonCtor]
            public RevisionRecord(ObjectId id, string sourceId, string revisionId)
            {
                Id = id;
                SourceId = sourceId;
                RevisionId = revisionId;
            }
        }
    }
}