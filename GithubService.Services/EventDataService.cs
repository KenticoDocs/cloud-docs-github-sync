using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubService.Models;
using GithubService.Repository;
using RepositoryModels = GithubService.Repository.Models;

namespace GithubService.Services
{
    public class EventDataService
    {
        private readonly IEventDataRepository _eventDataRepository;

        public EventDataService(IEventDataRepository eventDataRepository)
        {
            _eventDataRepository = eventDataRepository;
        }

        public async Task SaveCodeFragmentsAsync(
            string functionMode,
            IEnumerable<CodeFragment> addedCodeFragments,
            IEnumerable<CodeFragment> modifiedCodeFragments,
            IEnumerable<CodeFragment> removedCodeFragments
        )
        {
            var codeFragments = new List<RepositoryModels.CodeFragment>();

            codeFragments.AddRange(addedCodeFragments
                .Select(codeFragment =>
                    MapCodeFragment(codeFragment, CodeFragmentStatus.Added)));

            codeFragments.AddRange(modifiedCodeFragments
                .Select(codeFragment =>
                    MapCodeFragment(codeFragment, CodeFragmentStatus.Modified)));

            codeFragments.AddRange(removedCodeFragments
                .Select(codeFragment =>
                    MapCodeFragment(codeFragment, CodeFragmentStatus.Removed)));

            if (codeFragments.Count > 0)
            {
                var codeFragmentsEntity = new RepositoryModels.CodeFragmentsEntity
                {
                    CodeFragments = codeFragments,
                    Mode = functionMode,
                };

                await _eventDataRepository.StoreAsync(codeFragmentsEntity);
            }
        }

        private static RepositoryModels.CodeFragment MapCodeFragment(CodeFragment codeFragment, string status)
            => new RepositoryModels.CodeFragment
            {
                Content = codeFragment.Content,
                Identifier = codeFragment.Identifier,
                Language = codeFragment.Language,
                Platform = codeFragment.Platform,
                Status = status
            };
    }
}