using GithubService.Models.KenticoCloud;
using KenticoCloud.ContentManagement.Models.Items;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GithubService.Services.Tests.Utils
{
    internal static class CodeSamplesWrapper
    {
        private static Lazy<CodeSamplesComparer> Lazy => new Lazy<CodeSamplesComparer>();

        private static CodeSamplesComparer Comparer => Lazy.Value;

        public static EqualConstraint UsingCodeSamplesComparer(this EqualConstraint constraint) =>
            constraint.Using(Comparer);

        private sealed class CodeSamplesComparer : IEqualityComparer<CodeSamples>
        {
            public bool Equals(CodeSamples firstCodeSamples, CodeSamples secondCodeSamples)
            {
                if (ReferenceEquals(firstCodeSamples, secondCodeSamples)) return true;
                if (ReferenceEquals(firstCodeSamples, null)) return false;
                if (ReferenceEquals(secondCodeSamples, null)) return false;
                if (firstCodeSamples.GetType() != secondCodeSamples.GetType()) return false;

                return CompareSamples(firstCodeSamples.Samples, secondCodeSamples.Samples, new ContentItemIdentifierComparer());
            }

            public int GetHashCode(CodeSamples obj)
            {
                var hashCode = 0;
                foreach (var sample in obj.Samples)
                {
                    hashCode = 43 * (hashCode + sample.GetHashCode());
                }

                return hashCode;
            }

            private static bool CompareSamples(
                IEnumerable<ContentItemIdentifier> codeSamples1, 
                IEnumerable<ContentItemIdentifier> codeSamples2, 
                IEqualityComparer<ContentItemIdentifier> comparer
            ) 
                => codeSamples2.Count() == codeSamples1.Count() 
                    && codeSamples1.Intersect(codeSamples2, comparer).Count() == codeSamples2.Count();
        }
    }
}
