#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0063 // 'using' statement can be simplified
#pragma warning disable IDE0090 // Use 'new(...)'

using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDisposedReproDotNet48
{
    internal class SimpleClient
    {
        // For IHttpClientFactory
        private readonly ServiceCollection m_serviceCollection = new ServiceCollection();

        // For IHttpClientFactory
        private static string HttpClientKey => $"HttpClient_c0f58122315c4e879c6cf1e2568e4ffa";

        private readonly CancellationToken m_cancelToken;

        public SimpleClient(Uri baseUrl, CancellationToken cancelToken)
        {
            m_cancelToken = cancelToken;
            m_serviceCollection
            .AddHttpClient(HttpClientKey)
            .ConfigureHttpClient(client => client.BaseAddress = baseUrl)
            .ConfigurePrimaryHttpMessageHandler((handler) =>
                 new HttpClientHandler()
                 {
                     UseDefaultCredentials = true,
                     UseProxy = false
                 }
            );
        }

        public MemoryStream GetContentStream()
        {
            try
            {
                var response = SendRequest(() =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);
                    request.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
                    return request;
                });

                using (response)
                {
                    var stream = GetResponseStream(response);
                    var memStream = new MemoryStream();
                    stream.CopyTo(memStream);
                    GC.KeepAlive(response);
                    memStream.Position = 0;
                    return memStream;
                }
            }
            catch(TaskCanceledException)
            {
                return new MemoryStream();
            }
        }

        #region Helpers

        private HttpClient GetClient()
            => m_serviceCollection
            .BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(HttpClientKey);

        private static void EnsureValidResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                throw new InvalidOperationException("Received invalid response.");
            }
        }

        private HttpResponseMessage SendRequest(Func<HttpRequestMessage> requestFactory)
        {
            using (var request = requestFactory())
                return SendRequestCore(request);
        }

        private HttpResponseMessage SendRequestCore(HttpRequestMessage request)
        {
            using (var httpClient = GetClient())
            {
                var response = httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, m_cancelToken)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                EnsureValidResponse(response);
                return response;
            }
        }

        private static Stream GetResponseStream(HttpResponseMessage response)
            => response.Content
                   .ReadAsStreamAsync()
                   .ConfigureAwait(false)
                   .GetAwaiter()
                   .GetResult();

        #endregion Helpers
    }
}
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0063 // 'using' statement can be simplified
#pragma warning restore IDE0079 // Remove unnecessary suppression