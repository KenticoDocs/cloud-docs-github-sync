namespace GithubService.Repository
{
    internal static class StringExtensions
    {
        internal static string ToPartitionKey(this string filePath) =>
            filePath.Replace('/', '-');
    }
}