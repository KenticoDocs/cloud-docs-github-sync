namespace GithubService.Configuration
{
    internal interface IConfiguration
    {
        string GithubRepositoryName { get; }
        string GithubRepositoryOwner { get; }
        string GithubAccessToken { get; }

        string KenticoCloudProjectId { get; }
        string KenticoCloudContentManagementApiKy { get; }
        string KenticoCloudInternalApiKey { get; }

        string RepositoryConnectionString { get; }
    }
}
