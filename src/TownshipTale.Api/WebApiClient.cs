using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownshipTale.Api.Core.Server.Console;
using TownshipTale.Api.Identity;
using TownshipTale.Api.Server.Console;

namespace TownshipTale.Api
{
    public class WebApiClient : ApiClientBase
    {
        public WebApiClient(ApiClientConfiguration configuration, Func<AccessToken> authorizationCallback) 
            : base(configuration, authorizationCallback)
        {
        }
    }
}
