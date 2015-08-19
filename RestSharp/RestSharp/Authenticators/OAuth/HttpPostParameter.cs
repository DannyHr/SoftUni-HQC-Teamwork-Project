namespace RestSharp.Authenticators.OAuth
{
    using System.IO;

    /// <summary>
    /// 
    /// </summary>
    internal class HttpPostParameter : WebParameter
    {
        /// <summary>
        /// Initialize a new instance of the HttpPostParameter class
        /// </summary>
        /// <param name="name">The name of HttpPostParameter</param>
        /// <param name="value">The value of HttpPostParameter</param>
        public HttpPostParameter(string name, string value)
            : base(name, value)
        {
        }

        /// <summary>
        /// Gets the httpPostParameter Type 
        /// </summary>
        public virtual HttpPostParameterType Type { get; private set; }
     
        /// <summary>
        /// Gets the filename
        /// </summary>
        public virtual string FileName { get; private set; }

        /// <summary>
        /// Gets the filePath
        /// </summary>
        public virtual string FilePath { get; private set; }

        /// <summary>
        /// Gets the fileStream 
        /// </summary>
        public virtual Stream FileStream { get; set; }

        /// <summary>
        /// Gets the ContentType 
        /// </summary>
        public virtual string ContentType { get; private set; }

        /// <summary>
        /// Initialize a static HttpPostParameter class
        /// </summary>
        /// <param name="name">The name of HttpPostParameter</param>
        /// <param name="fileName">The fileName of HttpPostParameter</param>
        /// <param name="filePath">The filePath of HttpPostParameter</param>
        /// <param name="contentType">The contentType of HttpPostParameter</param>
        /// <returns>The contentType of HttpPostParameter object</returns>
        public static HttpPostParameter CreateFile(string name, string fileName, string filePath, string contentType)
        {
            var parameter = new HttpPostParameter(name, string.Empty)
            {
                Type = HttpPostParameterType.File,
                FileName = fileName,
                FilePath = filePath,
                ContentType = contentType,
            };
            return parameter;
        }

        /// <summary>
        /// Initialize a static HttpPostParameter class
        /// </summary>
        /// <param name="name">The name of HttpPostParameter</param>
        /// <param name="fileName">The fileName of HttpPostParameter</param>
        /// <param name="fileStream">The fileStream of HttpPostParameter</param>
        /// <param name="contentType">The contentType of HttpPostParameter</param>
        /// <returns>The contentType of HttpPostParameter object</returns>
        public static HttpPostParameter CreateFile(string name, string fileName, Stream fileStream, string contentType)
        {
            var parameter = new HttpPostParameter(name, string.Empty)
            {
                Type = HttpPostParameterType.File,
                FileName = fileName,
                FileStream = fileStream,
                ContentType = contentType,
            };

            return parameter;
        }
    }
}
