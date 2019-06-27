using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GithubService.Models;
using GithubService.Repository;
using GithubService.Services.Converters;
using GithubService.Services.Interfaces;

namespace GithubService.Services
{
    public class FileProcessor : IFileProcessor
    {
        private readonly ICodeFileRepository _codeFileRepository;
        private readonly IGithubService _githubService;

        public FileProcessor(IGithubService githubService, ICodeFileRepository codeFileRepository)
        {
            _codeFileRepository = codeFileRepository;
            _githubService = githubService;
        }

        public async Task<IEnumerable<CodeFragment>> ProcessAddedFiles(ICollection<string> addedFiles)
        {
            if (!addedFiles.Any())
                return Enumerable.Empty<CodeFragment>();

            var codeFiles = new List<CodeFile>();

            foreach (var filePath in addedFiles)
            {
                var codeFile = await _githubService.GetCodeFileAsync(filePath);

                await _codeFileRepository.StoreAsync(codeFile);
                codeFiles.Add(codeFile);
            }

            return codeFiles.SelectMany(file => file.CodeFragments);
        }

        public async Task<(IEnumerable<CodeFragment>, IEnumerable<CodeFragment>, IEnumerable<CodeFragment>)>
            ProcessModifiedFiles(ICollection<string> modifiedFiles, ILogger logger)
        {
            if (!modifiedFiles.Any())
                return (
                    Enumerable.Empty<CodeFragment>(),
                    Enumerable.Empty<CodeFragment>(),
                    Enumerable.Empty<CodeFragment>()
                );

            var fragmentsToAdd = new List<CodeFragment>();
            var fragmentsToModify = new List<CodeFragment>();
            var fragmentsToRemove = new List<CodeFragment>();

            var codeConverter = new CodeConverter();

            foreach (var filePath in modifiedFiles)
            {
                var oldCodeFile = await _codeFileRepository.GetAsync(filePath);

                var newCodeFile = await _githubService.GetCodeFileAsync(filePath);
                await _codeFileRepository.StoreAsync(newCodeFile);

                if (oldCodeFile == null)
                {
                    logger.LogWarning(
                        $"Trying to modify code file {filePath} might result in inconsistent content " +
                        "in KC because there is no known previous version of the code file.");

                    fragmentsToAdd.AddRange(newCodeFile.CodeFragments);
                }
                else
                {
                    var (newFragments, modifiedFragments, removedFragments) =
                        codeConverter.CompareFragmentLists(oldCodeFile.CodeFragments, newCodeFile.CodeFragments);

                    fragmentsToAdd.AddRange(newFragments);
                    fragmentsToModify.AddRange(modifiedFragments);
                    fragmentsToRemove.AddRange(removedFragments);
                }
            }

            return (fragmentsToAdd, fragmentsToModify, fragmentsToRemove);
        }

        public async Task<IEnumerable<CodeFragment>> ProcessRemovedFiles(ICollection<string> removedFiles)
        {
            if (!removedFiles.Any())
                return Enumerable.Empty<CodeFragment>();

            var codeFiles = new List<CodeFile>();

            foreach (var removedFile in removedFiles)
            {
                var archivedFile = await _codeFileRepository.ArchiveAsync(removedFile);

                if (archivedFile != null)
                {
                    codeFiles.Add(archivedFile);
                }
            }

            return codeFiles.SelectMany(file => file.CodeFragments);
        }
    }
}
