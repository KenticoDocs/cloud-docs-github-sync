using System;
using System.Runtime.Serialization;

namespace GithubService
{
    [Serializable]
    public sealed class GithubServiceException : Exception
    {
        public GithubServiceException(string message)
            : base(message)
        {
        }

        private GithubServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
