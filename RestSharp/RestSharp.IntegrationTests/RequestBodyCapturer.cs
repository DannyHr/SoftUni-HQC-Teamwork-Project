using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.IntegrationTests
{
    using System.IO;
    using System.Net;

    internal class RequestBodyCapturer
    {
        public const string RESOURCE = "Capture";

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
