using GithubService.Models.CodeSamples;
using Microsoft.WindowsAzure.Storage.Table;

namespace GithubService.Repository
{
    public class CodeSampleTableEntity : TableEntity
    {
        public string Content { get; set; }
        public CodeLanguage Language { get; set; }
    }
}