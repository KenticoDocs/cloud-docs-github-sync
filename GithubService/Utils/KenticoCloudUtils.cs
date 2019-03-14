using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubService.Models;
using GithubService.Services.Converters;

namespace GithubService.Utils
{
    internal static class KenticoCloudUtils
    {
        private static Lazy<CodeConverter> LazyCodeConverter => new Lazy<CodeConverter>();
        private static readonly CodeConverter CodeConverter = LazyCodeConverter.Value;

        internal static async Task ExecuteCodeFragmentChanges(
            Func<CodeFragment, Task> actionSelectorForSingleFragment,
            Func<CodenameCodeFragments, Task> actionSelectorForCodeSamples,
            IEnumerable<CodeFragment> fragments)
        {
            var fragmentsByCodenameRoot = CodeConverter.ConvertToCodenameCodeFragments(fragments);

            await Task.WhenAll(fragments.Select(actionSelectorForSingleFragment));
            await Task.WhenAll(fragmentsByCodenameRoot.Select(actionSelectorForCodeSamples));
        }

        internal static async Task ExecuteCodeFragmentChanges(
            Func<CodeFragment, Task> actionSelectorForSingleFragment,
            IEnumerable<CodeFragment> fragments) 
            => await Task.WhenAll(fragments.Select(actionSelectorForSingleFragment));
    }
}
