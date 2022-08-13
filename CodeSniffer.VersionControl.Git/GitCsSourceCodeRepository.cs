using CodeSniffer.Core.Source;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Serilog;

namespace CodeSniffer.SourceCodeRepository.Git
{
    public class GitCsSourceCodeRepository : ICsSourceCodeRepository
    {
        private readonly ILogger logger;
        private readonly GitCsSourceCodeRepositoryOptions options;


        public GitCsSourceCodeRepository(ILogger logger, GitCsSourceCodeRepositoryOptions options)
        {
            this.logger = logger;
            this.options = options;
        }


        public IAsyncEnumerable<ICsSourceCodeRevision> GetRevisions(CancellationToken cancellationToken)
        {
            return GetRemoteBranches()
                .Select(r =>
                {
                    var sha = r.ResolveToDirectReference().TargetIdentifier;
                    return new GitCsSourceCodeRevision(sha, GetBranchName(r));
                })
                .ToAsyncEnumerable();
        }


        public IAsyncEnumerable<string> GetActiveBranches(CancellationToken cancellationToken)
        {
            return GetRemoteBranches()
                .Select(GetBranchName)
                .ToAsyncEnumerable();
        }


        private IEnumerable<Reference> GetRemoteBranches()
        {
            logger.Debug("Listing remote references for url {url}", options.Url);

            var references = Repository.ListRemoteReferences(options.Url, GetCredentialsProvider(options));
            return references.Where(IsBranch);
        }


        private const string BranchNamePrefix = @"refs/heads/";


        private static bool IsBranch(Reference reference)
        {
            return reference.IsLocalBranch && reference.CanonicalName.StartsWith(BranchNamePrefix);
        }


        private static string GetBranchName(Reference reference)
        {
            return reference.CanonicalName[BranchNamePrefix.Length..];
        }


        public ValueTask Checkout(ICsSourceCodeRevision revision, string path)
        {
            var gitRevision = (GitCsSourceCodeRevision)revision;

            logger.Debug("Cloning branch {branch} from repository {url}", gitRevision.Branch, gitRevision.Id);
            Repository.Clone(options.Url, path, new CloneOptions
            {
                BranchName = gitRevision.Branch,
                CredentialsProvider = GetCredentialsProvider(options)
            });


            using var workingCopy = new Repository(path);
            if (workingCopy.Head.Tip.Sha == revision.Id) 
                return ValueTask.CompletedTask;

            logger.Debug("Current head has different sha {tipSha} from requested revision {sha}, performing hard reset", workingCopy.Head.Tip.Sha, gitRevision.Sha);

            var commit = workingCopy.Commits.FirstOrDefault(c => c.Sha == gitRevision.Sha);
            if (commit == null)
            {
                logger.Error("Unable to find commit with sha {sha}", gitRevision.Sha);
                return ValueTask.CompletedTask;
            }

            workingCopy.Reset(ResetMode.Hard, commit);
            return ValueTask.CompletedTask;
        }


        private static CredentialsHandler? GetCredentialsProvider(GitCsSourceCodeRepositoryOptions options)
        {
            return !string.IsNullOrEmpty(options.Username) || !string.IsNullOrEmpty(options.Password)
                ? (_, _, _) => new UsernamePasswordCredentials
                {
                    Username = options.Username,
                    Password = options.Password
                }
                : null;
        }


        private class GitCsSourceCodeRevision : ICsSourceCodeRevision
        {
            public string Id => Sha;
            public string Name => $"{Branch} - {Sha}";
            public string Branch { get; }

            public string Sha { get; }


            public GitCsSourceCodeRevision(string sha, string branch)
            {
                Sha = sha;
                Branch = branch;
            }
        }
    }
} 