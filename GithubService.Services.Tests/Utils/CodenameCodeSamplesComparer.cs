using System;
using System.Collections.Generic;
using GithubService.Models.CodeSamples;
using NUnit.Framework.Constraints;

namespace GithubService.Services.Tests.Utils
{
    internal static class CodenameCodeSamplesComparerWrapper
    {
        private static Lazy<CodenameCodeSamplesComparer> Lazy => new Lazy<CodenameCodeSamplesComparer>();

        private static CodenameCodeSamplesComparer Comparer => Lazy.Value;

        public static EqualConstraint UsingCodenameCodeSamplesComparer(this EqualConstraint constraint) =>
            constraint.Using(Comparer);

        private sealed class CodenameCodeSamplesComparer : IEqualityComparer<CodenameCodeSamples>
        {
            public bool Equals(CodenameCodeSamples x, CodenameCodeSamples y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                var codeSamplesEqual = x.CodeSamples.Count == y.CodeSamples.Count;

                foreach (var codeSample in x.CodeSamples)
                {
                    var secondSample = y.CodeSamples[codeSample.Key];
                    codeSamplesEqual = codeSamplesEqual && codeSample.Value == secondSample;
                }

                return string.Equals(x.Codename, y.Codename) && codeSamplesEqual;
            }

            public int GetHashCode(CodenameCodeSamples obj)
            {
                var hashCode = 0;
                foreach (var sample in obj.CodeSamples)
                {
                    hashCode = 43 * (hashCode + sample.GetHashCode());
                }

                return hashCode * obj.Codename.GetHashCode();
            }
        }
    }
}
