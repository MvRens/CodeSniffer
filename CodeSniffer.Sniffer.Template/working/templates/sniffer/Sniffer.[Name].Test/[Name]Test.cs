using System.Diagnostics;
using System.Reflection;
using CodeSniffer.Core.Sniffer;
using FluentAssertions;
using Serilog;
using Sniffer.[Name];
using Xunit;
using Xunit.Abstractions;

namespace Sniffer.[Name].Test
{
    public class [Name]Test
    {
        private readonly ILogger logger;
        private readonly string testDataPath;


        public [Name]Test(ITestOutputHelper testOutputHelper)
        {
            logger = new LoggerConfiguration()
                .WriteTo.TestOutput(testOutputHelper)
                .CreateLogger();

            testDataPath = Path.GetFullPath("..\\..\\..\\..\\data", Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(testDataPath))
                throw new Exception($"Test data path not found: {testDataPath}");
        }


        [Fact]
        public async Task Test()
        {
            var sniffer = new [Name]Sniffer(logger, [Name]Options.Default());
            var report = await sniffer.Execute(testDataPath, new TestContext(), CancellationToken.None);
            Debug.Assert(report != null);

            CheckAsset(report, "id1", "name", CsReportResult.Success);
            CheckNoAsset(report, "id2");
        }


        private static void CheckAsset(ICsReport report, string assetId, string expectedName, CsReportResult expectedResult)
        {
            var asset = report.Assets.Should().Contain(a => a.Id == assetId, $"{assetId} should be present").Which;

            using (new AssertionScope())
            {
                asset.Name.Should().Be(expectedName, assetId);
                asset.Result.Should().Be(expectedResult, assetId);
            }
        }


        private static void CheckNoAsset(ICsReport report, string assetId)
        {
            report.Assets.Should().NotContain(a => a.Id == assetId, $"{assetId} should not be present");
        }


        private class TestContext : ICsScanContext
        {
            public string BranchName => "test";
        }
    }
}