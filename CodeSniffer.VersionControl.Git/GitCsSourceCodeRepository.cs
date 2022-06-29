using System.Runtime.CompilerServices;
using CodeSniffer.Core.Source;
using LibGit2Sharp;
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


        private const string BranchNamePrefix = @"refs/heads/";


        public async IAsyncEnumerable<ICsSourceCodeRevision> GetRevisions([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            logger.Debug("Listing remote reference for url {url}", options.Url);

            var references = Repository.ListRemoteReferences(options.Url);
            foreach (var reference in references.Where(r => r.IsLocalBranch && r.CanonicalName.StartsWith(BranchNamePrefix)))
            {
                var branchName = reference.CanonicalName[BranchNamePrefix.Length..];
                var sha = reference.ResolveToDirectReference().TargetIdentifier;

                yield return new GitCsSourceCodeRevision(sha, branchName);
            }

            await Task.CompletedTask;
        }


        public ValueTask Checkout(ICsSourceCodeRevision revision, string path)
        {
            var gitRevision = (GitCsSourceCodeRevision)revision;

            logger.Debug("Cloning branch {branch} from repository {url}", gitRevision.BranchName, gitRevision.Id);
            Repository.Clone(options.Url, path, new CloneOptions
            {
                BranchName = gitRevision.BranchName,
                CredentialsProvider = !string.IsNullOrEmpty(options.Username) || !string.IsNullOrEmpty(options.Password)
                    ? (_, _, _) => new UsernamePasswordCredentials
                    {
                        Username = options.Username,
                        Password = options.Password
                    }
                    : null
            });


            var workingCopy = new Repository(path);
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



        private class GitCsSourceCodeRevision : ICsSourceCodeRevision
        {
            public string Id => Sha;
            public string Name => $"{BranchName} - {Sha}";

            public string Sha { get; }
            public string BranchName { get; }


            public GitCsSourceCodeRevision(string sha, string branchName)
            {
                Sha = sha;
                BranchName = branchName;
            }
        }
    }
} 