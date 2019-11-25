using System;
using System.Collections.Generic;
using System.Text;

namespace ttn.Web.JwtToken
{
    public class TokenObjectFactory
    {
        private static Dictionary<int, char> _charRef;

        public static TokenObject CreateToken(string userId)
        {
            var token = GenerateToken();
            var tokenObject = new TokenObject(userId, token, DateTime.UtcNow.AddDays(14));
            return tokenObject;
        }

        private static string GenerateToken()
        {
            var rand = new Random();

            if (_charRef == null)
                BuildRefTable();

            var newToken = new StringBuilder();
            for (var i = 0; i < 43; i++)
            {
                var c = _charRef[rand.Next(_charRef.Count)];

                newToken.Append(c);
            }

            return newToken.ToString();
        }

        private static void BuildRefTable()
        {
            _charRef = new Dictionary<int, char>();
            var key = 0;

            var arrays = new[] { new[] { 48, 58 }, new[] { 65, 91 }, new[] { 97, 123 } };

            foreach (var range in arrays)
            {
                for (var i = range[0]; i < range[1]; i++)
                {
                    _charRef.Add(key, (char)i);
                    key++;
                }
            }

        }
    }
}
