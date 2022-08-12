namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Describes the result type of a code sniffer.
    /// </summary>
    public enum CsReportResult
    {
        /// <summary>
        /// The scan was skipped.
        /// </summary>
        Skipped,

        /// <summary>
        /// The scan was a success and there are no issues to report.
        /// </summary>
        Success,

        /// <summary>
        /// The scan found one or more warnings.
        /// </summary>
        Warning,

        /// <summary>
        /// The scan found a critical issue.
        /// </summary>
        /// <remarks>
        /// This denotes a succesful scan which found code issues. If a scan was unable to be completed, use <see cref="Error"/> instead.
        /// </remarks>
        Critical,

        /// <summary>
        /// The scan was unable to be completed due to an error.
        /// </summary>
        /// <remarks>
        /// This denotes an issue with the scan itself. If a scan was succesfully completed but found a critical code issue, use <see cref="Critical"/> instead.
        /// </remarks>
        Error
    }
}
