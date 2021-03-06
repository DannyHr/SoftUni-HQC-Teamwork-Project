﻿#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

namespace RestSharp
{
#if FRAMEWORK || PocketPC
    using System;
    using System.Net;

#if !MONOTOUCH && !MONODROID && !PocketPC
    using System.Web;
#endif
    using RestSharp.Extensions;

    /// <summary>
    /// HttpWebRequest wrapper (sync methods)
    /// </summary>
    public partial class Http
    {
        /// <summary>
        /// Execute a POST request
        /// </summary>
        public HttpResponse Post()
        {
            return this.PostPutInternal("POST");
        }

        /// <summary>
        /// Execute a PUT request
        /// </summary>
        public HttpResponse Put()
        {
            return this.PostPutInternal("PUT");
        }

        /// <summary>
        /// Execute a GET request
        /// </summary>
        public HttpResponse Get()
        {
            return this.GetStyleMethodInternal("GET");
        }

        /// <summary>
        /// Execute a HEAD request
        /// </summary>
        public HttpResponse Head()
        {
            return this.GetStyleMethodInternal("HEAD");
        }

        /// <summary>
        /// Execute an OPTIONS request
        /// </summary>
        public HttpResponse Options()
        {
            return this.GetStyleMethodInternal("OPTIONS");
        }

        /// <summary>
        /// Execute a DELETE request
        /// </summary>
        public HttpResponse Delete()
        {
            return this.GetStyleMethodInternal("DELETE");
        }

        /// <summary>
        /// Execute a PATCH request
        /// </summary>
        public HttpResponse Patch()
        {
            return this.PostPutInternal("PATCH");
        }

        /// <summary>
        /// Execute a MERGE request
        /// </summary>
        public HttpResponse Merge()
        {
            return this.PostPutInternal("MERGE");
        }

        /// <summary>
        /// Execute a GET-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpResponse AsGet(string httpMethod)
        {
#if PocketPC
            return this.GetStyleMethodInternal(httpMethod.ToUpper());
#else
            return this.GetStyleMethodInternal(httpMethod.ToUpperInvariant());
#endif
        }

        /// <summary>
        /// Execute a POST-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpResponse AsPost(string httpMethod)
        {
#if PocketPC
            return this.PostPutInternal(httpMethod.ToUpper());
#else
            return this.PostPutInternal(httpMethod.ToUpperInvariant());
#endif
        }

        private HttpResponse GetStyleMethodInternal(string method)
        {
            var webRequest = this.ConfigureWebRequest(method, this.Url);

            if (this.HasBody && (method == "DELETE" || method == "OPTIONS"))
            {
                webRequest.ContentType = this.RequestContentType;
                this.WriteRequestBody(webRequest);
            }

            return this.GetResponse(webRequest);
        }

        private HttpResponse PostPutInternal(string method)
        {
            var webRequest = this.ConfigureWebRequest(method, this.Url);

            this.PreparePostBody(webRequest);

            this.WriteRequestBody(webRequest);
            return this.GetResponse(webRequest);
        }

