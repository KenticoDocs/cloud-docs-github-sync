using System;

namespace GithubService.Configuration
{
    internal class Configuration : IConfiguration
    {
        public string GithubRepositoryName
            => Environment.GetEnvironmentVariable("Github.RepositoryName");

        public string GithubRepositoryOwner
            => Environment.GetEnvironmentVariable("Github.RepositoryOwner");

        public string GithubAccessToken
            => Environment.GetEnvironmentVariable("Github.AccessToken");

        public string RepositoryConnectionString
            => Environment.GetEnvironmentVariable("Repository.ConnectionString");
    }
}