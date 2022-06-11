using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Provides a way to build an ICsReport instance.
    /// </summary>
    [PublicAPI]
    public class CsReportBuilder
    {
        private readonly Dictionary<string, string> configuration = new();
        private readonly List<Asset> assets = new();


        private CsReportBuilder()
        {
        }


        /// <summary>
        /// Create a new CsReportBuilder instance.
        /// </summary>
        /// <remarks>
        /// Call <see cref="Build"/> to create an ICsReport.
        /// </remarks>
        public static CsReportBuilder Create()
        {
            return new CsReportBuilder();
        }


        /// <summary>
        /// Adds an asset to the report.
        /// </summary>
        public Asset AddAsset(string id, string name)
        {
            var asset = new Asset(this, id, name);
            assets.Add(asset);

            return asset;
        }


        /// <summary>
        /// Create an ICsReport instance from this builder. The return value is guaranteed to be an immutable snapshot of the builder's state.
        /// </summary>
        public ICsReport Build()
        {
            return new Report(
                new ReadOnlyDictionary<string, string>(configuration),
                assets.Select(a => a.BuildAsset()).ToList()
                );
        }


        /// <inheritdoc cref="ICsReportAsset"/>
        [PublicAPI]
        public class Asset
        {
            private readonly CsReportBuilder builder;
            private Dictionary<string, string>? properties;

            /// <inheritdoc cref="ICsReportAsset.Id"/>
            public string Id { get; set; }

            /// <inheritdoc cref="ICsReportAsset.Name"/>
            public string Name { get; set; }

            /// <inheritdoc cref="ICsReportAsset.Result"/>
            public CsReportResult Result { get; set; } = CsReportResult.Success;

            /// <inheritdoc cref="ICsReportAsset.Summary"/>
            public string? Summary { get; set; }

            /// <inheritdoc cref="ICsReportAsset.Properties"/>
            public IDictionary<string, string> Properties
            {
                get
                {
                    properties ??= new Dictionary<string, string>();
                    return properties;
                }
            }

            /// <inheritdoc cref="ICsReportAsset.Output"/>
            public string? Output { get; set; }


            internal Asset(CsReportBuilder builder, string id, string name)
            {
                this.builder = builder;
                Id = id;
                Name = name;
            }


            /// <inheritdoc cref="ICsReportAsset.Name"/>
            public Asset SetName(string name)
            {
                Name = name;
                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Result"/>
            public Asset SetResult(CsReportResult result)
            {
                Result = result;
                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Result"/>
            public Asset SetResult(CsReportResult result, string summary)
            {
                Result = result;
                Summary = summary;
                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Result"/>
            public Asset SetResultIfHigher(CsReportResult result)
            {
                if (result > Result)
                    Result = result;

                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Result"/>
            public Asset SetResultIfHigher(CsReportResult result, string summary)
            {
                if (result <= Result) 
                    return this;

                Result = result;
                Summary = summary;
                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Summary"/>
            public Asset SetSummary(string summary)
            {
                Summary = summary;
                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Properties"/>
            public Asset SetProperty(string name, string value)
            {
                Properties[name] = value;
                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Output"/>
            public Asset SetOutput(string output)
            {
                Output = output;
                return this;
            }


            /// <inheritdoc cref="ICsReportAsset.Output"/>
            public Asset AddToOutput(string output)
            {
                if (string.IsNullOrWhiteSpace(Output))
                    Output = output;
                else
                    Output += "\n" + output;

                return this;
            }


            /// <inheritdoc cref="CsReportBuilder.AddAsset"/>
            public Asset AddAsset(string id, string name)
            {
                return builder.AddAsset(id, name);
            }

            /// <inheritdoc cref="CsReportBuilder.Build"/>
            public ICsReport Build()
            {
                return builder.Build();
            }


            internal ICsReportAsset BuildAsset()
            {
                return new ReportAsset(
                    Id,
                    Name,
                    Result, 
                    Summary,
                    properties is { Count: > 0 } ? properties : null, 
                    Output
                    );
            }
        }



        private class Report : ICsReport
        {
            public IReadOnlyDictionary<string, string>? Configuration { get; }
            public IEnumerable<ICsReportAsset> Assets { get; }


            public Report(IReadOnlyDictionary<string, string>? configuration, IEnumerable<ICsReportAsset> assets)
            {
                Configuration = configuration;
                Assets = assets;
            }
        }


        private class ReportAsset : ICsReportAsset
        {
            public string Id { get; set; }
            public string Name { get; }
            public CsReportResult Result { get; }
            public string? Summary { get; }
            public IReadOnlyDictionary<string, string>? Properties { get; }
            public string? Output { get; }


            public ReportAsset(string id, string name, CsReportResult result, string? summary, IReadOnlyDictionary<string, string>? properties, string? output)
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
