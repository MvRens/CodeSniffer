using System.Text.Json.Nodes;
using CodeSniffer.Repository.Checks;
using JetBrains.Annotations;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Checks
{
    public class LiteDbDefinitionRepository : BaseLiteDbRepository, IDefinitionRepository
    {
        private const string DefinitionCollection = "Definition";
        private const string DefinitionArchiveCollection = "DefinitionArchive";


        public LiteDbDefinitionRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        
        public static ValueTask Initialize(ILiteDatabase database)
        {
            database.GetCollection<DefinitionRecord>(DefinitionCollection);

            var archiveCollection = database.GetCollection<ArchivedDefinitionRecord>(DefinitionArchiveCollection);
            archiveCollection.EnsureIndex(r => r.OriginalId);

            return default;
        }


        public async ValueTask<IReadOnlyList<CsStoredDefinition>> GetAllDetails()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<DefinitionRecord>(DefinitionCollection);

            return collection.FindAll()
                .Select(MapDefinition)
                .ToList();
        }


        public async ValueTask<IReadOnlyList<ListDefinition>> List()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<DefinitionListRecord>(DefinitionCollection);

            return collection.FindAll()
                .Select(r => new ListDefinition(r.Id.ToString(), r.Name, r.Version))
                .ToList();
        }


        public async ValueTask<CsStoredDefinition> GetDetails(string id)
        {
            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<DefinitionRecord>(DefinitionCollection);

            var record = collection.FindById(recordId);
            if (record == null)
                throw new InvalidOperationException($"Unknown definition Id: {id}");

            return MapDefinition(record);
        }


        public async ValueTask<string> Insert(CsDefinition newDefinition, string author)
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<DefinitionRecord>(DefinitionCollection);

            var id = ObjectId.NewObjectId();
            var newRecord = MapDefinition(id, newDefinition, 1, author);

            collection.Insert(newRecord);
            return id.ToString();
        }


        public async ValueTask Update(string id, CsDefinition newDefinition, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<DefinitionRecord>(DefinitionCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                throw new InvalidOperationException($"Unknown definition Id: {id}");

            var newRecord = MapDefinition(recordId, newDefinition, currentRecord.Version + 1, author);
            if (!newRecord.Changed(currentRecord))
                return;


            var archiveCollection = connection.Database.GetCollection<ArchivedDefinitionRecord>(DefinitionArchiveCollection);
            archiveCollection.Insert(new ArchivedDefinitionRecord(
                ObjectId.NewObjectId(),
                currentRecord.Id,
                currentRecord.Name,
                currentRecord.Version,
                currentRecord.Author,
                null,
                currentRecord.SourceGroupId,
                currentRecord.Checks
            ));

            collection.Update(recordId, newRecord);
        }


        public async ValueTask Remove(string id, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<DefinitionRecord>(DefinitionCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                return;

            var removedRecord = new ArchivedDefinitionRecord(
                ObjectId.NewObjectId(), 
                currentRecord.Id,
                currentRecord.Name,
                currentRecord.Version,
                currentRecord.Author,
                author,
                currentRecord.SourceGroupId,
                currentRecord.Checks);

            var archiveCollection = connection.Database.GetCollection<ArchivedDefinitionRecord>(DefinitionArchiveCollection);
            archiveCollection.Insert(removedRecord);

            collection.Delete(recordId);
        }


        private static CsStoredDefinition MapDefinition(DefinitionRecord record)
        {
            return new CsStoredDefinition(
                record.Id.ToString(),
                record.Name,
                record.Version,
                record.Author,
                record.SourceGroupId,
                record.Checks.Select(c => new CsDefinitionCheck(c.Name, c.PluginId, ParseConfiguration(c.Configuration))).ToList()
            );
        }


        private static DefinitionRecord MapDefinition(ObjectId id, CsDefinition definition, int version, string author)
        {
            return new DefinitionRecord(
                id,
                definition.Name,
                version,
                author,
                definition.SourceGroupId,
                definition.Checks
                    .Select(c => new DefinitionCheckRecord(
                        c.Name,
                        c.PluginId, 
                        c.Configuration.ToJsonString()
                    ))
                    .ToArray()
            );
        }


        private static JsonObject ParseConfiguration(string configuration)
        {
            return JsonNode.Parse(configuration) as JsonObject ?? new JsonObject();
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class DefinitionRecord
        {
            [BsonId] 
            public ObjectId Id { get; }

            public string Name { get; }
            public int Version { get; }
            public string Author { get; }
            public string SourceGroupId { get; }
            public DefinitionCheckRecord[] Checks { get; }


            [BsonCtor]
            public DefinitionRecord(ObjectId id, string name, int version, string author, string sourceGroupId, BsonArray checks)
            {
                Id = id;
                Name = name;
                Version = version;
                Author = author;
                SourceGroupId = sourceGroupId;
                Checks = checks.ToArray<DefinitionCheckRecord>();
            }


            public DefinitionRecord(ObjectId id, string name, int version, string author, string sourceGroupId, DefinitionCheckRecord[] checks)
            {
                Id = id;
                Name = name;
                Version = version;
                Author = author;
                SourceGroupId = sourceGroupId;
                Checks = checks;
            }


            public bool Changed(DefinitionRecord reference)
            {
                return !string.Equals(reference.Name, Name, StringComparison.InvariantCulture) ||
                       !string.Equals(reference.SourceGroupId, SourceGroupId, StringComparison.InvariantCulture) ||
                       !reference.Checks.SequenceEqual(Checks);
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ArchivedDefinitionRecord : DefinitionRecord
        {
            public ObjectId OriginalId { get; }
            public string? RemovedBy { get; }

            [BsonCtor]
            public ArchivedDefinitionRecord(ObjectId id, ObjectId originalId, string name, int version, string author, string? removedBy, BsonArray sources, BsonArray checks)
                : base(id, name, version, author, sources, checks)
            {
                OriginalId = originalId;
                RemovedBy = removedBy;
            }


            public ArchivedDefinitionRecord(ObjectId id, ObjectId originalId, string name, int version, string author, string? removedBy, string sourceGroupId, DefinitionCheckRecord[] checks)
                : base(id, name, version, author, sourceGroupId, checks)
            {
                OriginalId = originalId;
                RemovedBy = removedBy;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class DefinitionSourceRecord : IEquatable<DefinitionSourceRecord>
        {
            public string Name { get; }
            public Guid PluginId { get; }
            public string Configuration { get; }


            [BsonCtor]
            public DefinitionSourceRecord(string name, Guid pluginId, string configuration)
            {
                Name = name;
                PluginId = pluginId;
                Configuration = configuration;
            }


            public bool Equals(DefinitionSourceRecord? other)
            {
                if (other == null) return false;
                if (ReferenceEquals(this, other)) return true;

                return Name == other.Name && PluginId == other.PluginId && Configuration == other.Configuration;
            }


            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is DefinitionSourceRecord sourceRecord && Equals(sourceRecord);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, PluginId, Configuration);
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class DefinitionCheckRecord : IEquatable<DefinitionCheckRecord>
        {
            public string Name { get; }
            public Guid PluginId { get; }
            public string Configuration { get; }


            [BsonCtor]
            public DefinitionCheckRecord(string name, Guid pluginId, string configuration)
            {
                Name = name;
                PluginId = pluginId;
                Configuration = configuration;
            }


            public bool Equals(DefinitionCheckRecord? other)
            {
                if (other == null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Name == other.Name && PluginId == other.PluginId && Configuration == other.Configuration;
            }


            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is DefinitionCheckRecord checkRecord && Equals(checkRecord);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, PluginId, Configuration);
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class DefinitionListRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string Name { get; }
            public int Version { get; }


            [BsonCtor]
            public DefinitionListRecord(ObjectId id, string name, int version)
            {
                Id = id;
                Name = name;
                Version = version;
            }
        }
    }
}