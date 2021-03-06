﻿using System.IO;
using System.Xml.Serialization;
using Nancy.ModelBinding;
using Nancy.Rest.Module.Filters.Serializers.Json;

namespace Nancy.Rest.Module.Filters.Serializers.Xml
{

    //Based on https://github.com/NancyFx/Nancy/blob/v1.4.3/src/Nancy/ModelBinding/DefaultBodyDeserializers/XmlBodyDeserializer.cs
    public class XmlFilteredBodyDeserializer : IBodyDeserializer
    {
        /// <summary>
        /// Whether the deserializer can deserialize the content type
        /// </summary>
        /// <param name="contentType">Content type to deserialize</param>
        /// <param name="context">Current <see cref="BindingContext"/>.</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanDeserialize(string contentType, BindingContext context)
        {
            return contentType.IsXmlType();
        }

        /// <summary>
        /// Deserialize the request body to a model
        /// </summary>
        /// <param name="contentType">Content type to deserialize</param>
        /// <param name="bodyStream">Request body stream</param>
        /// <param name="context">Current <see cref="BindingContext"/>.</param>
        /// <returns>Model instance</returns>
        public object Deserialize(string contentType, Stream bodyStream, BindingContext context)
        {
            bodyStream.Position = 0;
            var ser = new XmlSerializer(context.DestinationType);
            return ser.Deserialize(bodyStream);
        }
    }
}
