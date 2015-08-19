namespace RestSharp.IntegrationTests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;

    using RestSharp.IntegrationTests.Helpers;

    using Xunit;

    public class MultipartFormDataTests
    {
        private readonly string expected =
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"foo\"" + Environment.NewLine + Environment.NewLine +
            "bar" + Environment.NewLine +
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"a name with spaces\"" + Environment.NewLine + Environment.NewLine +
            "somedata" + Environment.NewLine +
            "-------------------------------28947758029299--" + Environment.NewLine;


        [Fact]
        public void MultipartFormDataAsync()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, EchoHandler))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };

                this.AddParameters(request);

                client.ExecuteAsync(
                    request, 
                    (restResponse, handle) =>
                {
                    Console.WriteLine(restResponse.Content);
                    Assert.Equal(this.expected, restResponse.Content);
                });
            }
        }

        [Fact]
        public void MultipartFormData()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, EchoHandler))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };

                this.AddParameters(request);

                var response = client.Execute(request);

                Assert.Equal(this.expected, response.Content);
            }
        }

        [Fact]
        public void Always_Multipart_Form_Data_With_Parameter_Execute()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, EchoHandler))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("?json_route=/posts")
                                  {
                                      AlwaysMultipartFormData = true,
                                      Method = Method.POST,
                                  };
                request.AddParameter("title", "test", ParameterType.RequestBody);

                var response = client.Execute(request);
                Assert.Null(response.ErrorException);
            }
        }

        [Fact]
        public void Always_Multipart_Form_Data_With_Parameter_Execute_Task_Async()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, EchoHandler))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("?json_route=/posts")
                                  {
                                      AlwaysMultipartFormData = true,
                                      Method = Method.POST,
                                  };
                request.AddParameter("title", "test", ParameterType.RequestBody);

                var task = client.ExecuteTaskAsync(request).ContinueWith(
                    x =>
                        {
                            Assert.Null(x.Result.ErrorException);
                        });

                task.Wait();
            }
        }

        [Fact]
        public void Always_Multipart_Form_Data_With_Parameter_Execute_Async()
        {
            const string BaseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(BaseUrl, EchoHandler))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("?json_route=/posts")
                {
                    AlwaysMultipartFormData = true,
                    Method = Method.POST,
                };
                request.AddParameter("title", "test", ParameterType.RequestBody);
                IRestResponse syncResponse = null;

                using (var eventWaitHandle = new AutoResetEvent(false))
                {
                    client.ExecuteAsync(
                        request,
                        response =>
                        {
                            syncResponse = response;
                            eventWaitHandle.Set();
                        });

                    eventWaitHandle.WaitOne();
                }

                Assert.Null(syncResponse.ErrorException);
            }
        }

        private static void EchoHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = 200;

            var streamReader = new StreamReader(obj.Request.InputStream);

            obj.Response.OutputStream.WriteStringUtf8(streamReader.ReadToEnd());
        }

        private void AddParameters(RestRequest request)
        {
            request.AddParameter("foo", "bar");
            request.AddParameter("a name with spaces", "somedata");
        }
    }
}
// done some renaming of methonds and props