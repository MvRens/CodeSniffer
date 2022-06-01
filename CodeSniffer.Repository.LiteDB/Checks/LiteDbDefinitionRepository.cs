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

            var newRecord = MapDefinition(ObjectId.NewObjectId(), newDefinition, 1, author, null);
            return collection.Insert(newRecord).ToString();
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

            var newRecord = MapDefinition(recordId, newDefinition, currentRecord.Version + 1, author, null);
            if (!newRecord.Changed(currentRecord))
                return;


            var archiveCollection = connection.Database.GetCollection<ArchivedDefinitionRecord>(DefinitionArchiveCollection);
            archiveCollection.Insert(new ArchivedDefinitionRecord(
                ObjectId.NewObjectId(),
                currentRecord.Id,
                currentRecord.Name,
                currentRecord.Version,
                currentRecord.Author,
                currentRecord.RemovedBy,
                currentRecord.Sources,
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
                currentRecord.Sources,
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
                record.Sources.Select(s => new CsDefinitionSource(s.Name, s.PluginName, ParseConfiguration(s.Configuration))).ToList(),
                record.Checks.Select(c => new CsDefinitionCheck(c.Name, c.PluginName, ParseConfiguration(c.Configuration))).ToList()
            );
        }


        private static DefinitionRecord MapDefinition(ObjectId id, CsDefinition definition, int version, string author, string? removedBy)
        {
            return new DefinitionRecord(
                id,
                definition.Name,
                version,
                author,
                removedBy,
                definition.Sources
                    .Select(s => new DefinitionSourceRecord(
                        s.Name, 
                        s.PluginName, 
                        s.Configuration.ToJsonString()
                    ))
                    .ToArray(),
                definition.Checks
                    .Select(c => new DefinitionCheckRecord(
                        c.Name,
                        c.PluginName, 
                        c.Configuration.ToJsonString()
                    ))
                    .ToArray()
            );
        }


        private static JsonObject ParseConfiguration(string configuration)
        {
            return JsonNode.Parse(configuration) as JsonObject ?? new JsonObject();
        }


        [UsedImplicitly]
        private class DefinitionRecord
        {
            [BsonId] 
            public ObjectId Id { get; }

            public string Name { get; }
            public int Version { get; }
            public string Author { get; }
            public string? RemovedBy { get; }
            public DefinitionSourceRecord[] Sources { get; }
            public DefinitionCheckRecord[] Checks { get; }


            [BsonCtor]
            public DefinitionRecord(ObjectId id, string name, int version, string author, string? removedBy, BsonArray sources, BsonArray checks)
            {
                Id = id;
                Name = name;
                Version = version;
                Author = author;
                RemovedBy = removedBy;
                Sources = sources.ToArray<DefinitionSourceRecord>();
                Checks = checks.ToArray<DefinitionCheckRecord>();
            }


            public DefinitionRecord(ObjectId id, string name, int version, string author, string? removedBy, DefinitionSourceRecord[] sources, DefinitionCheckRecord[] checks)
            {
                Id = id;
                Name = name;
                Version = version;
                Author = author;
                RemovedBy = removedBy;
                Sources = sources;
                Checks = checks;
            }


            public bool Changed(DefinitionRecord reference)
            {
                return !string.Equals(reference.Name, Name, StringComparison.InvariantCulture) ||
                       !reference.Sources.SequenceEqual(Sources) ||
                       !reference.Checks.SequenceEqual(Checks);
            }
        }


        [UsedImplicitly]
        private class ArchivedDefinitionRecord : DefinitionRecord
        {
            public ObjectId OriginalId { get; }

            [BsonCtor]
            public ArchivedDefinitionRecord(ObjectId id, ObjectId originalId, string name, int version, string author, string? removedBy, BsonArray sources, BsonArray checks)
                : base(id, name, version, author, removedBy, sources, checks)
            {
                OriginalId = originalId;
            }


            public ArchivedDefinitionRecord(ObjectId id, ObjectId originalId, string name, int version, string author, string? removedBy, DefinitionSourceRecord[] sources, DefinitionCheckRecord[] checks)
                : base(id, name, version, author, removedBy, sources, checks)
            {
                OriginalId = originalId;
            }
        }


        private class DefinitionSourceRecord : IEquatable<DefinitionSourceRecord>
        {
            public string Name { get; }
            public string PluginName { get; }
            public string Configuration { get; }


            [BsonCtor]
            public DefinitionSourceRecord(string name, string pluginName, string configuration)
            {
                Name = name;
                PluginName = pluginName;
                Configuration = configuration;
            }


            public bool Equals(DefinitionSourceRecord? other)
            {
                if (other == null) return false;
                if (ReferenceEquals(this, other)) return true;

                return Name == other.Name && PluginName == other.PluginName && Configuration == other.Configuration;
            }


            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is DefinitionSourceRecord sourceRecord && Equals(sourceRecord);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, PluginName, Configuration);
            }
        }


        private class DefinitionCheckRecord : IEquatable<DefinitionCheckRecord>
        {
            public string Name { get; }
            public string PluginName { get; }
            public string Configuration { get; }


            [BsonCtor]
            public DefinitionCheckRecord(string name, string pluginName, string configuration)
            {
                Name = name;
                PluginName = pluginName;
                Configuration = configuration;
            }


            public bool Equals(DefinitionCheckRecord? other)
            {
                if (other == null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Name == other.Name && PluginName == other.PluginName && Configuration == other.Configuration;
            }


            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is DefinitionCheckRecord checkRecord && Equals(checkRecord);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, PluginName, Configuration);
            }
        }


        [UsedImplicitly]
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