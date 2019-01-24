using System;
using System.Collections.Generic;
using GithubService.Models;
using NUnit.Framework.Constraints;

namespace GithubService.Services.Tests.Utils
{
    internal static class CodenameCodeSamplesComparerWrapper
    {
        private static Lazy<CodenameCodeSamplesComparer> Lazy => new Lazy<CodenameCodeSamplesComparer>();

        private static CodenameCodeSamplesComparer Comparer => Lazy.Value;

        public static EqualConstraint UsingCodenameCodeSamplesComparer(this EqualConstraint constraint) =>
            constraint.Using(Comparer);

        private sealed class CodenameCodeSamplesComparer : IEqualityComparer<CodenameCodeFragments>
        {
            public bool Equals(CodenameCodeFragments x, CodenameCodeFragments y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                var codeSamplesEqual = x.CodeFragments.Count == y.CodeFragments.Count;

                foreach (var codeSample in x.CodeFragments)
                {
                    var secondSample = y.CodeFragments[codeSample.Key];
                    codeSamplesEqual = codeSamplesEqual && codeSample.Value == secondSample;
                }

                return string.Equals(x.Codename, y.Codename) && codeSamplesEqual;
            }

            public int GetHashCode(CodenameCodeFragments obj)
            {
                var hashCode = 0;
                foreach (var sample in obj.CodeFragments)
                {
                    hashCode = 43 * (hashCode + sample.GetHashCode());
                }

                return hashCode * obj.Codename.GetHashCode();
            }
        }
    }
}
