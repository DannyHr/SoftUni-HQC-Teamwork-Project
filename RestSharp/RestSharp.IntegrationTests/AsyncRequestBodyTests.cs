namespace RestSharp.IntegrationTests
{
    using System.IO;
    using System.Net;
    using System.Threading;
    using RestSharp.IntegrationTests.Helpers;
    using Xunit;

    public class AsyncRequestBodyTests
    {
        private const string BaseUrl = "http://localhost:8888/";

        [Fact]
        public void Can_Not_Be_Added_To_GET_Request()
        {
            const Method HttpMethod = Method.GET;

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.Resource, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                var resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasNoRequestBody();
            }
        }

        [Fact]
        public void Can_Be_Added_To_POST_Request()
        {
            const Method HttpMethod = Method.POST;

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.Resource, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                var resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(ContentType, BodyData);
            }
        }

        [Fact]
        public void Can_Be_Added_To_PUT_Request()
        {
            const Method HttpMethod = Method.PUT;

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.Resource, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                var resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(ContentType, BodyData);
            }
        }

        [Fact]
        public void Can_Be_Added_To_DELETE_Request()
        {
            const Method HttpMethod = Method.DELETE;

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.Resource, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                var resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(ContentType, BodyData);
            }
        }

        [Fact]
        public void Can_Not_Be_Added_To_HEAD_Request()
        {
            const Method HttpMethod = Method.HEAD;

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.Resource, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                var resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasNoRequestBody();
            }
        }

        [Fact]
        public void Can_Be_Added_To_OPTIONS_Request()
        {
            const Method HttpMethod = Method.OPTIONS;

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.Resource, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                var resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(ContentType, BodyData);
            }
        }

        [Fact]
        public void Can_Be_Added_To_PATCH_Request()
        {
            const Method HttpMethod = Method.PATCH;

            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.Resource, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                var resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(ContentType, BodyData);
            }
        }

        private static void AssertHasNoRequestBody()
        {
            Assert.Null(RequestBodyCapturer.CapturedContentType);
            Assert.Equal(false, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.Equal(string.Empty, RequestBodyCapturer.CapturedEntityBody);
        }

        private static void AssertHasRequestBody(string contentType, string bodyData)
        {
            Assert.Equal(contentType, RequestBodyCapturer.CapturedContentType);
            Assert.Equal(true, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.Equal(bodyData, RequestBodyCapturer.CapturedEntityBody);
        }

        private class RequestBodyCapturer
        {
            public const string Resource = "Capture";

            public static string CapturedContentType { get; set; }

            public static bool CapturedHasEntityBody { get; set; }

            public static string CapturedEntityBody { get; set; }

            public static void Capture(HttpListenerContext context)
            {
                var request = context.Request;

                CapturedContentType = request.ContentType;
                CapturedHasEntityBody = request.HasEntityBody;
                CapturedEntityBody = StreamToString(request.InputStream);
            }

            private static string StreamToString(Stream stream)
            {
                var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }
    }
}
