using System;
using System.Collections.Generic;
using System.Text;

namespace ttn.Web.JwtToken
{
    public class TokenRefreshServiceResponse
    {
        public bool Authenticated { get; set; }

        public string UserId { get; set; }
    }
}
