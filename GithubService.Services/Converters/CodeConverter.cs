using GithubService.Models;
using GithubService.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace GithubService.Services.Converters
{
    public class CodeConverter : ICodeConverter
    {
        public (List<CodeFragment> newFragments, List<CodeFragment> modifiedFragments, List<CodeFragment>
            removedFragments) CompareFragmentLists(List<CodeFragment> oldFragmentList,
                List<CodeFragment> newFragmentList)
        {
            var newFragments = new List<CodeFragment>();
            var modifiedFragments = new List<CodeFragment>();
            var removedFragments = new List<CodeFragment>();

            foreach (var codeFragment in oldFragmentList)
            {
                var matchingCodeFragment = newFragmentList.FirstOrDefault(cf => CompareCodeFragments(cf, codeFragment));

                if (matchingCodeFragment == null)
                {
                    // Fragment was present in the old file but it's not in the new one
                    removedFragments.Add(codeFragment);
                }
                else if (matchingCodeFragment.Content != codeFragment.Content)
                {
                    // The content of the fragment changed
                    modifiedFragments.Add(matchingCodeFragment);
                }
            }

            foreach (var codeFragment in newFragmentList)
            {
                var matchingCodeFragment = oldFragmentList.FirstOrDefault(cf => CompareCodeFragments(cf, codeFragment));

                if (matchingCodeFragment == null)
                {
                    // Fragment was added
                    newFragments.Add(codeFragment);
                }
            }

            return (newFragments, modifiedFragments, removedFragments);
        }

        private static bool CompareCodeFragments(CodeFragment first, CodeFragment second)
            => first.Identifier == second.Identifier &&
               first.Language == second.Language &&
               first.Platform == second.Platform;
    }
}