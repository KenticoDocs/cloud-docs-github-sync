using System.Threading.Tasks;
using GithubService.Repository.Models;

namespace GithubService.Repository
{
    public interface IEventDataRepository
    {
        Task StoreAsync(CodeFragmentEvent codeFragmentEvent);
    }
}