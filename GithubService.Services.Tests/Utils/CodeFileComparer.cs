using GithubService.Models;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;

namespace GithubService.Services.Tests.Utils
{
    internal static class CodeFileComparerWrapper
    {
        private static Lazy<CodeFileComparer> Lazy => new Lazy<CodeFileComparer>();

        private static CodeFileComparer Comparer => Lazy.Value;

        public static EqualConstraint UsingCodeSampleFileComparer(this EqualConstraint constraint) =>
            constraint.Using(Comparer);

        private sealed class CodeFileComparer : IEqualityComparer<CodeFile>
        {
            private static Lazy<CodeFragmentComparer> LazyCodeFragmentComparer => new Lazy<CodeFragmentComparer>();
            private static CodeFragmentComparer CodeFragmentComparer => LazyCodeFragmentComparer.Value;

            public bool Equals(CodeFile x, CodeFile y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                var codeSamplesEqual = x.CodeFragments.Count == y.CodeFragments.Count;

                foreach (var codeSample in x.CodeFragments)
                {
                    var secondSample = y.CodeFragments[x.CodeFragments.IndexOf(codeSample)];
                    codeSamplesEqual = codeSamplesEqual && CodeFragmentComparer.Equals(codeSample, secondSample);
                }

                return string.Equals(x.FilePath, y.FilePath) && codeSamplesEqual;
            }

            public int GetHashCode(CodeFile obj)
            {
                var hashCode = 0;
                foreach (var sample in obj.CodeFragments)
                {
                    hashCode = 43 * (hashCode + sample.GetHashCode());
                }

                return hashCode * obj.FilePath.GetHashCode();
            }
        }
    }
}
