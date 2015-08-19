namespace RestSharp.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using RestSharp.IntegrationTests.Helpers;

    using Xunit;

    public class AsyncTests
    {
        [Fact]
        public void Can_Perform_Get_Async()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");
            const string Val = "Basic async test";

            var resetEvent = new ManualResetEvent(false);

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.EchoValue(Val)))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(string.Empty);

                client.ExecuteAsync(
                    request, 
                    (response, asyncHandle) =>
                {
                    Assert.NotNull(response.Content);
                    Assert.Equal(Val, response.Content);
                    resetEvent.Set();
                });

                resetEvent.WaitOne();
            }
        }

        [Fact]
        public void Can_Perform_Get_Async_Without_Async_Handle()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");
            const string Val = "Basic async test";

            var resetEvent = new ManualResetEvent(false);

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.EchoValue(Val)))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(string.Empty);

                client.ExecuteAsync(
                    request, 
                    response =>
                {
                    Assert.NotNull(response.Content);
                    Assert.Equal(Val, response.Content);
                    resetEvent.Set();
                });

                resetEvent.WaitOne();
            }
        }

        [Fact]
        public void Can_Perform_Get_Task_Async()
        {
            const string BaseUrl = "http://localhost:8888/";
            const string Val = "Basic async task test";

            using (SimpleServer.Create(BaseUrl, Handlers.EchoValue(Val)))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(string.Empty);
                var task = client.ExecuteTaskAsync(request);

                task.Wait();

                Assert.NotNull(task.Result.Content);
                Assert.Equal(Val, task.Result.Content);
            }
        }

        [Fact]
        public void Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler()
        {
            const string BaseUrl = "http://localhost:8888/";
            const string ExceptionMessage = "Thrown from OnBeforeDeserialization";

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("success");

                request.OnBeforeDeserialization += r =>
                                                   {
                                                       throw new Exception(ExceptionMessage);
                                                   };

                var task = client.ExecuteTaskAsync<Response>(request);
                task.Wait();
                var response = task.Result;
                Assert.Equal(ExceptionMessage, response.ErrorMessage);
                Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
            }
        }

        [Fact]
        public void Can_Perform_Execute_Get_Task_Async_With_Response_Type()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("success");
                var task = client.ExecuteTaskAsync<Response>(request);

                task.Wait();

                Assert.Equal("Works!", task.Result.Data.Message);
            }
        }

        [Fact]
        public void Can_Perform_Get_Task_Async_With_Response_Type()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("success");
                var task = client.GetTaskAsync<Response>(request);

                task.Wait();

                Assert.Equal("Works!", task.Result.Message);
            }
        }

        [Fact]
        public void Can_Cancel_Get_Task_Async()
        {
            const string BaseUrl = "http://localhost:8888/";
            const string Val = "Basic async task test";

            using (SimpleServer.Create(BaseUrl, Handlers.EchoValue(Val)))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("timeout");
                var cancellationTokenSource = new CancellationTokenSource();
                var task = client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                Assert.True(task.IsCanceled);
            }
        }

        [Fact]
        public void Can_Cancel_GET_TaskAsync_With_Response_Type()
        {
            const string BaseUrl = "http://localhost:8888/";
            const string Val = "Basic async task test";

            using (SimpleServer.Create(BaseUrl, Handlers.EchoValue(Val)))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("timeout");
                var cancellationTokenSource = new CancellationTokenSource();
                var task = client.ExecuteTaskAsync<Response>(request, cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                Assert.True(task.IsCanceled);
            }
        }

        [Fact]
        public void Handles_Get_Request_Errors_Task_Async()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, UrlToStatusCodeHandler))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("404");
                var task = client.ExecuteTaskAsync(request);

                task.Wait();

                Assert.Equal(HttpStatusCode.NotFound, task.Result.StatusCode);
            }
        }

        [Fact]
        public void Handles_GET_Request_Errors_TaskAsync_With_Response_Type()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, this.UrlToStatusCodeHandler))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("404");
                var task = client.ExecuteTaskAsync<Response>(request);

                task.Wait();

                Assert.Null(task.Result.Data);
            }
        }

        [Fact]
        public void Can_Timeout_Get_Task_Async()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("timeout", Method.GET).AddBody("Body_Content");

                // Half the value of ResponseHandler.Timeout
                request.Timeout = 500;


                var task = client.ExecuteTaskAsync(request);
                task.Wait();
                var response = task.Result;
                Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);

            }
        }

        [Fact]
        public void Can_Timeout_PUT_TaskAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("timeout", Method.PUT).AddBody("Body_Content");

                // Half the value of ResponseHandler.Timeout
                request.Timeout = 500;

                var task = client.ExecuteTaskAsync(request);
                task.Wait();
                var response = task.Result;
                Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
            }
        }

        public void UrlToStatusCodeHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());
        }

        public class ResponseHandler
        {
            private void Error(HttpListenerContext context)
            {
                context.Response.StatusCode = 400;
                context.Response.Headers.Add("Content-Type", "application/xml");
                context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                <Response>
                <Error>
                <Message>Not found!</Message>
                </Error>
                </Response>");
            }

           private void Success(HttpListenerContext context)
            {
                context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                <Response>
                <Success>
                <Message>Works!</Message>
                </Success>
                </Response>");
            }

            private void Timeout(HttpListenerContext context)
            {
                Thread.Sleep(1000);
            }
        }

        public class Response
        {
            public string Message { get; set; }
        }
    }
}
// done some terrible renaming of methods in order to be according to the convention :D