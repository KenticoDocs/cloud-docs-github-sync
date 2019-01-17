using System;
using System.Threading.Tasks;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudInternalClient
    {
        Task CreateNewVersionOfDefaultVariantAsync(Guid contentItemId);
    }
}
