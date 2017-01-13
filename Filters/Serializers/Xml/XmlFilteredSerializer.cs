using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nancy.IO;
using Nancy.Rest.Module.Filters.Serializers.Json;
using Nancy.Xml;
using YAXLib;

namespace Nancy.Rest.Module.Filters.Serializers.Xml
{

    //Based on https://github.com/NancyFx/Nancy/blob/v1.4.3/src/Nancy/Responses/DefaultXmlSerializer.cs
    public class XmlFilteredSerializer : ISerializer, IFilterSupport
    {
        /// <summary>
        /// Whether the serializer can serialize the content type
        /// </summary>
        /// <param name="contentType">Content type to serialise</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanSerialize(string contentType)
        {
            return contentType.IsXmlType();
        }

        /// <summary>
        /// Gets the list of extensions that the serializer can handle.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of extensions if any are available, otherwise an empty enumerable.</value>
        public IEnumerable<string> Extensions
        {
            get
            {
                yield return "xml";
            }
        }

        /// <summary>
        /// Serialize the given model with the given contentType
        /// </summary>
        /// <param name="contentType">Content type to serialize into</param>
        /// <param name="model">Model to serialize</param>
        /// <param name="outputStream">Output stream to serialize to</param>
        /// <returns>Serialised object</returns>
        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            try
            {

                TextWriter writer=null;
                try
                {
                    if (XmlSettings.EncodingEnabled)
                        writer = new StreamWriter(new UnclosableStreamWrapper(outputStream), XmlSettings.DefaultEncoding);
                    else
                        writer = new StreamWriter(new UnclosableStreamWrapper(outputStream));

                    if (model is FilterCarrier)
                    {
                        FilterCarrier carrier = (FilterCarrier) (object) model;
                        YAXSerializer ser = new YAXSerializer(carrier.Object.GetType());
                        ser.AttributesPreprocessor = new XmlFilteredSerializeProcessor(carrier.Level,carrier.ExcludeTags);
                        ser.Serialize(carrier.Object, writer);
                    }
                    else
                    {
                        YAXSerializer ser = new YAXSerializer(model.GetType());
                        ser.Serialize(model);
                    }
                }
                finally
                {
                    writer?.Dispose();
                }
            }
            catch (Exception exception)
            {
                if (!StaticConfiguration.DisableErrorTraces)
                {
                    var bytes = Encoding.UTF8.GetBytes(exception.Message);
                    outputStream.Write(bytes, 0, exception.Message.Length);
                }
            }
        }

    }
}
