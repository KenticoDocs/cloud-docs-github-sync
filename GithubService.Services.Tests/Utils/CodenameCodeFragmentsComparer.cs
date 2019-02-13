using GithubService.Models;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GithubService.Services.Tests.Utils
{
    internal static class CodenameCodeFragmentsComparerWrapper
    {
        private static Lazy<CodenameCodeFragmentsComparer> Lazy => new Lazy<CodenameCodeFragmentsComparer>();

        private static CodenameCodeFragmentsComparer Comparer => Lazy.Value;

        public static EqualConstraint UsingCodenameCodeFragmentsComparer(this EqualConstraint constraint) =>
            constraint.Using(Comparer);

        private sealed class CodenameCodeFragmentsComparer : IEqualityComparer<CodenameCodeFragments>
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
                    var secondSample = y.CodeFragments.First(fragment => fragment.Language == codeSample.Language).Content;
                    codeSamplesEqual = codeSamplesEqual && codeSample.Content == secondSample;
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
