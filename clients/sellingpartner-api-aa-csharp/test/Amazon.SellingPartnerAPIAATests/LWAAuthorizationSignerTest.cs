using System;
using System.Linq;
using System.Threading;

using Amazon.SellingPartnerAPIAA;

using Moq;

using RestSharp;

using Xunit;

namespace Amazon.SellingPartnerAPIAATests
{
    public class LWAAuthorizationSignerTest
    {
        private static readonly LWAAuthorizationCredentials LWAAuthorizationCredentials = new LWAAuthorizationCredentials()
        {
            ClientId = "cid",
            ClientSecret = "csecret",
            Endpoint = new Uri("https://www.amazon.com")
        };

        private LWAAuthorizationSigner lwaAuthorizationSignerUnderTest;

        public LWAAuthorizationSignerTest()
        {
            lwaAuthorizationSignerUnderTest = new LWAAuthorizationSigner(LWAAuthorizationCredentials);
        }

        [Fact]
        public void ConstructorInitializesLWAClientWithCredentials()
        {
            Assert.Equal(LWAAuthorizationCredentials, lwaAuthorizationSignerUnderTest.LWAClient.LWAAuthorizationCredentials);
        }

        [Fact]
        public async void RequestIsSignedFromLWAClientProvidedToken()
        {
            var expectedAccessToken = "foo";

            var mockLWAClient = new Mock<LWAClient>(LWAAuthorizationCredentials);
            mockLWAClient.Setup(lwaClient => lwaClient.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedAccessToken);
            lwaAuthorizationSignerUnderTest.LWAClient = mockLWAClient.Object;

            var restRequest = new RestRequest();
            restRequest = await lwaAuthorizationSignerUnderTest.SignAsync(restRequest);

            var actualAccessTokenHeader = restRequest.Parameters.Single(parameter =>
                ParameterType.HttpHeader.Equals(parameter.Type) && parameter.Name == LWAAuthorizationSigner.AccessTokenHeaderName);

            Assert.Equal(expectedAccessToken, actualAccessTokenHeader.Value);
        }
    }
}
