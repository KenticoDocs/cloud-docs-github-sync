using System;
using System.Collections.Generic;
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

            foreach (var codeFragment in fragments)
            {
                await actionSelectorForSingleFragment(codeFragment);
            }

            foreach (var fragmentByCodename in fragmentsByCodenameRoot)
            {
                if (fragmentByCodename.CodeFragments.Count > 1)
                {
                    await actionSelectorForCodeSamples(fragmentByCodename);
                }
            }
        }

        internal static async Task ExecuteCodeFragmentChanges(
            Func<CodeFragment, Task> actionSelectorForSingleFragment,
            IEnumerable<CodeFragment> fragments)
        {
            foreach (var fragment in fragments)
            {
                await actionSelectorForSingleFragment(fragment);
            }
        }
    }
}