        partial void AddSyncHeaderActions()
        {
            this.restrictedHeaderActions.Add("Connection", (r, v) => r.Connection = v);
            this.restrictedHeaderActions.Add("Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v));
            this.restrictedHeaderActions.Add("Expect", (r, v) => r.Expect = v);
            this.restrictedHeaderActions.Add("If-Modified-Since", (r, v) => r.IfModifiedSince = Convert.ToDateTime(v));
            this.restrictedHeaderActions.Add("Referer", (r, v) => r.Referer = v);
            this.restrictedHeaderActions.Add("Transfer-Encoding", (r, v) => { r.TransferEncoding = v; r.SendChunked = true; });
            this.restrictedHeaderActions.Add("User-Agent", (r, v) => r.UserAgent = v);
        }

        private void ExtractErrorResponse(HttpResponse httpResponse, Exception ex)
        {
            var webException = ex as WebException;

            if (webException != null && webException.Status == WebExceptionStatus.Timeout)
            {
                httpResponse.ResponseStatus = ResponseStatus.TimedOut;
                httpResponse.ErrorMessage = ex.Message;
                httpResponse.ErrorException = webException;
                return;
            }

            httpResponse.ErrorMessage = ex.Message;
            httpResponse.ErrorException = ex;
            httpResponse.ResponseStatus = ResponseStatus.Error;
        }

        private HttpResponse GetResponse(HttpWebRequest request)
        {
            var response = new HttpResponse { ResponseStatus = ResponseStatus.None };

            try
            {
                var webResponse = GetRawResponse(request);
                this.ExtractResponseData(response, webResponse);
            }
            catch (Exception ex)
            {
                this.ExtractErrorResponse(response, ex);
            }

            return response;
        }

        private static HttpWebResponse GetRawResponse(HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                // Check to see if this is an HTTP error or a transport error.
                // In cases where an HTTP error occurs ( status code >= 400 )
                // return the underlying HTTP response, otherwise assume a
                // transport exception (ex: connection timeout) and
                // rethrow the exception

                if (ex.Response is HttpWebResponse)
                {
                    return ex.Response as HttpWebResponse;
                }

                throw;
            }
        }

        private void WriteRequestBody(HttpWebRequest webRequest)
        {
            if (this.HasBody || this.HasFiles || this.AlwaysMultipartFormData)
            {
#if !WINDOWS_PHONE && !PocketPC
                webRequest.ContentLength = this.CalculateContentLength();
#endif
            }

            using (var requestStream = webRequest.GetRequestStream())
            {
                if (this.HasFiles || this.AlwaysMultipartFormData)
                {
                    this.WriteMultipartFormData(requestStream);
                }
                else if (this.RequestBodyBytes != null)
                {
                    requestStream.Write(this.RequestBodyBytes, 0, this.RequestBodyBytes.Length);
                }
                else
                {
                    this.WriteStringTo(requestStream, this.RequestBody);
                }
            }
        }

        // TODO: Try to merge the shared parts between ConfigureWebRequest and ConfigureAsyncWebRequest (quite a bit of code
        // TODO: duplication at the moment).
        private HttpWebRequest ConfigureWebRequest(string method, Uri url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
#if !PocketPC
            webRequest.UseDefaultCredentials = this.UseDefaultCredentials;
#endif
            webRequest.PreAuthenticate = this.PreAuthenticate;

            webRequest.ServicePoint.Expect100Continue = false;

            this.AppendHeaders(webRequest);
            this.AppendCookies(webRequest);

            webRequest.Method = method;

            // make sure Content-Length header is always sent since default is -1
            if (!this.HasFiles && !this.AlwaysMultipartFormData)
            {
                webRequest.ContentLength = 0;
            }

            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

#if FRAMEWORK
            if (this.ClientCertificates != null)
            {
                webRequest.ClientCertificates.AddRange(this.ClientCertificates);
            }
#endif

            if (this.UserAgent.HasValue())
            {
                webRequest.UserAgent = this.UserAgent;
            }

            if (this.Timeout != 0)
            {
                webRequest.Timeout = this.Timeout;
            }

            if (this.ReadWriteTimeout != 0)
            {
                webRequest.ReadWriteTimeout = this.ReadWriteTimeout;
            }

            if (this.Credentials != null)
            {
                webRequest.Credentials = this.Credentials;
            }

            if (this.Proxy != null)
            {
                webRequest.Proxy = this.Proxy;
            }

#if FRAMEWORK
            if (this.CachePolicy != null)
            {
                webRequest.CachePolicy = this.CachePolicy;
            }
#endif

            webRequest.AllowAutoRedirect = this.FollowRedirects;
            if (this.FollowRedirects && this.MaxRedirects.HasValue)
            {
                webRequest.MaximumAutomaticRedirections = this.MaxRedirects.Value;
            }

            return webRequest;
        }
    }
}

#endif
