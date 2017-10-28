using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Nancy.Configuration;
using Nancy.Responses.Negotiation;
using Nancy.Rest.Annotations.Atributes;
using Nancy.Rest.Module.Filters.Serializers.Json;
using Nancy.Xml;


namespace Nancy.Rest.Module.Filters.Serializers.Xml
{

    //Based on https://github.com/NancyFx/Nancy/blob/v1.4.3/src/Nancy/Responses/DefaultXmlSerializer.cs
    public class XmlFilteredSerializer : ISerializer, IFilterSupport
    {
        private readonly XmlConfiguration configuration;
        private readonly TraceConfiguration traceConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultXmlSerializer"/> class,
        /// with the provided <see cref="INancyEnvironment"/>.
        /// </summary>
        /// <param name="environment">An <see cref="INancyEnvironment"/> instance.</param>
        public XmlFilteredSerializer(INancyEnvironment environment)
        {
            this.configuration = environment.GetValue<XmlConfiguration>();
            this.traceConfiguration = environment.GetValue<TraceConfiguration>();
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
            return content.IsXmlType();
        }


        /// <summary>
        /// Gets the list of extensions that the serializer can handle.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of extensions if any are available, otherwise an empty enumerable.</value>
        public IEnumerable<string> Extensions
        {
            get { yield return "xml"; }
        }


        /// <summary>
        /// Serialize the given model with the given contentType
        /// </summary>
        /// <param name="mediaRange">Content type to serialize into</param>
        /// <param name="model">Model to serialize</param>
        /// <param name="outputStream">Output stream to serialize to</param>
        /// <returns>Serialised object</returns>
        public void Serialize<TModel>(MediaRange mediaRange, TModel model, Stream outputStream)
        {
            if (model is FilterCarrier)
            {
                FilterCarrier carrier = (FilterCarrier) (object) model;
                HashSet<Type> types = new HashSet<Type>();
                GetTypesFromType(carrier.Object.GetType(), types);
                XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                if (carrier.Level != int.MaxValue || (carrier.ExcludeTags != null && carrier.ExcludeTags.Count > 0))
                {
                    XmlAttributes attribs = new XmlAttributes();
                    attribs.XmlIgnore = true;
                    foreach (Type t in types)
                    {
                        List<MemberInfo> members = t.GetProperties().Cast<MemberInfo>().ToList();
                        members.AddRange(t.GetFields().Cast<MemberInfo>());

                        foreach (MemberInfo m in members)
                        {
                            bool add = false;
                            Level lev = m.GetCustomAttribute<Level>();
                            if (lev?.Value > carrier.Level)
                                add = true;
                            else if (carrier.ExcludeTags != null && carrier.ExcludeTags.Count > 0)
                            {
                                Tags tags = m.GetCustomAttribute<Tags>();
                                if (tags?.Values != null && tags.Values.Count > 0)
                                {
                                    foreach (string s in tags.Values)
                                    {
                                        if (carrier.ExcludeTags.Contains(s))
                                        {
                                            add = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (add)
                            {
                                overrides.Add(t, m.Name, attribs);
                            }
                        }
                    }
                }
                var serializer = new XmlSerializer(carrier.Object.GetType(), overrides);

                if (configuration.EncodingEnabled)
                {
                    serializer.Serialize(new StreamWriter(outputStream, configuration.DefaultEncoding), carrier.Object);
                }
                else
                {
                    serializer.Serialize(outputStream, carrier.Object);
                }
            }
            else
            {
                var serializer = new XmlSerializer(typeof(TModel));

                if (configuration.EncodingEnabled)
                {
                    serializer.Serialize(new StreamWriter(outputStream, configuration.DefaultEncoding), model);
                }
                else
                {
                    serializer.Serialize(outputStream, model);
                }
            }
        }

        private static void GetTypesFromType(Type type, HashSet<Type> types)
        {
            types.Add(type);
            foreach (PropertyInfo p in type.GetProperties())
            {
                if (p.PropertyType.IsClass)
                {
                    if (!types.Contains(p.PropertyType))
                        GetTypesFromType(p.PropertyType, types);
                }
                else if (p.PropertyType.GetGenericArguments().Length > 0)
                {
                    foreach (Type n in p.PropertyType.GenericTypeArguments)
                    {
                        if (!types.Contains(n))
                            GetTypesFromType(n, types);
                    }
                }
            }
        }
    }
}

