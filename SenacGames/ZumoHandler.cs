using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SenacGames
{
    public class ZumoHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

            HttpResponseMessage response = null;
            response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
