using System.Collections.Generic;
using System.IO;
using Nancy.Configuration;
using Nancy.IO;
using Nancy.Json;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Nancy.Xml;
using Newtonsoft.Json;

namespace Nancy.Rest.Module.Filters.Serializers.Json
{
    //Based on https://github.com/NancyFx/Nancy.Serialization.JsonNet/tree/v2.0.0-clinteastwood/src/Nancy.Serialization.JsonNet

    public class JsonFilteredSerializer : ISerializer, IFilterSupport
    {
        private readonly JsonSerializer _serializer;
        private readonly JsonConfiguration jsonConfiguration;
        private readonly GlobalizationConfiguration globalizationConfiguration;




        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultXmlSerializer"/> class,
        /// with the provided <see cref="INancyEnvironment"/>.
        /// </summary>
        /// <param name="environment">An <see cref="INancyEnvironment"/> instance.</param>
        public JsonFilteredSerializer(INancyEnvironment environment)
        {
            this.jsonConfiguration = environment.GetValue<JsonConfiguration>();
            this.globalizationConfiguration = environment.GetValue<GlobalizationConfiguration>();
            _serializer = JsonSerializer.CreateDefault();
        }

        /// <summary>
        /// Whether the serializer can serialize the content type
        /// </summary>
        /// <param name="mediaRange">Content type to serialise</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanSerialize(MediaRange mediaRange)
        {
            if (string.IsNullOrEmpty(mediaRange))
                return false;
            var content = mediaRange.ToString().Split(';')[0];
            return content.IsJsonType();
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
        /// <param name="mediaRange">Content type to serialize into</param>
        /// <param name="model">Model to serialize</param>
        /// <param name="outputStream">Stream to serialize to</param>
        /// <returns>Serialised object</returns>
        public void Serialize<TModel>(MediaRange mediaRange, TModel model, Stream outputStream)
        {
            if (model is FilterCarrier)
            {
                FilterCarrier carrier = (FilterCarrier)(object)model;
                JsonSerializer nserializer = new JsonSerializer { ContractResolver = new FilteredContractResolver(carrier.Level, carrier.ExcludeTags), ReferenceLoopHandling = ReferenceLoopHandling.Serialize };
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
