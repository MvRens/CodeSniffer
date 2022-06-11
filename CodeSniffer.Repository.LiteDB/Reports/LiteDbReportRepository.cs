using CodeSniffer.Core.Sniffer;
using CodeSniffer.Repository.Reports;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Reports
{
    public class LiteDbReportRepository : BaseLiteDbRepository, IReportRepository
    {
        private const string ReportCollection = "Report";
        private const string ReportArchiveCollection = "ReportArchive";


        public LiteDbReportRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        public static ValueTask Initialize(ILiteDatabase database)
        {
            var reportCollection = database.GetCollection<ReportRecord>(ReportCollection);
            reportCollection.EnsureIndex(r => r.DefinitionId);

            var reportArchiveCollection = database.GetCollection<ReportRecord>(ReportArchiveCollection);
            reportArchiveCollection.EnsureIndex(r => r.DefinitionId);

            return default;
        }


        public ValueTask<string> Store(ICsJobReport report)
        {
            // TODO reimplement
            throw new NotImplementedException();

            //CsReportResult result;

            /*
            var assets = report.Assets
                .Select(a => new ReportAssetRecord(
                    a.Name,
                    a.Result,
                    a.Summary,
                    a.Properties?.ToDictionary(p => p.Key, p => p.Value),
                    a.Output
                ))
                .ToList();

            if (assets.Count == 0)
            {
                result = CsReportResult.Error;
                assets.Add(new ReportAssetRecord("No output", CsReportResult.Error,
                    "Error in the check plugin. The report did not contain any assets.", null, null));
            }
            else
                result = assets.Max(a => a.Result);


            var record = new ReportRecord(
                ObjectId.NewObjectId(), 
                new ObjectId(definitionId), 
                DateTime.UtcNow,
                result,
                report.Configuration?.ToDictionary(p => p.Key, p => p.Value),
                assets.ToArray()
            );


            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<ReportRecord>(ReportCollection);
            return collection.Insert(record).ToString();
            */
        }


        public async ValueTask<IReadOnlyDictionary<string, CsReportResult>> GetDefinitionsStatus()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<ReportRecord>(ReportCollection);

            return collection.FindAll()
                .GroupBy(r => r.DefinitionId)
                .Select(group =>
                {
                    CsReportResult? result = null;

                    foreach (var report in group)
                    {
                        if (result == null || report.Result > result)
                            result = report.Result;
                    }


                    return result == null
                        ? null
                        : new
                        {
                            DefinitionId = group.Key.ToString(),
                            Result = result.Value
                        };
                })
                .Where(r => r != null)
                .ToDictionary(r => r!.DefinitionId, r => r!.Result);
        }



        private class ReportRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public ObjectId DefinitionId { get; }
            public DateTime Timestamp { get; }
            public CsReportResult Result { get; }
            public IDictionary<string, string>? Configuration { get; }
            public ReportAssetRecord[] Assets { get; }


            [BsonCtor]
            public ReportRecord(ObjectId id, ObjectId definitionId, DateTime timestamp, CsReportResult result, IDictionary<string, string>? configuration, ReportAssetRecord[] assets)
            {
                Id = id;
                DefinitionId = definitionId;
                Timestamp = timestamp;
                Result = result;
                Configuration = configuration;
                Assets = assets;
            }
        }


        public class ReportAssetRecord
        {
            public string Name { get; }
            public CsReportResult Result { get; }
            public string? Summary { get; }
            public IDictionary<string, string>? Properties { get; }
            public string? Output { get; }


            [BsonCtor]
            public ReportAssetRecord(string name, CsReportResult result, string? summary, IDictionary<string, string>? properties, string? output)
            {
                Name = name;
                Result = result;
                Summary = summary;
                Properties = properties;
                Output = output;
            }
        }
    }
}