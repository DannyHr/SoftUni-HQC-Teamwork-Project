using System;
using System.Net;
using RestSharp.Deserializers;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
    public class StructuredSyntaxSuffixTests
    {
     

        private const string XmlContent = "<Person><name>Bob</name><age>50</age></Person>";
        private const string JsonContent = @"{ ""name"":""Bob"", ""age"":50 }";


        [Fact]
        public void By_default_content_types_with_JSON_structured_syntax_suffix_should_deserialize_as_JSON()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                var client = new RestClient(baseUrl);
                
                var request = new RestRequest();
                request.AddParameter("ct", "application/vnd.somebody.something+json");
                request.AddParameter("c", JsonContent);

                var response = client.Execute<Person>(request);

                Assert.Equal("Bob", response.Data.Name);
                Assert.Equal(50, response.Data.Age);
            }
        }

        [Fact]
        public void By_default_content_types_with_XML_structured_syntax_suffix_should_deserialize_as_XML()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                var client = new RestClient(baseUrl);

                var request = new RestRequest();
                request.AddParameter("ct", "application/vnd.somebody.something+xml");
                request.AddParameter("c", XmlContent);

                var response = client.Execute<Person>(request);

                Assert.Equal("Bob", response.Data.Name);
                Assert.Equal(50, response.Data.Age);
            }
        }

        [Fact]
        //Renamed method 
        public void Content_Type_That_Matches_The_tructured_Syntax_Suffix_Format_Should_Use_Supplied_Deserializer()
       // Content_type_that_matches_the_structured_syntax_suffix_format_but_was_given_an_explicit_handler_should_use_supplie//d_deserializer()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, this.QueryStringBasedContentAndContentTypeHandler))
            {
                var client = new RestClient(baseUrl);

                // In spite of the content type (+xml), treat this specific content type as JSON
                client.AddHandler("application/vnd.somebody.something+xml", new JsonDeserializer());

                var request = new RestRequest();
                request.AddParameter("ct", "application/vnd.somebody.something+xml");
                request.AddParameter("c", JsonContent);

                var response = client.Execute<Person>(request);

                Assert.Equal("Bob", response.Data.Name);
                Assert.Equal(50, response.Data.Age);
            }
        }

        [Fact]
        public void Should_Allow_Wildcard_Content_Types_To_Be_Defined()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                var client = new RestClient(baseUrl);

                // In spite of the content type, handle ALL structured syntax suffixes of "+xml" as JSON
                client.AddHandler("*+xml", new JsonDeserializer());

                var request = new RestRequest();
                request.AddParameter("ct", "application/vnd.somebody.something+xml");
                request.AddParameter("c", JsonContent);

                var response = client.Execute<Person>(request);

                Assert.Equal("Bob", response.Data.Name);
                Assert.Equal(50, response.Data.Age);
            }
        }

        [Fact]
        public void By_default_application_json_content_type_should_deserialize_as_JSON()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                var client = new RestClient(baseUrl);

                var request = new RestRequest();
                request.AddParameter("ct", "application/json");
                request.AddParameter("c", JsonContent);

                var response = client.Execute<Person>(request);

                Assert.Equal("Bob", response.Data.Name);
                Assert.Equal(50, response.Data.Age);
            }
        }

        [Fact]
        public void By_default_text_xml_content_type_should_deserialize_as_XML()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                var client = new RestClient(baseUrl);

                var request = new RestRequest();
                request.AddParameter("ct", "text/xml");
                request.AddParameter("c", XmlContent);

                var response = client.Execute<Person>(request);

                Assert.Equal("Bob", response.Data.Name);
                Assert.Equal(50, response.Data.Age);
            }
        }

        private void QueryStringBasedContentAndContentTypeHandler(HttpListenerContext obj)
        {
            obj.Response.ContentType = obj.Request.QueryString["ct"];
            obj.Response.OutputStream.WriteStringUtf8(obj.Request.QueryString["c"]);
            obj.Response.StatusCode = 200;
        }

        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
