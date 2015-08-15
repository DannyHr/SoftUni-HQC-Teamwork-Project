namespace RestSharp
{
    using System.Net;

    public class RestRequestAsyncHandle
    {
        private HttpWebRequest webRequest;

        public RestRequestAsyncHandle()
        {
        }
        
        public RestRequestAsyncHandle(HttpWebRequest webRequest)
        {
            this.WebRequest = webRequest;
        }

        public HttpWebRequest WebRequest
        {
            get
            {
                return this.webRequest;
            }

            set
            {
                this.webRequest = value;
            }
        }
        
        public void Abort()
        {
            if (this.WebRequest != null)
            {
                this.WebRequest.Abort();
            }
        }
    }
}
