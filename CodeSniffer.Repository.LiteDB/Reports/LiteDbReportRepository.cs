using CodeSniffer.Core.Sniffer;
using CodeSniffer.Repository.Reports;
using JetBrains.Annotations;
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

            var reportArchiveCollection = database.GetCollection<ArchivedReportRecord>(ReportArchiveCollection);
            reportArchiveCollection.EnsureIndex(r => r.DefinitionId);

            return default;
        }


        public async ValueTask<string> Store(ICsScanReport report)
        {
            using var connection = await GetConnection();
            var reportCollection = connection.Database.GetCollection<ReportRecord>(ReportCollection);
            var reportArchiveCollection = connection.Database.GetCollection<ArchivedReportRecord>(ReportArchiveCollection);

            var definitionId = new ObjectId(report.DefinitionId);
            var sourceId = new ObjectId(report.SourceId);


            // Archive previous reports for the same combination of definition, source and branch
            foreach (var priorReport in reportCollection
                         .Find(r =>
                             r.DefinitionId == definitionId && 
                             r.SourceId == sourceId && 
                             r.Branch == report.Branch)
                         .ToList())
            {
                var archivedRecord = new ArchivedReportRecord(
                    ObjectId.NewObjectId(),
                    priorReport.Id,
                    priorReport.DefinitionId,
                    priorReport.SourceId,
                    priorReport.RevisionId,
                    priorReport.RevisionName,
                    priorReport.Branch,
                    priorReport.Timestamp,
                    priorReport.Result,
                    priorReport.Checks);

                reportArchiveCollection.Insert(archivedRecord);
                reportCollection.Delete(priorReport.Id);
            }


            // Store new report
            var reportId = ObjectId.NewObjectId();
            var timestamp = DateTime.UtcNow;
            var reportResult = CsReportResult.Success;


            var checks = report.Checks
                .Select(c =>
                {
                    var assets = c.Report.Assets
                        .Select(a => new ReportAssetRecord(
                            a.Id,
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
                        c.PluginId,
                        c.Name,
                        result,
                        c.Report.Configuration?.ToDictionary(p => p.Key, p => p.Value),
                        assets);
                })
                .ToArray();



            var record = new ReportRecord(
                reportId,
                definitionId,
                sourceId,
                report.RevisionId,
                report.RevisionName,
                report.Branch,
                timestamp,
                reportResult,
                checks
            );

            reportCollection.Insert(record);


            return reportId.ToString();
        }


        public async ValueTask<IReadOnlyList<ICsScanReport>> GetActiveReports()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<ReportRecord>(ReportCollection);

            return collection.FindAll()
                .Select(MapReport)
                .ToList();
        }


        public async ValueTask<IReadOnlyList<ICsScanReport>> GetSourceBranchReports(string sourceId, string branch)
        {
            var sourceObjectId = new ObjectId(sourceId);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<ReportRecord>(ReportCollection);

            return collection.Find(r => r.SourceId == sourceObjectId && r.Branch == branch)
                .Select(MapReport)
                .ToList();
        }



        private ICsScanReport MapReport(ReportRecord record)
        {
            return new StoredScanReport(
                record.DefinitionId.ToString(),
                record.SourceId.ToString(),
                record.RevisionId,
                record.RevisionName,
                record.Branch,
                record.Checks
                    .Select(c => new StoredScanReportCheck(
                        c.PluginId,
                        c.Name,
                        new StoredReport(
                            c.Configuration?.ToDictionary(p => p.Key, p => p.Value),
                            c.Assets
                                .Select(a => new StoredAsset(
                                    a.Id,
                                    a.Name,
                                    a.Result,
                                    a.Summary,
                                    a.Properties?.ToDictionary(p => p.Key, p => p.Value),
                                    a.Output))
                                .ToList())
                    ))
                    .ToList());
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
            public string Branch { get; }
            public DateTime Timestamp { get; }
            public CsReportResult Result { get; }
            public ReportCheckRecord[] Checks { get; }


            [BsonCtor]
            public ReportRecord(ObjectId id, ObjectId definitionId, ObjectId sourceId, string revisionId, string revisionName, string branch, DateTime timestamp, CsReportResult result, BsonArray checks)
            {
                Id = id;
                DefinitionId = definitionId;
                SourceId = sourceId;
                RevisionId = revisionId;
                RevisionName = revisionName;
                Branch = branch;
                Timestamp = timestamp;
                Result = result;
                Checks = checks.ToArray<ReportCheckRecord>();
            }


            public ReportRecord(ObjectId id, ObjectId definitionId, ObjectId sourceId, string revisionId, string revisionName, string branch, DateTime timestamp, CsReportResult result, ReportCheckRecord[] checks)
            {
                Id = id;
                DefinitionId = definitionId;
                SourceId = sourceId;
                RevisionId = revisionId;
                RevisionName = revisionName;
                Branch = branch;
                Timestamp = timestamp;
                Result = result;
                Checks = checks;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ReportCheckRecord
        {
            public Guid PluginId { get; }
            public string Name { get; }
            public CsReportResult Result { get; }
            public IDictionary<string, string>? Configuration { get; }
            public ReportAssetRecord[] Assets { get; }


            [BsonCtor]
            public ReportCheckRecord(Guid pluginId, string name, CsReportResult result, BsonDocument? configuration, BsonArray assets)
            {
                PluginId = pluginId;
                Name = name;
                Result = result;
                Configuration = configuration?.ToObject<IDictionary<string, string>>();
                Assets = assets.ToArray<ReportAssetRecord>();
            }


            public ReportCheckRecord(Guid pluginId, string name, CsReportResult result, IDictionary<string, string>? configuration, ReportAssetRecord[] assets)
            {
                PluginId = pluginId;
                Name = name;
                Result = result;
                Configuration = configuration;
                Assets = assets;
            }
        }



        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class ReportAssetRecord
        {
            public string Id { get; }
            public string Name { get; }
            public CsReportResult Result { get; }
            public string? Summary { get; }
            public IDictionary<string, string>? Properties { get; }
            public string? Output { get; }


            [BsonCtor]
            public ReportAssetRecord(string id, string name, CsReportResult result, string? summary, BsonDocument? properties, string? output)
            {
                Id = id;
                Name = name;
                Result = result;
                Summary = summary;
                Properties = properties?.ToObject<IDictionary<string, string>>();
                Output = output;
            }


            public ReportAssetRecord(string id, string name, CsReportResult result, string? summary, IDictionary<string, string>? properties, string? output)
            {
                Id = id;
                Name = name;
                Result = result;
                Summary = summary;
                Properties = properties;
                Output = output;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ArchivedReportRecord : ReportRecord
        {
            public ObjectId OriginalId { get; }


            [BsonCtor]
            public ArchivedReportRecord(ObjectId id, ObjectId originalId, ObjectId definitionId, ObjectId sourceId, string revisionId, string revisionName, string branch, DateTime timestamp, CsReportResult result, BsonArray checks)
                : base(id, definitionId, sourceId, revisionId, revisionName, branch, timestamp, result, checks)
            {
                OriginalId = originalId;
            }


            public ArchivedReportRecord(ObjectId id, ObjectId originalId, ObjectId definitionId, ObjectId sourceId, string revisionId, string revisionName, string branch, DateTime timestamp, CsReportResult result, ReportCheckRecord[] checks)
                : base(id, definitionId, sourceId, revisionId, revisionName, branch, timestamp, result, checks)
            {
                OriginalId = originalId;
            }
        }


        private class StoredScanReport : ICsScanReport
        {
            public string DefinitionId { get; }
            public string SourceId { get; }
            public string RevisionId { get; }
            public string RevisionName { get; }
            public string Branch { get; }
            public IReadOnlyList<ICsScanReportCheck> Checks { get; }


            public StoredScanReport(string definitionId, string sourceId, string revisionId, string revisionName, string branch, IReadOnlyList<ICsScanReportCheck> checks)
            {
                DefinitionId = definitionId;
                SourceId = sourceId;
                RevisionId = revisionId;
                RevisionName = revisionName;
                Branch = branch;
                Checks = checks;
            }
        }


        private class StoredScanReportCheck : ICsScanReportCheck
        {
            public Guid PluginId { get; }
            public string Name { get; }
            public ICsReport Report { get; }


            public StoredScanReportCheck(Guid pluginId, string name, ICsReport report)
            {
                PluginId = pluginId;
                Name = name;
                Report = report;
            }
        }


        private class StoredReport : ICsReport
        {
            public IReadOnlyDictionary<string, string>? Configuration { get; }
            public IEnumerable<ICsReportAsset> Assets { get; }


            public StoredReport(IReadOnlyDictionary<string, string>? configuration, IEnumerable<ICsReportAsset> assets)
            {
                Configuration = configuration;
                Assets = assets;
            }
        }


        private class StoredAsset : ICsReportAsset
        {
            public string Id { get; }
            public string Name { get; }
            public CsReportResult Result { get; }
            public string? Summary { get; }
            public IReadOnlyDictionary<string, string>? Properties { get; }
            public string? Output { get; }


            public StoredAsset(string id, string name, CsReportResult result, string? summary, IReadOnlyDictionary<string, string>? properties, string? output)
            {
                Id = id;
                Name = name;
                Result = result;
                Summary = summary;
                Properties = properties;
                Output = output;
            }
        }
    }
}