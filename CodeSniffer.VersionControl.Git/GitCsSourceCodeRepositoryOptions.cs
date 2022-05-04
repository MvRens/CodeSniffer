using System.Text.Json.Serialization;

namespace CodeSniffer.SourceCodeRepository.Git
{
    [JsonSerializable(typeof(GitCsSourceCodeRepositoryOptions))]
    public class GitCsSourceCodeRepositoryOptions
    {
        public string? Url { get; set; }

        // TODO credentials


        public static GitCsSourceCodeRepositoryOptions Default()
        {
            return new GitCsSourceCodeRepositoryOptions
            {
                Url = "https://example.com/repository.git"
            };
        }
    }
}
