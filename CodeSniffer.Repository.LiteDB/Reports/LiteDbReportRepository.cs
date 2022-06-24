using CodeSniffer.Core.Sniffer;
using CodeSniffer.Repository.Reports;
using JetBrains.Annotations;
using LiteDB;

namespace CodeSniffer.Repository.LiteDB.Reports
{
    public class LiteDbReportRepository : BaseLiteDbRepository, IReportRepository
    {
        private const string ReportCollection = "Report";
        private const string ReportSourceCollection = "ReportSource";
        //private const string ReportArchiveCollection = "ReportArchive";


        public LiteDbReportRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        public static ValueTask Initialize(ILiteDatabase database)
        {
            var reportCollection = database.GetCollection<ReportRecord>(ReportCollection);
            reportCollection.EnsureIndex(r => r.DefinitionId);

            var reportSourceCollection = database.GetCollection<ReportSourceRecord>(ReportSourceCollection);
            reportSourceCollection.EnsureIndex(r => r.ReportId);

            //var reportArchiveCollection = database.GetCollection<ReportRecord>(ReportArchiveCollection);
            //reportArchiveCollection.EnsureIndex(r => r.DefinitionId);

            return default;
        }


        public async ValueTask<string> Store(ICsJobReport report)
        {
            using var connection = await GetConnection();
            var reportCollection = connection.Database.GetCollection<ReportRecord>(ReportCollection);
            var reportSourceCollection = connection.Database.GetCollection<ReportSourceRecord>(ReportSourceCollection);

            var reportId = ObjectId.NewObjectId();
            var timestamp = DateTime.UtcNow;
            var reportResult = CsReportResult.Success;


            foreach (var source in report.Sources)
            {
                if (source.Checks.Count == 0)
                    continue;

                var sourceResult = CsReportResult.Success;
                var checks = source.Checks
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
                        if (result > sourceResult)
                            sourceResult = result;

                        if (result > reportResult)
                            sourceResult = result;

                        return new ReportCheckRecord(
                            result,
                            c.Report.Configuration?.ToDictionary(p => p.Key, p => p.Value),
                            assets);
                    })
                    .ToArray();


                var sourceRecord = new ReportSourceRecord(
                    ObjectId.NewObjectId(),
                    reportId,
                    new ObjectId(report.DefinitionId),
                    source.Name,
                    timestamp,
                    sourceResult,
                    checks);

                reportSourceCollection.Insert(sourceRecord);
            }


            var record = new ReportRecord(
                reportId,
                new ObjectId(report.DefinitionId),
                timestamp,
                reportResult
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
            public DateTime Timestamp { get; }
            public CsReportResult Result { get; }


            [BsonCtor]
            public ReportRecord(ObjectId id, ObjectId definitionId, DateTime timestamp, CsReportResult result)
            {
                Id = id;
                DefinitionId = definitionId;
                Timestamp = timestamp;
                Result = result;
            }
        }


        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ReportSourceRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public ObjectId ReportId { get; }
            public ObjectId DefinitionId { get; }
            public string SourceName { get; }
            public DateTime Timestamp { get; }
            public CsReportResult Result { get; }
            public ReportCheckRecord[] Checks{ get; }


            [BsonCtor]
            public ReportSourceRecord(ObjectId id, ObjectId reportId, ObjectId definitionId, string sourceName, DateTime timestamp, CsReportResult result, ReportCheckRecord[] checks)
            {
                Id = id;
                ReportId = reportId;
                DefinitionId = definitionId;
                SourceName = sourceName;
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