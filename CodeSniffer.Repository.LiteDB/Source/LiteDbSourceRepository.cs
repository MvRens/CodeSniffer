using System.Text.Json.Nodes;
using CodeSniffer.Repository.Source;
using JetBrains.Annotations;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Source
{
    public class LiteDbSourceRepository : BaseLiteDbRepository, ISourceRepository
    {
        private const string SourceCollection = "Source";
        private const string SourceArchiveCollection = "SourceArchive";

        private const string SourceGroupCollection = "SourceGroup";
        private const string SourceGroupArchiveCollection = "SourceGroupArchive";


        public LiteDbSourceRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        public static ValueTask Initialize(ILiteDatabase database)
        {
            database.GetCollection<SourceRecord>(SourceCollection);

            var sourceArchiveCollection = database.GetCollection<ArchivedSourceRecord>(SourceArchiveCollection);
            sourceArchiveCollection.EnsureIndex(r => r.OriginalId);

            database.GetCollection<SourceGroupRecord>(SourceGroupCollection);

            var sourceGroupArchiveCollection = database.GetCollection<ArchivedSourceGroupRecord>(SourceGroupArchiveCollection);
            sourceGroupArchiveCollection.EnsureIndex(r => r.OriginalId);

            return default;
        }


        public async ValueTask<IReadOnlyList<ListSource>> ListSources()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceListRecord>(SourceCollection);

            return collection.FindAll()
                .Select(r => new ListSource(r.Id.ToString(), r.Name))
                .ToList();
        }


        public async ValueTask<IReadOnlyList<ListSourceGroup>> ListSourceGroups()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceGroupListRecord>(SourceGroupCollection);

            return collection.FindAll()
                .Select(r => new ListSourceGroup(r.Id.ToString(), r.Name))
                .ToList();
        }


        public async ValueTask<IReadOnlyList<CsStoredSource>> GetAllSources()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceRecord>(SourceCollection);

            return collection.FindAll()
                .Select(MapSource)
                .ToList();
        }


        public async ValueTask<IReadOnlyList<CsStoredSourceGroup>> GetAllSourceGroups()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceGroupRecord>(SourceGroupCollection);

            return collection.FindAll()
                .Select(MapSourceGroup)
                .ToList();
        }


        public async ValueTask<CsStoredSource> GetSourceDetails(string id)
        {
            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceRecord>(SourceCollection);

            var record = collection.FindById(recordId);
            if (record == null)
                throw new InvalidOperationException($"Unknown source Id: {id}");

            return MapSource(record);
        }


        public async ValueTask<CsStoredSourceGroup> GetSourceGroupDetails(string id)
        {
            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceGroupRecord>(SourceGroupCollection);

            var record = collection.FindById(recordId);
            if (record == null)
                throw new InvalidOperationException($"Unknown source group Id: {id}");

            return MapSourceGroup(record);
        }


        public async ValueTask<string> InsertSource(CsSource newSource, string author)
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceRecord>(SourceCollection);

            var id = ObjectId.NewObjectId();
            var newRecord = MapSource(id, newSource, 1, author);

            collection.Insert(newRecord);
            return id.ToString();
        }


        public async ValueTask UpdateSource(string id, CsSource newSource, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceRecord>(SourceCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                throw new InvalidOperationException($"Unknown definition Id: {id}");

            var newRecord = MapSource(recordId, newSource, currentRecord.Version + 1, author);
            if (!newRecord.Changed(currentRecord))
                return;


            var archiveCollection = connection.Database.GetCollection<ArchivedSourceRecord>(SourceArchiveCollection);
            archiveCollection.Insert(new ArchivedSourceRecord(
                ObjectId.NewObjectId(),
                currentRecord.Id,
                currentRecord.Name,
                currentRecord.Version,
                currentRecord.Author,
                null,
                currentRecord.PluginId,
                currentRecord.Configuration
            ));

            collection.Update(recordId, newRecord);
        }


        public async ValueTask RemoveSource(string id, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceRecord>(SourceCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                return;

            var removedRecord = new ArchivedSourceRecord(
                ObjectId.NewObjectId(),
                currentRecord.Id,
                currentRecord.Name,
                currentRecord.Version,
                currentRecord.Author,
                author,
                currentRecord.PluginId,
                currentRecord.Configuration);

            var archiveCollection = connection.Database.GetCollection<ArchivedSourceRecord>(SourceArchiveCollection);
            archiveCollection.Insert(removedRecord);

            collection.Delete(recordId);
        }


        public async ValueTask<string> InsertSourceGroup(CsSourceGroup newSourceGroup, string author)
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceGroupRecord>(SourceGroupCollection);

            var id = ObjectId.NewObjectId();
            var newRecord = MapSourceGroup(id, newSourceGroup, 1, author);

            collection.Insert(newRecord);
            return id.ToString();
        }


        public async ValueTask UpdateSourceGroup(string id, CsSourceGroup newSourceGroup, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceGroupRecord>(SourceGroupCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                throw new InvalidOperationException($"Unknown definition Id: {id}");

            var newRecord = MapSourceGroup(recordId, newSourceGroup, currentRecord.Version + 1, author);
            if (!newRecord.Changed(currentRecord))
                return;


            var archiveCollection = connection.Database.GetCollection<ArchivedSourceGroupRecord>(SourceArchiveCollection);
            archiveCollection.Insert(new ArchivedSourceGroupRecord(
                ObjectId.NewObjectId(),
                currentRecord.Id,
                currentRecord.Name,
                currentRecord.Version,
                currentRecord.Author,
                null,
                currentRecord.SourceIds
            ));

            collection.Update(recordId, newRecord);
        }


        public async ValueTask RemoveSourceGroup(string id, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<SourceGroupRecord>(SourceGroupCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                return;

            var removedRecord = new ArchivedSourceGroupRecord(
                ObjectId.NewObjectId(),
                currentRecord.Id,
                currentRecord.Name,
                currentRecord.Version,
                currentRecord.Author,
                author,
                currentRecord.SourceIds);

            var archiveCollection = connection.Database.GetCollection<ArchivedSourceGroupRecord>(SourceGroupArchiveCollection);
            archiveCollection.Insert(removedRecord);

            collection.Delete(recordId);
        }


        private static CsStoredSource MapSource(SourceRecord record)
        {
            return new CsStoredSource(
                record.Id.ToString(),
                record.Name,
                record.Version,
                record.Author,
                record.PluginId,
                ParseConfiguration(record.Configuration)
            );
        }


        private static SourceRecord MapSource(ObjectId id, CsSource source, int version, string author)
        {
            return new SourceRecord(
                id,
                source.Name,
                version,
                author,
                source.PluginId,
                source.Configuration.ToJsonString()
            );
        }


        private static CsStoredSourceGroup MapSourceGroup(SourceGroupRecord record)
        {
            return new CsStoredSourceGroup(
                record.Id.ToString(),
                record.Name,
                record.Version,
                record.Author,
                record.SourceIds.Select(i => i.ToString()).ToList()
            );
        }


        private static SourceGroupRecord MapSourceGroup(ObjectId id, CsSourceGroup sourceGroup, int version, string author)
        {
            return new SourceGroupRecord(
                id,
                sourceGroup.Name,
                version,
                author,
                sourceGroup.SourceIds.Select(i => new ObjectId(i)).ToArray()
            );
        }


        private static JsonObject ParseConfiguration(string configuration)
        {
            return JsonNode.Parse(configuration) as JsonObject ?? new JsonObject();
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class SourceListRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string Name { get; }


            [BsonCtor]
            public SourceListRecord(ObjectId id, string name)
            {
                Id = id;
                Name = name;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class SourceGroupListRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string Name { get; }


            [BsonCtor]
            public SourceGroupListRecord(ObjectId id, string name)
            {
                Id = id;
                Name = name;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class SourceRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string Name { get; }
            public int Version { get; }
            public string Author { get; }
            public Guid PluginId { get; }
            public string Configuration { get; }



            [BsonCtor]
            public SourceRecord(ObjectId id, string name, int version, string author, Guid pluginId, string configuration)
            {
                Id = id;
                Name = name;
                Version = version;
                Author = author;
                PluginId = pluginId;
                Configuration = configuration;
            }


            public bool Changed(SourceRecord reference)
            {
                return reference.PluginId != PluginId ||
                       !string.Equals(reference.Name, Name, StringComparison.InvariantCulture) ||
                       !string.Equals(reference.Configuration, Configuration, StringComparison.InvariantCulture);
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ArchivedSourceRecord : SourceRecord
        {
            public ObjectId OriginalId { get; }
            public string? RemovedBy { get; }


            [BsonCtor]
            public ArchivedSourceRecord(ObjectId id, ObjectId originalId, string name, int version, string author, string? removedBy, Guid pluginId, string configuration) 
                : base(id, name, version, author, pluginId, configuration)
            {
                OriginalId = originalId;
                RemovedBy = removedBy;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class SourceGroupRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string Name { get; }
            public int Version { get; }
            public string Author { get; }
            public ObjectId[] SourceIds { get; }


            [BsonCtor]
            public SourceGroupRecord(ObjectId id, string name, int version, string author, BsonArray sourceIds)
            {
                Id = id;
                Name = name;
                Version = version;
                Author = author;
                SourceIds = sourceIds.ToArray<ObjectId>();
            }


            public SourceGroupRecord(ObjectId id, string name, int version, string author, ObjectId[] sourceIds)
            {
                Id = id;
                Name = name;
                Version = version;
                Author = author;
                SourceIds = sourceIds;
            }


            public bool Changed(SourceGroupRecord reference)
            {
                return !string.Equals(reference.Name, Name, StringComparison.InvariantCulture) ||
                       !reference.SourceIds.SequenceEqual(SourceIds);
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ArchivedSourceGroupRecord : SourceGroupRecord
        {
            public ObjectId OriginalId { get; }
            public string? RemovedBy { get; }


            [BsonCtor]
            public ArchivedSourceGroupRecord(ObjectId id, ObjectId originalId, string name, int version, string author, string? removedBy, BsonArray sourceIds)
                : base(id, name, version, author, sourceIds)
            {
                OriginalId = originalId;
                RemovedBy = removedBy;
            }


            public ArchivedSourceGroupRecord(ObjectId id, ObjectId originalId, string name, int version, string author, string? removedBy, ObjectId[] sourceIds) 
                : base(id, name, version, author, sourceIds)
            {
                OriginalId = originalId;
                RemovedBy = removedBy;
            }
        }
    }
}