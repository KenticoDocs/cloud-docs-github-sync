using System;
using GithubService.Models.CodeSamples;
using GithubService.Repository;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TestRepository
{
    class RepositoryConfig : ICodeSampleFileRepositoryConfig
    {
        public string TableName { get; set; }
        public string ConnectionString { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var workingDirectory = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(workingDirectory).Parent?.Parent?.FullName;

            var config = new ConfigurationBuilder()
                .SetBasePath(projectDirectory)
                .AddJsonFile("local.config.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var cfg = new RepositoryConfig
            {
                TableName = config["TableName"],
                ConnectionString = config["ConnectionString"]
            };
            var repository = CodeSampleFileRepository
                .CreateInstance(cfg)
                .GetAwaiter()
                .GetResult();

            var codeSampleA = new CodeSample { Codename = "AAAA", Content = "AAAAAAAAAAAAAA", Language = CodeLanguage.CSharp };
            //var codeSampleB = new CodeSample { Codename = "BBBB", Content = "BBBBBBBBBBBBBBBB", Language = CodeLanguage.Javascript };
            //var codeSampleC = new CodeSample { Codename = "CCCC", Content = "CCCCCCCCCCCCCCCC", Language = CodeLanguage.CUrl };


            var file = new CodeSampleFile { FilePath = "path", CodeSamples = new List<CodeSample>() { codeSampleA } };
            //var fileModified = new CodeSampleFile { FilePath = "path", CodeSamples = new List<CodeSample>() { codeSampleA, codeSampleB } };


            var newFile = repository.AddFileAsync(file).GetAwaiter().GetResult();
            var retrieved = repository.GetFileAsync("path").GetAwaiter().GetResult();

            
            //var x = Execute(repository, file, fileModified).GetAwaiter().GetResult();

            Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            Console.ReadKey();
        }

        static async Task<CodeSampleFile> Execute(CodeSampleFileRepository repository, CodeSampleFile file, CodeSampleFile fileModified)
        {
            var res = await repository.AddFileAsync(file);
            var res2 = await repository.GetFileAsync(file.FilePath);
            var res3 = await repository.UpdateFileAsync(fileModified);

            return await repository.ArchiveFileAsync(fileModified);
        }
    }
}
