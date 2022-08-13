namespace CodeSniffer.Core.Source
{
    /// <summary>
    /// Provides access to a source code repository, typically in a version control system like Git.
    /// </summary>
    public interface ICsSourceCodeRepository
    {
        /// <summary>
        /// Queries the source code repository for revisions available to analyze.
        /// </summary>
        /// <remarks>
        /// Should return a reasonable list of scannable revisions, for example the last commit
        /// of each active branch. The caller will check if a revision was already analyzed based on it's Id.
        /// </remarks>
        IAsyncEnumerable<ICsSourceCodeRevision> GetRevisions(CancellationToken cancellationToken);


        /// <summary>
        /// Queries the source code repository for active branches, for cleanup purposes.
        /// </summary>
        IAsyncEnumerable<string> GetActiveBranches(CancellationToken cancellationToken);


        /// <summary>
        /// Checks out a working copy of the source code for the specified revision.
        /// </summary>
        /// <remarks>
        /// The revision instance passed is guaranteed to be one returned from GetRevisions, which means
        /// you can use it to pass along additional metadata by casting the ICsSourceCodeRevision back.
        /// </remarks>
        /// <param name="revision">A revision as returned from GetRevisions.</param>
        /// <param name="path">The path where the source code for the revision should be written to.</param>
        ValueTask Checkout(ICsSourceCodeRevision revision, string path);
    }
}
