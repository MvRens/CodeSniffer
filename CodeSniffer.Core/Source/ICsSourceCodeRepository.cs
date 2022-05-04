namespace CodeSniffer.Core.Source
{
    /// <summary>
    /// Provides access to a source code repository, typically in a version control system like Git.
    /// </summary>
    public interface ICsSourceCodeRepository
    {
        /// <summary>
        /// The unique Id of this repository for the purpose of persisting the state.
        /// </summary>
        /// <remarks>
        /// Include only as much information as required to make it unique. The same Id should be
        /// returned for the same configuration.
        /// </remarks>
        string Id { get; }

        /// <summary>
        /// The name of the source code repository for display purposes.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Queries the source code repository for revisions available to analyze.
        /// </summary>
        /// <remarks>
        /// Should return a reasonable list of scannable revisions, for example the last commit
        /// of each active branch. The caller will check if a revision was already analyzed based on it's Id.
        /// </remarks>
        IAsyncEnumerable<ICsSourceCodeRevision> GetRevisions(CancellationToken cancellationToken);


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
