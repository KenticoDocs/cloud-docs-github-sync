using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Models;
using Microsoft.Extensions.Logging;

namespace GithubService.Services.Interfaces
{
    public interface IFileProcessor
    {
        Task<IEnumerable<CodeFragment>> ProcessAddedFiles(ICollection<string> addedFiles);

        Task<(IEnumerable<CodeFragment>, IEnumerable<CodeFragment>, IEnumerable<CodeFragment>)> ProcessModifiedFiles(
            ICollection<string> modifiedFiles,
            ILogger logger
        );

        Task<IEnumerable<CodeFragment>> ProcessRemovedFiles(ICollection<string> removedFiles);
    }
}
