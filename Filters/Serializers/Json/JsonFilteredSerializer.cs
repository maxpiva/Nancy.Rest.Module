using System.Collections.Generic;
using System.IO;
using Nancy.IO;
using Newtonsoft.Json;

namespace Nancy.Rest.Module.Filters.Serializers.Json
{
    //Based on https://github.com/NancyFx/Nancy.Serialization.JsonNet/tree/v1.4.1/src/Nancy.Serialization.JsonNet

    public class JsonFilteredSerializer : ISerializer, IFilterSupport
    {
        private readonly JsonSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFilteredSerializer"/> class.
        /// </summary>
        public JsonFilteredSerializer()
        {
            _serializer = JsonSerializer.CreateDefault();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFilteredSerializer"/> class,
        /// with the provided <paramref name="serializer"/>.
        /// </summary>
        /// <param name="serializer">Json converters used when serializing.</param>
        public JsonFilteredSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Whether the serializer can serialize the content type
        /// </summary>
        /// <param name="contentType">Content type to serialise</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanSerialize(string contentType)
        {
            return contentType.IsJsonType();
        }

        /// <summary>
        /// Gets the list of extensions that the serializer can handle.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of extensions if any are available, otherwise an empty enumerable.</value>
        public IEnumerable<string> Extensions
        {
            get { yield return "json"; }
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
            if (model is FilterCarrier)
            {
                FilterCarrier carrier = (FilterCarrier)(object)model;
                JsonSerializer nserializer = new JsonSerializer { ContractResolver = new FilteredContractResolver(carrier.Level, carrier.ExcludeTags) };
                using (var writer = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream))))
                {
                    nserializer.Serialize(writer, carrier.Object);
                }
                return;
            }
            using (var writer = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream))))
            {
                _serializer.Serialize(writer, model);
            }
        }

    }
}
