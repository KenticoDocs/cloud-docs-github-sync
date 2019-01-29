using GithubService.Models;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;

namespace GithubService.Services.Tests.Utils
{
    internal class CodeFragmentComparer : IEqualityComparer<CodeFragment>
    {
        public bool Equals(CodeFragment x, CodeFragment y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return string.Equals(x.Codename, y.Codename) && string.Equals(x.Content, y.Content) && x.Language == y.Language && x.Type == y.Type;
        }

        public int GetHashCode(CodeFragment obj)
        {
            unchecked
            {
                var hashCode = (obj.Codename != null ? obj.Codename.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Content != null ? obj.Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)obj.Language;

                return hashCode;
            }
        }
    }

    internal static class CodeFragmentComparerWrapper
    {
        private static Lazy<CodeFragmentComparer> Lazy => new Lazy<CodeFragmentComparer>();

        private static CodeFragmentComparer Comparer => Lazy.Value;

        public static SomeItemsConstraint UsingCodeFragmentComparer(this SomeItemsConstraint constraint) =>
            constraint.Using(Comparer);
    }
}
