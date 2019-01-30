using System;
using System.Threading.Tasks;

namespace GithubService.Services.Interfaces
{
    internal interface IKenticoCloudInternalClient
    {
        Task CreateNewVersionOfDefaultVariantAsync(Guid contentItemId);
    }
}
