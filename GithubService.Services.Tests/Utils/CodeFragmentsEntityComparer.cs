using System;
using System.Collections.Generic;
using GithubService.Repository.Models;

namespace GithubService.Services.Tests.Utils
{
    internal class CodeFragmentsEntityComparer : IEqualityComparer<CodeFragmentsEntity>
    {
        public bool Equals(CodeFragmentsEntity x, CodeFragmentsEntity y)
        {
            if (x == y) return true;
            if (x == null) return false;
            if (y == null) return false;
            if (x.GetType() != y.GetType()) return false;

            return string.Equals(x.Mode, y.Mode) &&
                   x.CodeFragments.SequenceEqual(y.CodeFragments);
        }

        public int GetHashCode(CodeFragmentsEntity obj)
        {
            unchecked
            {
                var hashCode = (obj.Mode != null ? obj.Mode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.CodeFragments != null ? obj.CodeFragments.GetHashCode() : 0);

                return hashCode;
            }
        }
    }

    internal static class CodeFragmentsEntityComparerWrapper
    {
        private static Lazy<CodeFragmentsEntityComparer> Lazy => new Lazy<CodeFragmentsEntityComparer>();

        private static CodeFragmentsEntityComparer Comparer => Lazy.Value;

        public static bool DoesEqual(this CodeFragmentsEntity fragments, CodeFragmentsEntity fragments2) =>
            Comparer.Equals(fragments, fragments2);
    }
}
