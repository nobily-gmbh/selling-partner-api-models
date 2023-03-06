using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

using Amazon.SellingPartnerAPIAA;

using Moq;
using Moq.Contrib.HttpClient;

using Newtonsoft.Json.Linq;

using RestSharp;

using Xunit;

namespace Amazon.SellingPartnerAPIAATests
{
    public class LWAClientTest : IDisposable
    {
        private const string TestClientSecret = "cSecret";
        private const string TestClientId = "cId";
        private const string TestRefreshGrantType = "rToken";

        private static readonly Uri TestEndpoint = new Uri("https://www.amazon.com/lwa");
        private static readonly LWAAuthorizationCredentials LWAAuthorizationCredentials = new LWAAuthorizationCredentials
        {
            ClientId = TestClientId,
            ClientSecret = TestClientSecret,
            RefreshToken = TestRefreshGrantType,
            Endpoint = TestEndpoint
        };

        private static readonly string ResponseText = @"{access_token:'Azta|foo'}";
        private static readonly string ResponseType = "application/json";

        private Mock<LWAAccessTokenRequestMetaBuilder> mockLWAAccessTokenRequestMetaBuilder = new();
        private Mock<HttpMessageHandler> mockHttpMessageHandler = new();
        private RestClient restClient;

        public LWAClientTest()
        {
            var httpClient = mockHttpMessageHandler.CreateClient();
            httpClient.BaseAddress = new Uri(TestEndpoint.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped));

            restClient = new RestClient(httpClient);
        }

        public void Dispose()
        {
            restClient.Dispose();
        }

        [Fact]
        public void InitializeLWAAuthorizationCredentials()
        {
            var lwaClientUnderTest = new LWAClient(LWAAuthorizationCredentials);
            Assert.Equal(LWAAuthorizationCredentials, lwaClientUnderTest.LWAAuthorizationCredentials);
        }

        [Fact]
        public async void MakeRequestFromMeta()
        {
            var expectedLWAAccessTokenRequestMeta = new LWAAccessTokenRequestMeta()
            {
                ClientSecret = "expectedSecret",
                ClientId = "expectedClientId",
                RefreshToken = "expectedRefreshToken",
                GrantType = "expectedGrantType"
            };

            HttpRequestMessage request = null!;
            string requestBody = null!;
            mockHttpMessageHandler.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK, ResponseText, ResponseType)
                .Callback((HttpRequestMessage req, CancellationToken _) =>
                {
                    request = req;
                    requestBody = req.Content!.ReadAsStringAsync(CancellationToken.None).Result;
                });

            mockLWAAccessTokenRequestMetaBuilder.Setup(builder => builder.Build(LWAAuthorizationCredentials))
                .Returns(expectedLWAAccessTokenRequestMeta);

            var lwaClientUnderTest = new LWAClient(LWAAuthorizationCredentials)
            {
                RestClient = restClient,
                LWAAccessTokenRequestMetaBuilder = mockLWAAccessTokenRequestMetaBuilder.Object
            };

            await lwaClientUnderTest.GetAccessTokenAsync();

            Assert.NotNull(request);
            Assert.Equal(HttpMethod.Post, request.Method);

            var jsonRequestBody = JObject.Parse(requestBody);

            Assert.Equal(TestEndpoint.AbsolutePath, request.RequestUri!.AbsolutePath);
            Assert.Equal(expectedLWAAccessTokenRequestMeta.RefreshToken, jsonRequestBody.GetValue("refresh_token"));
            Assert.Equal(expectedLWAAccessTokenRequestMeta.GrantType, jsonRequestBody.GetValue("grant_type"));
            Assert.Equal(expectedLWAAccessTokenRequestMeta.ClientId, jsonRequestBody.GetValue("client_id"));
            Assert.Equal(expectedLWAAccessTokenRequestMeta.ClientSecret, jsonRequestBody.GetValue("client_secret"));
        }

        [Fact]
        public async void ReturnAccessTokenFromResponse()
        {
            mockHttpMessageHandler.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK, ResponseText, ResponseType);

            var lwaClientUnderTest = new LWAClient(LWAAuthorizationCredentials)
            {
                RestClient = restClient
            };

            var accessToken = await lwaClientUnderTest.GetAccessTokenAsync();

            Assert.Equal("Azta|foo", accessToken);
        }

        [Fact]
        public async void UnsuccessfulPostThrowsException()
        {
            mockHttpMessageHandler.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.BadRequest);

            var lwaClientUnderTest = new LWAClient(LWAAuthorizationCredentials)
            {
                RestClient = restClient
            };

            var systemException = await Assert.ThrowsAsync<SystemException>(() => lwaClientUnderTest.GetAccessTokenAsync());
            Assert.IsType<IOException>(systemException.InnerException);
        }

        [Fact]
        public async void MissingAccessTokenInResponseThrowsException()
        {
            mockHttpMessageHandler.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK);

            var lwaClientUnderTest = new LWAClient(LWAAuthorizationCredentials)
            {
                RestClient = restClient
            };

            await Assert.ThrowsAsync<SystemException>(() => lwaClientUnderTest.GetAccessTokenAsync());
        }
    }
}
