using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ttn.Web.JwtToken
{
    public class TokenObject : IEquatable<TokenObject>
    {
        public TokenObject(string userId, string token, DateTime expiration)
        {
            UserId = userId;
            Token = token;
            ExpirationDate = expiration;
        }

        public string UserId { get; set; }


        public string Token { get; set; }


        public DateTime ExpirationDate { get; set; }

        public override string ToString()
        {
            return $"{Token}::{ExpirationDate.ToString(CultureInfo.InvariantCulture)}";
        }

        public bool Equals(TokenObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UserId == other.UserId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TokenObject)obj);
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }
    }
}
