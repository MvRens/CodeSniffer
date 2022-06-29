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
        IReadOnlyList<CsJobCheck> Checks { get; }
    }


    /// <summary>
    /// Describes a code sniffer check that was run as part of a job.
    /// </summary>
    public class CsJobCheck
    {
        /// <summary>
        /// The Id of the plugin that performed the check.
        /// </summary>
        public Guid PluginId { get; }

        /// <summary>
        /// The name of the step.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Describes the result of the code sniffer associated with this step.
        /// </summary>
        public ICsReport Report { get; }


        /// <inheritdoc cref="CsJobCheck"/>
        public CsJobCheck(Guid pluginId, string name, ICsReport report)
        {
            PluginId = pluginId;
            Name = name;
            Report = report;
        }
    }
}
