namespace RestSharp.Authenticators.OAuth
{
    using System;

#if !SILVERLIGHT && !WINDOWS_PHONE && !PocketPC
    [Serializable]
#endif
    public enum OAuthType
    {
        RequestToken,
        AccessToken,
        ProtectedResource,
        ClientAuthentication
    }
}
