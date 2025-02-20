namespace CRM.BackgroundJob.Core.UnitTests.Helpers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockHttpMessageHandler : DelegatingHandler
    {
        private readonly IDictionary<string, HttpResponseMessage> messages;

        public MockHttpMessageHandler(IDictionary<string, HttpResponseMessage> messages)
        {
            this.messages = messages;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            if (messages.ContainsKey(request.RequestUri.ToString()))
                response = messages[request.RequestUri.ToString()] ?? new HttpResponseMessage(HttpStatusCode.NoContent);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}
