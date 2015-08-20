namespace RestSharp
{
    /// <summary>
    /// Interface for creating new request
    /// </summary>
    public interface IHttpFactory
    {
        IHttp Create();
    }

    /// <summary>
    /// Factory for creating new request
    /// </summary>
    /// <typeparam name="T">type of request</typeparam>
    public class SimpleFactory<T> : IHttpFactory 
        where T : IHttp, new()
    {
        public IHttp Create()
        {
            return new T();
        }
    }
}
