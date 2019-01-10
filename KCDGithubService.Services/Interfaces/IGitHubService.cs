using System.Net.Http;
using System.Threading.Tasks;

namespace KCDGithubService.Services.Interfaces
{
    public interface IGitHubService
    {
        Task<HttpResponseMessage> GetFile(string url);
    }
}
