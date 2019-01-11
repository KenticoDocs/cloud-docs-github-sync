using GithubService.Models.CodeSamples;
using GithubService.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestRepository
{
    class Program
    {
        static void Main(string[] args)
        {
            var repository = new CodeSampleFileRepository();

            var codeSampleA = new CodeSample { Codename = "AAAA", Content = "AAAAAAAAAAAAAA", Language = CodeLanguage.CSharp };
            var codeSampleB = new CodeSample { Codename = "BBBB", Content = "BBBBBBBBBBBBBBBB", Language = CodeLanguage.Javascript };
            var codeSampleC = new CodeSample { Codename = "CCCC", Content = "CCCCCCCCCCCCCCCC", Language = CodeLanguage.CUrl };


            var file = new CodeSampleFile { FilePath = "path", CodeSamples = new List<CodeSample>() { codeSampleA } };
            var fileModified = new CodeSampleFile { FilePath = "path", CodeSamples = new List<CodeSample>() { codeSampleA, codeSampleB } };
            
            var x = Execute(repository, file, fileModified).GetAwaiter().GetResult();

            System.Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
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
