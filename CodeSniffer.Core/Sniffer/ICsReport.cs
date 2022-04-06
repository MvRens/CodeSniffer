namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Describes the result of a code sniffer.
    /// </summary>
    public interface ICsReport
    {
        /// <summary>
        /// The configuration used for the code sniffer, in a format suitable for display.
        /// </summary>
        IReadOnlyDictionary<string, string>? Configuration { get; }

        /// <summary>
        /// The assets which were included in the result of a code sniffer.
        /// </summary>
        IEnumerable<ICsReportAsset> Assets { get; }
    }
}
