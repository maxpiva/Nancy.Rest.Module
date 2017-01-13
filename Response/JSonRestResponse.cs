using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.IO;
using Nancy.Json;
using Newtonsoft.Json;

namespace Nancy.Rest.Module.Serializers.JSon
{
    public class JsonRestResponse<T> : Response
    {
        public JsonRestResponse(T model, int level)
        {
            this.Contents = model == null ? NoBody : GetJsonContents(model, level);
            this.ContentType = DefaultContentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        private static string DefaultContentType => string.Concat("application/json", Encoding);

        private static string Encoding => string.Concat("; charset=", JsonSettings.DefaultEncoding.WebName);

        private static Action<Stream> GetJsonContents(T model, int level)
        {
            return stream =>
            {
                JsonSerializer serializer = new JsonSerializer {ContractResolver = new FilteredContractResolver(level)};
                using (var writer = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(stream))))
                {
                    serializer.Serialize(writer, model);
                }
            };
        }
    }
    public class JsonRestResponse : JsonRestResponse<object>
    {
        public JsonRestResponse(object model, int level) : base(model, level)
        {
        }
    }
}
