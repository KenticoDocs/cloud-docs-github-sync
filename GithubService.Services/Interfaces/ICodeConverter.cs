using GithubService.Models;
using System.Collections.Generic;

namespace GithubService.Services.Interfaces
{
    public interface ICodeConverter
    {
        (List<CodeFragment> newFragments, List<CodeFragment> modifiedFragments, List<CodeFragment> removedFragments) CompareFragmentLists(List<CodeFragment> oldFragmentList, List<CodeFragment> newFragmentList);
    }
}
