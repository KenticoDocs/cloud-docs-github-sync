using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubService.Models;
using GithubService.Services.Converters;

namespace GithubService.Shared
{
    internal static class KenticoCloudUtils
    {
        private static Lazy<CodeConverter> LazyCodeConverter => new Lazy<CodeConverter>();
        private static readonly CodeConverter CodeConverter = LazyCodeConverter.Value;

        internal static async Task ExecuteCodeFragmentChanges(
            Func<CodenameCodeFragments, Task> actionSelectorForMultipleFragments,
            Func<CodeFragment, Task> actionSelectorForSingleFragment,
            IEnumerable<CodeFragment> fragments)
        {
            var multipleCodeFragments = fragments.Where(codeFragment => codeFragment.Type == CodeFragmentType.Multiple);
            var singleCodeFragment = fragments.Where(codeFragment => codeFragment.Type == CodeFragmentType.Single);
            var multipleCodeFragmentsByCodename = CodeConverter.ConvertToCodenameCodeFragments(multipleCodeFragments);

            await Task.WhenAll(multipleCodeFragmentsByCodename.Select(actionSelectorForMultipleFragments));
            await Task.WhenAll(singleCodeFragment.Select(actionSelectorForSingleFragment));
        }
    }
}
