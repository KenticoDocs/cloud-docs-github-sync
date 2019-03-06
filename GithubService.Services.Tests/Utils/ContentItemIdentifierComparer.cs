using KenticoCloud.ContentManagement.Models.Items;
using System;
using System.Collections.Generic;

namespace GithubService.Services.Tests.Utils
{
    internal class ContentItemIdentifierComparer : IEqualityComparer<ContentItemIdentifier>
    {
        public Boolean Equals(ContentItemIdentifier x, ContentItemIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return x.Codename == y.Codename 
                && x.ExternalId == y.ExternalId 
                && x.Id == y.Id;
        }

        public int GetHashCode(ContentItemIdentifier obj)
        {
            unchecked
            {
                var hashCode = (obj.Codename != null ? obj.Codename.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ExternalId != null ? obj.ExternalId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Id != null ? obj.Id.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}
