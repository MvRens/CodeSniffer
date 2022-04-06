using System.Text.Json;
using System.Text.Json.Nodes;
using CodeSniffer.Core.Plugin;
using CodeSniffer.Core.Source;
using Serilog;

namespace CodeSniffer.SourceCodeRepository.Git
{
    [CsPlugin("git")]
    public class GitCsSourceCodeRepositoryPlugin : ICsSourceCodeRepositoryPlugin
    {
        public string Name => "Git";
        public JsonObject? DefaultOptions => JsonSerializer.SerializeToNode(GitCsSourceCodeRepositoryOptions.Default()) as JsonObject;


        public ICsSourceCodeRepository Create(ILogger logger, JsonObject options)
        {
            var gitOptions = options.Deserialize<GitCsSourceCodeRepositoryOptions>();

            CsOptionMissingException.ThrowIfNull(gitOptions);
            CsOptionMissingException.ThrowIfEmpty(gitOptions.Url);

            return new GitCsSourceCodeRepository(logger, gitOptions);
        }
    }
}