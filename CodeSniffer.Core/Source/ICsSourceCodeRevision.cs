namespace CodeSniffer.Core.Source
{
    /// <summary>
    /// Represent a revision of the source code.
    /// </summary>
    public interface ICsSourceCodeRevision
    {
        /// <summary>
        /// The unique Id of this revision for the purpose of detecting whether a revision has already been analyzed.
        /// </summary>
        /// <remarks>
        /// Include only as much information as required to make it unique. Should not be used to store additional metadata.
        /// </remarks>
        string Id { get; }


        /// <summary>
        /// The name of the revision for display purposes.
        /// </summary>
        string Name { get; }
    }
}
