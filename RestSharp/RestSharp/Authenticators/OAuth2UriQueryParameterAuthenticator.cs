namespace RestSharp.Authenticators
{
    /// <summary>
    /// The OAuth 2 authenticator using URI query parameter.
    /// </summary>
    /// <remarks>
    /// Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.2
    /// </remarks>
    public class OAuth2UriQueryParameterAuthenticator : OAuth2Authenticator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2UriQueryParameterAuthenticator"/> class.
        /// </summary>
        /// <param name="accessToken">
        /// The access token.
        /// </param>
        public OAuth2UriQueryParameterAuthenticator(string accessToken)
            : base(accessToken)
        {
        }

        public override void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddParameter("oauth_token", this.AccessToken, ParameterType.GetOrPost);
        }
    }
}