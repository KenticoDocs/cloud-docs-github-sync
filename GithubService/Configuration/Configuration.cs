using System;

namespace GithubService.Configuration
{
    internal class Configuration : IConfiguration
    {
        private readonly string _testSuffix = "";

        public Configuration(string testAttribute)
        {
            if (testAttribute == "test")
            {
                _testSuffix = ".Test";
            }
        }

        public string GithubRepositoryName
            => Environment.GetEnvironmentVariable($"Github.RepositoryName{_testSuffix}");

        public string GithubRepositoryOwner
            => Environment.GetEnvironmentVariable($"Github.RepositoryOwner{_testSuffix}");

        public string GithubAccessToken
            => Environment.GetEnvironmentVariable($"Github.AccessToken{_testSuffix}");

        public string RepositoryConnectionString
            => Environment.GetEnvironmentVariable($"Repository.ConnectionString{_testSuffix}");
    }
}