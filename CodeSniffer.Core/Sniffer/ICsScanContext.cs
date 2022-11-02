namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Provides information about the scan job the sniffer is executing on.
    /// </summary>
    public interface ICsScanContext
    {
        /// <summary>
        /// Returns the name of the branch the working copy is on.
        /// </summary>
        public string BranchName { get; }
    }
}
