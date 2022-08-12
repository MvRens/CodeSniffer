using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.API.Report
{
    public enum ReportResult
    {
        /// <inheritdoc cref="CsReportResult.Skipped"/>
        Skipped = 0,

        /// <inheritdoc cref="CsReportResult.Success"/>
        Success,

        /// <inheritdoc cref="CsReportResult.Warning"/>
        Warning,

        /// <inheritdoc cref="CsReportResult.Critical"/>
        Critical,

        /// <inheritdoc cref="CsReportResult.Error"/>
        Error
    }
}
