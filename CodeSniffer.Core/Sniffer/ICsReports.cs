namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Describes the result of a code sniffer job.
    /// </summary>
    public interface ICsJobResult
    {
        /// <summary>
        /// Contains the executed steps and their reports.
        /// </summary>
        IReadOnlyList<CsCheckReport> Checks { get; }
    }


    /// <summary>
    /// Describes a code sniffer step that was run as part of a job.
    /// </summary>
    public class CsCheckReport
    {
        /// <summary>
        /// The name of the step.
        /// </summary>
        public string StepName { get; }


        /// <inheritdoc cref="CsReportResult"/>
        public CsReportResult Result { get; }


        /// <summary>
        /// Describes the result of the code sniffer associated with this step.
        /// </summary>
        public ICsReport Report { get; }


        /// <inheritdoc cref="CsCheckReport"/>
        public CsCheckReport(string stepName, ICsReport report)
        {
            StepName = stepName;
            Report = report;

            Result = CsReportResult.Success;
            foreach (var asset in report.Assets)
            {
                if (asset.Result > Result)
                    Result = asset.Result;
            }
        }
    }
}
