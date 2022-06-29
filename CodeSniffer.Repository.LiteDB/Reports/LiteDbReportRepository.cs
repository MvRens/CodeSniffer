using CodeSniffer.Core.Sniffer;
using CodeSniffer.Repository.Reports;
using JetBrains.Annotations;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Reports
{
    public class LiteDbReportRepository : BaseLiteDbRepository, IReportRepository
    {
        private const string ReportCollection = "Report";
        //private const string ReportArchiveCollection = "ReportArchive";


        public LiteDbReportRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        public static ValueTask Initialize(ILiteDatabase database)
        {
            var reportCollection = database.GetCollection<ReportRecord>(ReportCollection);
            reportCollection.EnsureIndex(r => r.DefinitionId);

            //var reportArchiveCollection = database.GetCollection<ReportRecord>(ReportArchiveCollection);
            //reportArchiveCollection.EnsureIndex(r => r.DefinitionId);

            return default;
        }


        public async ValueTask<string> Store(ICsScanReport report)
        {
            using var connection = await GetConnection();
            var reportCollection = connection.Database.GetCollection<ReportRecord>(ReportCollection);

            var reportId = ObjectId.NewObjectId();
            var timestamp = DateTime.UtcNow;
            var reportResult = CsReportResult.Success;


            var checks = report.Checks
                .Select(c =>
                {
                    var assets = c.Report.Assets
                        .Select(a => new ReportAssetRecord(
                            a.Name,
                            a.Result,
                            a.Summary,
                            a.Properties?.ToDictionary(p => p.Key, p => p.Value),
                            a.Output
                        ))
                        .ToArray();

                    var result = assets.Length > 0 ? assets.Max(a => a.Result) : CsReportResult.Success;
                    if (result > reportResult)
                        reportResult = result;

                    return new ReportCheckRecord(
                        result,
                        c.Report.Configuration?.ToDictionary(p => p.Key, p => p.Value),
                        assets);
                })
                .ToArray();



            var record = new ReportRecord(
                reportId,
                new ObjectId(report.DefinitionId),
                new ObjectId(report.SourceId),
                report.RevisionId,
                report.RevisionName,
                timestamp,
                reportResult,
                checks
            );

            reportCollection.Insert(record);
            return reportId.ToString();
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


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ReportRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public ObjectId DefinitionId { get; }
            public ObjectId SourceId { get; }
            public string RevisionId { get; }
            public string RevisionName { get; }
            public DateTime Timestamp { get; }
            public CsReportResult Result { get; }
            public ReportCheckRecord[] Checks { get; }


            [BsonCtor]
            public ReportRecord(ObjectId id, ObjectId definitionId, ObjectId sourceId, string revisionId, string revisionName, DateTime timestamp, CsReportResult result, ReportCheckRecord[] checks)
            {
                Id = id;
                DefinitionId = definitionId;
                SourceId = sourceId;
                RevisionId = revisionId;
                RevisionName = revisionName;
                Timestamp = timestamp;
                Result = result;
                Checks = checks;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ReportCheckRecord
        {
            public CsReportResult Result { get; }
            public IDictionary<string, string>? Configuration { get; }
            public ReportAssetRecord[] Assets { get; }


            [BsonCtor]
            public ReportCheckRecord(CsReportResult result, IDictionary<string, string>? configuration, ReportAssetRecord[] assets)
            {
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