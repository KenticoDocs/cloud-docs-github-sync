namespace GithubService.Repository
{
    public interface ICodeSampleFileRepositoryConfig
    {
        string TableName { get; }
        string ConnectionString { get; }
    }
}