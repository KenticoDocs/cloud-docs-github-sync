using System;
using System.Collections.Generic;
using GithubService.Models.CodeSamples;
using NUnit.Framework.Constraints;

namespace GithubService.Services.Tests.Utils
{
    internal static class CodeSampleFileComparerWrapper
    {
        private static Lazy<CodeSampleFileComparer> Lazy => new Lazy<CodeSampleFileComparer>();

        private static CodeSampleFileComparer Comparer => Lazy.Value;

        public static EqualConstraint UsingCodeSampleFileComparer(this EqualConstraint constraint) =>
            constraint.Using(Comparer);

        private sealed class CodeSampleFileComparer : IEqualityComparer<CodeSampleFile>
        {
            private static Lazy<CodeSampleComparer> LazyCodeSampleComparer => new Lazy<CodeSampleComparer>();
            private static CodeSampleComparer CodeSampleComparer => LazyCodeSampleComparer.Value;

            public bool Equals(CodeSampleFile x, CodeSampleFile y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                var codeSamplesEqual = x.CodeSamples.Count == y.CodeSamples.Count;

                foreach (var codeSample in x.CodeSamples)
                {
                    var secondSample = y.CodeSamples[x.CodeSamples.IndexOf(codeSample)];
                    codeSamplesEqual = codeSamplesEqual && CodeSampleComparer.Equals(codeSample, secondSample);
                }

                return string.Equals(x.FilePath, y.FilePath) && codeSamplesEqual;
            }

            public int GetHashCode(CodeSampleFile obj)
            {
                var hashCode = 0;
                foreach (var sample in obj.CodeSamples)
                {
                    hashCode = 43 * (hashCode + sample.GetHashCode());
                }

                return hashCode * obj.FilePath.GetHashCode();
            }
        }
    }
}
