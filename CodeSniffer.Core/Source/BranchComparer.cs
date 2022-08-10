namespace CodeSniffer.Core.Source
{
    /// <summary>
    /// Used for sorting by branch name. Takes into account common branch names to prioritize.
    /// </summary>
    /// <remarks>
    /// Currently only recognizes the Git naming conventions.
    /// </remarks>
    public class BranchComparer : IComparer<string>
    {
        /// <summary>
        /// Static instance of a BranchComparer
        /// </summary>
        public static BranchComparer Default = new();


        private static readonly BranchRank[] KnownBranches = 
        {
            new("master"),
            new("main"),

            new("develop"),

            new("hotfix/", true),
            new("release/", true),
            new("feature/", true)
        };


        // ReSharper disable once ConvertIfStatementToReturnStatement
        /// <inheritdoc />>
        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;


            var xRank = GetBranchRank(x);
            var yRank = GetBranchRank(y);

            if (xRank > yRank) return 1;
            if (xRank < yRank) return -1;

            return StringComparer.InvariantCultureIgnoreCase.Compare(x, y);
        }


        private static int GetBranchRank(string branch)
        {
            for (var i = 0; i < KnownBranches.Length; i++)
            {
                if (KnownBranches[i].IsPrefix)
                {
                    if (branch.StartsWith(KnownBranches[i].BranchName, StringComparison.InvariantCultureIgnoreCase))
                        return i;
                }
                else
                {
                    if (string.Equals(branch, KnownBranches[i].BranchName, StringComparison.InvariantCultureIgnoreCase))
                        return i;
                }
            }

            return int.MaxValue;
        }


        private struct BranchRank
        {
            public string BranchName { get; }
            public bool IsPrefix { get; }


            public BranchRank(string branchName, bool isPrefix = false)
            {
                BranchName = branchName;
                IsPrefix = isPrefix;
            }
        }
    }
}
