namespace GithubService.Configuration
{
    internal interface IConfiguration
    {
        string GithubRepositoryName { get; }

        string GithubRepositoryOwner { get; }

        string GithubAccessToken { get; }

        string KenticoCloudProjectId { get; }

        string KenticoCloudContentManagementApiKey { get; }

        string RepositoryConnectionString { get; }
    }
}
