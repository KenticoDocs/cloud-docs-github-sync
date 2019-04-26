using System;
using System.Collections.Generic;
using GithubService.Models.Github;
using NUnit.Framework.Constraints;

namespace GithubService.Services.Tests.Utils
{
    internal class GithubTreeNodesComparer : IComparer<GithubTreeNode>
    {
        private static Lazy<GithubTreeNodeComparer> LazyNodeComparer => new Lazy<GithubTreeNodeComparer>();

        private static GithubTreeNodeComparer NodeComparer => LazyNodeComparer.Value;

        public int Compare(GithubTreeNode x, GithubTreeNode y) => 
            NodeComparer.Equals(x, y) ? 0 : 1;

        internal class GithubTreeNodeComparer : IEqualityComparer<GithubTreeNode>
        {
            public bool Equals(GithubTreeNode x, GithubTreeNode y)
            {
                if (x == y) return true;
                if (x == null) return false;
                if (y == null) return false;
                if (x.GetType() != y.GetType()) return false;

                return string.Equals(x.Id, y.Id) &&
                       string.Equals(x.Path, y.Path) &&
                       string.Equals(x.Type, y.Type);
            }

            public int GetHashCode(GithubTreeNode obj)
            {
                unchecked
                {
                    var hashCode = obj.Id != null ? obj.Id.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (obj.Path != null ? obj.Path.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Type != null ? obj.Type.GetHashCode() : 0);

                    return hashCode;
                }
            }
        }
    }

    internal static class GithubTreeNodeWrapper
    {
        private static Lazy<GithubTreeNodesComparer> LazyComparer => new Lazy<GithubTreeNodesComparer>();

        private static GithubTreeNodesComparer Comparer => LazyComparer.Value;

        public static CollectionItemsEqualConstraint UsingGithubNodeTreeComparer(this CollectionItemsEqualConstraint constraint) =>
            constraint.Using(Comparer);
    }
}
