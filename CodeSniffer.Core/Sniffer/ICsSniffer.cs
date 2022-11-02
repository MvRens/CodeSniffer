namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Implements a code sniffer which can generate a report based on the contents of a folder.
    /// </summary>
    public interface ICsSniffer
    {
        /// <summary>
        /// Run analysis on the code in the provided path and return a report.
        /// </summary>
        /// <param name="path">The path to the source code working copy.</param>
        /// <param name="context">Provides information about the scan job the sniffer is executing on.</param>
        /// <param name="cancellationToken">A CancellationToken which will be cancelled when the service is shut down</param>
        ValueTask<ICsReport?> Execute(string path, ICsScanContext context, CancellationToken cancellationToken);
    }
}
