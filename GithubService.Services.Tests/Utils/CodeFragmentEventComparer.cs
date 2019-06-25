using System;
using System.Collections.Generic;
using GithubService.Repository.Models;
using GithubService.Tests.Common.Comparers;

namespace GithubService.Services.Tests.Utils
{ 
    public class CodeFragmentEventComparer : IEqualityComparer<CodeFragmentEvent>
    {
        public bool Equals(CodeFragmentEvent x, CodeFragmentEvent y)
        {
            if (x == y) return true;
            if (x == null) return false;
            if (y == null) return false;
            if (x.GetType() != y.GetType()) return false;

            return string.Equals(x.Mode, y.Mode) &&
                   x.CodeFragments.SequenceEqual(y.CodeFragments);
        }

        public int GetHashCode(CodeFragmentEvent obj)
        {
            unchecked
            {
                var hashCode = (obj.Mode != null ? obj.Mode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.CodeFragments != null ? obj.CodeFragments.GetHashCode() : 0);

                return hashCode;
            }
        }
    }

    public static class CodeFragmentEventComparerWrapper
    {
        private static Lazy<CodeFragmentEventComparer> Lazy => new Lazy<CodeFragmentEventComparer>();

        private static CodeFragmentEventComparer Comparer => Lazy.Value;

        public static bool DoesEqual(this CodeFragmentEvent x, CodeFragmentEvent y) =>
            Comparer.Equals(x, y);
    }
}
