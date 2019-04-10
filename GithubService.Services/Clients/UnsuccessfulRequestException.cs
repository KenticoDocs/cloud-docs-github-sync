using System;
using System.Runtime.Serialization;

namespace GithubService.Services.Clients
{
    [Serializable]
    public sealed class UnsuccessfulRequestException : Exception
    {
        public UnsuccessfulRequestException(string message)
            : base(message)
        {
        }

        private UnsuccessfulRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}