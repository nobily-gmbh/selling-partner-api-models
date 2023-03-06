using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Amazon.SellingPartnerAPIAA
{
    public class LWAClient
    {
        public const string AccessTokenKey = "access_token";
        // public const string JsonMediaType = "application/json; charset=utf-8";

        private RestClient _restClient;

        public RestClient RestClient
        {
            get => _restClient;
            set
            {
                _restClient = value;
                _restClient.UseNewtonsoftJson();
            }
        }

        public LWAAccessTokenRequestMetaBuilder LWAAccessTokenRequestMetaBuilder { get; set; }
        public LWAAuthorizationCredentials LWAAuthorizationCredentials { get; private set; }


        public LWAClient(LWAAuthorizationCredentials lwaAuthorizationCredentials)
        {
            LWAAuthorizationCredentials = lwaAuthorizationCredentials;
            LWAAccessTokenRequestMetaBuilder = new LWAAccessTokenRequestMetaBuilder();
            RestClient = new RestClient(LWAAuthorizationCredentials.Endpoint.GetLeftPart(UriPartial.Authority));
        }

        /// <summary>
        /// Retrieves access token from LWA
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>LWA Access Token</returns>
        public virtual async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            LWAAccessTokenRequestMeta lwaAccessTokenRequestMeta = LWAAccessTokenRequestMetaBuilder.Build(LWAAuthorizationCredentials);
            var accessTokenRequest = new RestRequest(LWAAuthorizationCredentials.Endpoint.AbsolutePath, Method.Post);

            string jsonRequestBody = JsonConvert.SerializeObject(lwaAccessTokenRequestMeta);

            // accessTokenRequest.AddParameter(JsonMediaType, jsonRequestBody, ParameterType.RequestBody);
            accessTokenRequest.AddJsonBody(lwaAccessTokenRequestMeta);

            string accessToken;
            try
            {
                var response = await RestClient.ExecuteAsync(accessTokenRequest, cancellationToken);

                if (!IsSuccessful(response) || string.IsNullOrEmpty(response.Content))
                {
                    throw new IOException("Unsuccessful LWA token exchange", response.ErrorException);
                }

                var responseJson = JObject.Parse(response.Content);

                accessToken = responseJson.GetValue(AccessTokenKey)?.ToString();
            }
            catch (Exception e)
            {
                throw new SystemException("Error getting LWA Access Token", e);
            }

            if (accessToken == null)
            {
                throw new SystemException("Error getting LWA Access Token - no access token returned");
            }

            return accessToken;
        }

        private bool IsSuccessful(RestResponse response)
        {
            int statusCode = (int)response.StatusCode;
            return statusCode >= 200 && statusCode <= 299 && response.ResponseStatus == ResponseStatus.Completed;
        }
    }
}
