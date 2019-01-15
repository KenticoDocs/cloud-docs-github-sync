using System.Collections.Generic;
using System.Linq;
using GithubService.Models.Webhooks;
using GithubService.Services.Interfaces;

namespace GithubService.Services.Services
{
    public class WebhookParser : IWebhookParser
    {
        public (IEnumerable<string>, IEnumerable<string>, IEnumerable<string>) ParseFiles(WebhookMessage message)
        {
            var addedFiles = new HashSet<string>();
            var modifiedFiles = new HashSet<string>();
            var removedFiles = new HashSet<string>();

            foreach (var commit in message.Commits)
            {
                if (commit.Added.Count > 0)
                {
                    removedFiles.ExceptWith(commit.Added);
                    addedFiles.UnionWith(commit.Added);
                }

                if (commit.Modified.Count > 0)
                {
                    var onlyModifiedFiles = commit.Modified.Except(addedFiles);
                    modifiedFiles.UnionWith(onlyModifiedFiles);
                }

                if (commit.Removed.Count > 0)
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
