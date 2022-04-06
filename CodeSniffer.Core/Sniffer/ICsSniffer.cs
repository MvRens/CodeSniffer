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
        ICsReport? Execute(string path);
    }
}
