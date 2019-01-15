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
            // get config variables
            var workingDirectory = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(workingDirectory).Parent?.Parent?.FullName;
            var config = new ConfigurationBuilder()
                .SetBasePath(projectDirectory)
                .AddJsonFile("local.config.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();


            // initialize repo
            var cfg = new RepositoryConfig
            {
                TableName = config["TableName"],
                ConnectionString = config["ConnectionString"]
            };
            var repository = CodeSampleFileRepository
                .CreateInstance(cfg)
                .GetAwaiter()
                .GetResult();


            // initialize test samples
            var codeSampleA = new CodeSample { Codename = "AAAA", Content = "AAAAAAAAAAAAAA", Language = CodeLanguage.CSharp };
            //var codeSampleB = new CodeSample { Codename = "BBBB", Content = "BBBBBBBBBBBBBBBB", Language = CodeLanguage.Javascript };
            //var codeSampleC = new CodeSample { Codename = "CCCC", Content = "CCCCCCCCCCCCCCCC", Language = CodeLanguage.CUrl };

            var file = new CodeSampleFile { FilePath = "path/asdf/fdsa", CodeSamples = new List<CodeSample>() { codeSampleA } };
            //var fileModified = new CodeSampleFile { FilePath = "path", CodeSamples = new List<CodeSample>() { codeSampleA, codeSampleB } };


            // test stuff
            //var newFile = repository.AddFileAsync(file).GetAwaiter().GetResult();
            var retrieved = repository.GetFileAsync("path/asdf").GetAwaiter().GetResult();


            Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            Console.ReadKey();
        }
    }
}
