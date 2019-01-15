using GithubService.Models.CodeSamples;

namespace GithubService.Repository
{
    internal static class CodeSampleExtensions
    {
        internal static CodeSampleTableEntity ToTableEntity(this CodeSample sample, string partitionKey) =>
            new CodeSampleTableEntity
            {
                Content = sample.Content,
                Language = sample.Language,
                PartitionKey = partitionKey,
                RowKey = sample.Codename
            };

        internal static CodeSample ToCodeSample(this CodeSampleTableEntity entity) =>
            new CodeSample
            {
                Codename = entity.RowKey,
                Content = entity.Content,
                Language = entity.Language,
            };
    }
}