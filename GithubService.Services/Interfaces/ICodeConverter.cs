using GithubService.Models;
using GithubService.Models.KenticoCloud;
using System.Collections.Generic;

namespace GithubService.Services.Interfaces
{
    public interface ICodeConverter
    {
        IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(IEnumerable<CodeFragment> fragments);

        CodeSamples ConvertToCodeSamples(CodenameCodeFragments codenameCodeFragments);

        (List<CodeFragment> newFragments, List<CodeFragment> modifiedFragments, List<CodeFragment> removedFragments) CompareFragmentLists(List<CodeFragment> oldFragmentList, List<CodeFragment> newFragmentList);
    }
}
