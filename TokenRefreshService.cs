using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ttn.Web.JwtToken
{
    public class TokenRefreshService
    {
        private HashSet<TokenObject> _hash;

        public TokenRefreshService()
        {
            _hash = new HashSet<TokenObject>();
        }

        public string GetRefreshToken(string userId)
        {
            var tokenObject = TokenObjectFactory.CreateToken(userId);

            //we invalidate if existing refresh token.
            _hash.RemoveWhere(t => t.UserId == tokenObject.UserId);
            _hash.Add(tokenObject);

            return tokenObject.Token;
        }

        public TokenRefreshServiceResponse ValidateRefreshToken(string refreshToken)
        {
            var token = _hash.FirstOrDefault(t => t.Token == refreshToken);
            if (token == null || token.ExpirationDate < DateTime.UtcNow)
                return new TokenRefreshServiceResponse { Authenticated = false };

            _hash.Remove(token);

            return new TokenRefreshServiceResponse { Authenticated = true, UserId = token.UserId };
        }
    }

}
