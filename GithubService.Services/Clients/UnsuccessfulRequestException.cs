using System;

namespace GithubService.Services.Clients
{
    public sealed class UnsuccessfulRequestException : Exception
    {
        public UnsuccessfulRequestException(string message)
            : base(message)
        {
        }
    }
}
