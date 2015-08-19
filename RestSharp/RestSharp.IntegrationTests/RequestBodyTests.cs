using System.IO;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
    public class RequestBodyTests
    {
        private const string BaseUrl = "http://localhost:8888/";

        [Fact]
        public void Can_Not_Be_Added_To_GET_Request()
        {
            const Method httpMethod = Method.GET;
            using (SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                client.Execute(request);

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
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                client.Execute(request);

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
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                client.Execute(request);

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
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                client.Execute(request);

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
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                client.Execute(request);

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
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                client.Execute(request);

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
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, HttpMethod);

                const string ContentType = "text/plain";
                const string BodyData = "abc123 foo bar baz BING!";

                request.AddParameter(ContentType, BodyData, ParameterType.RequestBody);

                client.Execute(request);

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

      
    }
}
