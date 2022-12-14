using CodeSniffer.Core.Sniffer;
using Serilog;

namespace Sniffer.[Name]
{
    public class [Name]Sniffer : ICsSniffer
    {
        private readonly ILogger logger;
        private readonly [Name]Options options;


        public [Name]Sniffer(ILogger logger, [Name]Options options)
        {
            this.logger = logger;
            this.options = options;
        }


        public async ValueTask<ICsReport?> Execute(string path, ICsScanContext context, CancellationToken cancellationToken)
        {
            //var builder = CsReportBuilder.Create();

            throw new NotImplementedException();


            //return builder.Build();
        }
    }
}
