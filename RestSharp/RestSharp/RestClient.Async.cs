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
    using System;
    using System.Net;
    using System.Threading;
#if NET4 || MONODROID || MONOTOUCH || WP8
using System.Threading.Tasks;
#endif
    
    public partial class RestClient
    {
        /// <summary>
        /// Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        public virtual RestRequestAsyncHandle ExecuteAsync(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback)
        {
#if PocketPC
            string method = request.Method.ToString();
#else
            string method = Enum.GetName(typeof(Method), request.Method);
#endif

            switch (request.Method)
            {
                case Method.MERGE:
                case Method.PATCH:
                case Method.POST:
                case Method.PUT:
                    return this.ExecuteAsync(request, callback, method, DoAsPostAsync);

                default:
                    return this.ExecuteAsync(request, callback, method, DoAsGetAsync);
            }
        }

        /// <summary>
        /// Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        public virtual RestRequestAsyncHandle ExecuteAsyncGet(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            string httpMethod)
        {
            return this.ExecuteAsync(request, callback, httpMethod, DoAsGetAsync);
        }

        /// <summary>
        /// Executes a POST-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        public virtual RestRequestAsyncHandle ExecuteAsyncPost(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            string httpMethod)
        {
            request.Method = Method.POST; // Required by RestClient.BuildUri... 

            return this.ExecuteAsync(request, callback, httpMethod, DoAsPostAsync);
        }
        
        /// <summary>
        /// Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        public virtual RestRequestAsyncHandle ExecuteAsync<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback)
        {
            return this.ExecuteAsync(
                request,
                (response, asyncHandle) => this.DeserializeResponse(request, callback, response, asyncHandle));
        }

        /// <summary>
        /// Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        public virtual RestRequestAsyncHandle ExecuteAsyncGet<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            string httpMethod)
        {
            return this.ExecuteAsyncGet(
                request,
                (response, asyncHandle) => this.DeserializeResponse(request, callback, response, asyncHandle),
                httpMethod);
        }

        /// <summary>
        /// Executes a POST-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        public virtual RestRequestAsyncHandle ExecuteAsyncPost<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            string httpMethod)
        {
            return this.ExecuteAsyncPost(
                request,
                (response, asyncHandle) => this.DeserializeResponse(request, callback, response, asyncHandle),
                httpMethod);
        }
        
        private static HttpWebRequest DoAsGetAsync(IHttp http, Action<HttpResponse> responseCb, string method)
        {
            return http.AsGetAsync(responseCb, method);
        }

        private static HttpWebRequest DoAsPostAsync(IHttp http, Action<HttpResponse> responseCb, string method)
        {
            return http.AsPostAsync(responseCb, method);
        }

        private RestRequestAsyncHandle ExecuteAsync(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            string httpMethod,
            Func<IHttp, Action<HttpResponse>, string, HttpWebRequest> getWebRequest)
        {
            var http = this.httpFactory.Create();
            this.AuthenticateIfNeeded(this, request);

            this.ConfigureHttp(request, http);

            var asyncHandle = new RestRequestAsyncHandle();

            Action<HttpResponse> responseCb = r => this.ProcessResponse(request, r, asyncHandle, callback);

#if !PocketPC
            if (this.UseSynchronizationContext && SynchronizationContext.Current != null)
            {
                var ctx = SynchronizationContext.Current;
                var cb = responseCb;

                responseCb = resp => ctx.Post(s => cb(resp), null);
            }
#endif

            asyncHandle.WebRequest = getWebRequest(http, responseCb, httpMethod);
            return asyncHandle;
        }

        private void ProcessResponse(
            IRestRequest request,
            HttpResponse httpResponse,
            RestRequestAsyncHandle asyncHandle,
            Action<IRestResponse, RestRequestAsyncHandle> callback)
        {
            var restResponse = ConvertToRestResponse(request, httpResponse);
            callback(restResponse, asyncHandle);
        }

        private void DeserializeResponse<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            IRestResponse response,
            RestRequestAsyncHandle asyncHandle)
        {
            IRestResponse<T> restResponse;

            try
            {
                restResponse = this.Deserialize<T>(request, response);
            }
            catch (Exception ex)
            {
                restResponse = new RestResponse<T>
                {
                    Request = request,
                    ResponseStatus = ResponseStatus.Error,
                    ErrorMessage = ex.Message,
                    ErrorException = ex
                };
            }

            callback(restResponse, asyncHandle);
        }

#if NET4 || MONODROID || MONOTOUCH || WP8
        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        public virtual Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request)
        {
            return this.ExecuteGetTaskAsync<T>(request, CancellationToken.None);
        }

        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        public virtual Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Method = Method.GET;
            return this.ExecuteTaskAsync<T>(request, token);
        }

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        public virtual Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request)
        {
            return this.ExecutePostTaskAsync<T>(request, CancellationToken.None);
        }

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        public virtual Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Method = Method.POST;
            return this.ExecuteTaskAsync<T>(request, token);
        }

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        public virtual Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request)
        {
            return this.ExecuteTaskAsync<T>(request, CancellationToken.None);
        }

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        public virtual Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();

            try
            {
                var async = this.ExecuteAsync<T>(
                    request,
                    (response, _) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            taskCompletionSource.TrySetCanceled();
                        }
                        // Don't run TrySetException, since we should set Error properties and
                        // swallow exceptions to be consistent with sync methods
                        else
                        {
                            taskCompletionSource.TrySetResult(response);
                        }
                    });

                token.Register(() =>
                    {
                        async.Abort();
                        taskCompletionSource.TrySetCanceled();
                    });
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        public virtual Task<IRestResponse> ExecuteTaskAsync(IRestRequest request)
        {
            return this.ExecuteTaskAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        public virtual Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request)
        {
            return this.ExecuteGetTaskAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        public virtual Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Method = Method.GET;
            return this.ExecuteTaskAsync(request, token);
        }

        /// <summary>
        /// Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        public virtual Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request)
        {
            return this.ExecutePostTaskAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        public virtual Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Method = Method.POST;
            return this.ExecuteTaskAsync(request, token);
        }

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        public virtual Task<IRestResponse> ExecuteTaskAsync(IRestRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var taskCompletionSource = new TaskCompletionSource<IRestResponse>();

            try
            {
                var async = this.ExecuteAsync(
                    request,
                    (response, _) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            taskCompletionSource.TrySetCanceled();
                        }
                        // Don't run TrySetException, since we should set Error properties and
                        // swallow exceptions to be consistent with sync methods
                        else
                        {
                            taskCompletionSource.TrySetResult(response);
                        }
                    });

                token.Register(() =>
                    {
                        async.Abort();
                        taskCompletionSource.TrySetCanceled();
                    });
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }

            return taskCompletionSource.Task;
        }
#endif
    }
}
