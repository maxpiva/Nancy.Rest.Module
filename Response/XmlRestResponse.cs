using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Xml;

namespace Nancy.Rest.Module.Serializers.Xml
{
    public class XmlRestResponse<T> : Response
    {
        public XmlRestResponse(T model, int level)
        {


            this.Contents = GetXmlContents(model, level);
            this.ContentType = DefaultContentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        private static string DefaultContentType => string.Concat("application/xml", Encoding);

        private static string Encoding => XmlSettings.EncodingEnabled
            ? string.Concat("; charset=", XmlSettings.DefaultEncoding.WebName)
            : string.Empty;

        private static Action<Stream> GetXmlContents(T model, int level)
        {
            return stream =>
            {

                serializer.Serialize(DefaultContentType, model, stream)
            };
        }
    }
}
