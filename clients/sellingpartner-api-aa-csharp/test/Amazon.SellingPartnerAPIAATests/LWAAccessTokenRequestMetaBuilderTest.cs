using System;
using System.Collections.Generic;

using Amazon.SellingPartnerAPIAA;

using Xunit;

namespace Amazon.SellingPartnerAPIAATests
{
    public class LWAAccessTokenRequestMetaBuilderTest
    {
        private const string TestClientId = "cid";
        private const string TestClientSecret = "csecret";
        private const string TestRefreshToken = "rtoken";
        private static readonly Uri TestUri = new Uri("https://www.amazon.com");
        private LWAAccessTokenRequestMetaBuilder lwaAccessTokenRequestMetaBuilderUnderTest;

        public LWAAccessTokenRequestMetaBuilderTest()
        {
            lwaAccessTokenRequestMetaBuilderUnderTest = new LWAAccessTokenRequestMetaBuilder();
        }

        [Fact]
        public void LWAAuthorizationCredentialsWithoutScopesBuildsSellerTokenRequestMeta()
        {
            var lwaAuthorizationCredentials = new LWAAuthorizationCredentials()
            {
                ClientId = TestClientId,
                ClientSecret = TestClientSecret,
                Endpoint = TestUri,
                RefreshToken = TestRefreshToken
            };

            var expected = new LWAAccessTokenRequestMeta()
            {
                ClientId = TestClientId,
                ClientSecret = TestClientSecret,
                GrantType = LWAAccessTokenRequestMetaBuilder.SellerAPIGrantType,
                RefreshToken = TestRefreshToken,
                Scope = null
            };

            var actual = lwaAccessTokenRequestMetaBuilderUnderTest.Build(lwaAuthorizationCredentials);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LWAAuthorizationCredentialsWithScopesBuildsSellerlessTokenRequestMeta()
        {
            var lwaAuthorizationCredentials = new LWAAuthorizationCredentials()
            {
                ClientId = TestClientId,
                ClientSecret = TestClientSecret,
                Endpoint = TestUri,
                Scopes = new List<string>() { ScopeConstants.ScopeMigrationAPI, ScopeConstants.ScopeNotificationsAPI }
            };

            var expected = new LWAAccessTokenRequestMeta()
            {
                ClientId = TestClientId,
                ClientSecret = TestClientSecret,
                GrantType = LWAAccessTokenRequestMetaBuilder.SellerlessAPIGrantType,
                Scope = string.Format("{0} {1}", ScopeConstants.ScopeMigrationAPI, ScopeConstants.ScopeNotificationsAPI),
                RefreshToken = null
            };

            var actual = lwaAccessTokenRequestMetaBuilderUnderTest.Build(lwaAuthorizationCredentials);

            Assert.Equal(expected, actual);
        }
    }
}
