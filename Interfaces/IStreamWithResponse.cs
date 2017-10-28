using System.Collections.Generic;

namespace Nancy.Rest.Module.Interfaces
{
    public interface IStreamWithResponse
    {
        string ContentType { get; }
        HttpStatusCode ResponseStatus { get; }
        string ResponseDescription { get; }
        Dictionary<string, string> Headers { get;  }
        long ContentLength { get; }
        bool HasContent { get; }
    }
}
