using System.Net.Http;

namespace EjectorTest
{
    internal class MockHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
                MaxAutomaticRedirections = 10,
                UseCookies = false
            });
        }
    }
}