namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Describes an asset which was included in the result of a code sniffer.
    /// </summary>
    public interface ICsReportAsset
    {
        /// <summary>
        /// A unique identifier for the asset, used to track the asset across multiple reports. Should be stable between scans.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The name of the asset for display purposes.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Describes the result type of a code sniffer.
        /// </summary>
        CsReportResult Result { get; }

        /// <summary>
        /// A one line summary of the result.
        /// </summary>
        string? Summary { get; }
        
        /// <summary>
        /// Properties found while scanning the code, for example the framework version used by a project. Used for display and filtering.
        /// </summary>
        IReadOnlyDictionary<string, string>? Properties { get; }

        /// <summary>
        /// Optional additional output to be displayed which is not suitable for either Properties or the logger.
        /// </summary>
        string? Output { get; }
    }
}
