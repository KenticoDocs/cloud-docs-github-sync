using System.Collections.Generic;
using System.Linq;
using GithubService.Models.Webhooks;
using GithubService.Services.Interfaces;

namespace GithubService.Services.Parsers
{
    public class WebhookParser : IWebhookParser
    {
        public (ICollection<string> addedFiles, ICollection<string> modifiedFiles, ICollection<string> removedFiles) ExtractFiles(WebhookMessage message)
        {
            var addedFiles = new HashSet<string>();
            var modifiedFiles = new HashSet<string>();
            var removedFiles = new HashSet<string>();

            foreach (var commit in message.Commits)
            {
                if (commit.Added.Any())
                {
                    removedFiles.ExceptWith(commit.Added);
                    addedFiles.UnionWith(commit.Added);
                }

                if (commit.Modified.Any())
                {
                    var onlyModifiedFiles = commit.Modified.Except(addedFiles);
                    modifiedFiles.UnionWith(onlyModifiedFiles);
                }

                if (commit.Removed.Any())
                {
                    var addedAndRemoved = addedFiles.Intersect(commit.Removed).ToList();

                    addedFiles.ExceptWith(addedAndRemoved);
                    modifiedFiles.ExceptWith(commit.Removed);
                    removedFiles.UnionWith(commit.Removed.Except(addedAndRemoved));
                }
            }

            return (addedFiles, modifiedFiles, removedFiles);
        }
    }
}
