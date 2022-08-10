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


        public async ValueTask<IReadOnlyList<RevisionDefinition>> GetRevisionDefinitions(string sourceId,
            string revisionId)
        {
            using var connection = await GetConnection();
            var revisionCollection = connection.Database.GetCollection<RevisionRecord>(RevisionCollection);

            var revision = revisionCollection.FindOne(r => r.RevisionId == revisionId && r.SourceId == sourceId);
            if (revision == null)
                return Array.Empty<RevisionDefinition>();

            return revision.Definitions
                .Select(d => new RevisionDefinition(d.DefinitionId, d.Version))
                .ToList();
        }


        public async ValueTask StoreRevision(string sourceId, string revisionId, IReadOnlyList<RevisionDefinition> definitions)
        {
            using var connection = await GetConnection();
            var revisionCollection = connection.Database.GetCollection<RevisionRecord>(RevisionCollection);

            if (revisionCollection.Exists(r => r.RevisionId == revisionId && r.SourceId == sourceId))
                return;

            revisionCollection.Insert(new RevisionRecord(ObjectId.NewObjectId(), sourceId, revisionId, 
                definitions.Select(d => new RevisionDefinitionRecord(d.DefinitionId, d.Version)).ToArray()));
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class RevisionRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string SourceId { get; }
            public string RevisionId{ get; }

            public RevisionDefinitionRecord[] Definitions { get; }


            [BsonCtor]
            public RevisionRecord(ObjectId id, string sourceId, string revisionId, BsonArray definitions)
            {
                Id = id;
                SourceId = sourceId;
                RevisionId = revisionId;
                Definitions = definitions.ToArray<RevisionDefinitionRecord>();
            }


            public RevisionRecord(ObjectId id, string sourceId, string revisionId, RevisionDefinitionRecord[] definitions)
            {
                Id = id;
                SourceId = sourceId;
                RevisionId = revisionId;
                Definitions = definitions;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class RevisionDefinitionRecord
        {
            public string DefinitionId { get; }
            public int Version { get; }


            [BsonCtor]
            public RevisionDefinitionRecord(string definitionId, int version)
            {
                DefinitionId = definitionId;
                Version = version;
            }
        }
    }
}