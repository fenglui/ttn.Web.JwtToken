using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ttn.Web.JwtToken
{
    public class TokenResponse
    {
        [JsonIgnore]
        public string UserId { get; set; }

        /// <summary>
        /// Signed Jwt Token for authorizing requests.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Time to live for Jwt Token in seconds
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Refresh token provided to retrieve new Jwt access token.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

    }
}
