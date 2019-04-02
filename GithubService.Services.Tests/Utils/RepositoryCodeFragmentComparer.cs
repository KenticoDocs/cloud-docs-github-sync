using System;
using System.Collections.Generic;
using System.Linq;
using RepositoryModels = GithubService.Repository.Models;

namespace GithubService.Services.Tests.Utils
{
    internal class RepositoryCodeFragmentComparer : IEqualityComparer<RepositoryModels.CodeFragment>
    {
        public bool Equals(RepositoryModels.CodeFragment x, RepositoryModels.CodeFragment y)
        {
            if (x == y) return true;
            if (x == null) return false;
            if (y == null) return false;
            if (x.GetType() != y.GetType()) return false;

            return string.Equals(x.Identifier, y.Identifier) &&
                   string.Equals(x.Content, y.Content) &&
                   x.Language == y.Language &&
                   x.Platform == y.Platform &&
                   x.Status == y.Status;
        }

        public bool SequenceEqual(IEnumerable<RepositoryModels.CodeFragment> x,
            IEnumerable<RepositoryModels.CodeFragment> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null) return false;
            if (y == null) return false;
            if (x.GetType() != y.GetType()) return false;

            return x
                .Select(codeFragment => 
                    Equals(codeFragment, y.FirstOrDefault(fragment => fragment.Codename == codeFragment.Codename)))
                .All(areFragmentsEqual => areFragmentsEqual);
        }

        public int GetHashCode(RepositoryModels.CodeFragment obj)
        {
            unchecked
            {
                var hashCode = (obj.Identifier != null ? obj.Identifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Content != null ? obj.Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Language != null ? obj.Language.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Platform != null ? obj.Platform.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Status != null ? obj.Status.GetHashCode() : 0);

                return hashCode;
            }
        }
    }

    internal static class RepositoryCodeFragmentComparerWrapper
    {
        private static Lazy<RepositoryCodeFragmentComparer> Lazy => new Lazy<RepositoryCodeFragmentComparer>();

        private static RepositoryCodeFragmentComparer Comparer => Lazy.Value;

        public static bool SequenceEqual(this IEnumerable<RepositoryModels.CodeFragment> fragments, IEnumerable<RepositoryModels.CodeFragment> fragments2) =>
            Comparer.SequenceEqual(fragments, fragments2);
    }
}