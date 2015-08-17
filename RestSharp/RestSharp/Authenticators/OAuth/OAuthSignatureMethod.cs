namespace RestSharp.Authenticators.OAuth
{
    using System;

#if !SILVERLIGHT && !WINDOWS_PHONE && !PocketPC
    [Serializable]
#endif
    public enum OAuthSignatureMethod
    {
        HmacSha1,
        PlainText,
        RsaSha1
    }
}
