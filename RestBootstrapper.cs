using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Rest.Module.Filters.Serializers.Json;
using Nancy.Rest.Module.Filters.Serializers.Xml;

namespace Nancy.Rest.Module
{
    public class RestBootstrapper : DefaultNancyBootstrapper
    {

        //We Need to skip this assembly from the available modules.
        private ModuleRegistration[] _modules;
        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return 
                    _modules
                    ??
                    (_modules = AppDomainAssemblyTypeScanner
                                        .TypesOf<INancyModule>(ScanMode.ExcludeNancy)
                                        .NotOfType<DiagnosticModule>()
                                        .Where(a=>a.Name!=typeof(RestModule).Name).Select(t => new ModuleRegistration(t))
                                        .ToArray());
            }
        }
        private IEnumerable<Type> _bodySerializers;
        protected override IEnumerable<Type> BodyDeserializers
        {
            get
            {
                if (_bodySerializers == null)
                {
                    List<Type> serializers = AppDomainAssemblyTypeScanner.TypesOf<IBodyDeserializer>(ScanMode.ExcludeNancy).ToList();
                    RemoveProcessors(serializers).ForEach(a => serializers.Remove(a)); 
                    serializers.Add(typeof(JsonFilteredBodyDeserializer));
                    serializers.Add(typeof(XmlFilteredBodyDeserializer));
                    _bodySerializers = serializers;
                }
                return _bodySerializers;
            } 
        }
        //TODO find a dynamic way of remove originals, instead of asking for ContentType, so it can be extensible to other protocols.
        //Remove json and xml serializer, add our filter-able serializers.
        private NancyInternalConfiguration _configuration;
        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = base.InternalConfiguration ?? NancyInternalConfiguration.Default;
                    RemoveProcessors(_configuration.Serializers).ForEach(a=> _configuration.Serializers.Remove(a));
                    _configuration.Serializers.Add(typeof(JsonFilteredSerializer));
                    _configuration.Serializers.Add(typeof(XmlFilteredSerializer));
                    _configuration.ResponseProcessors.Remove(typeof(JsonProcessor));
                    _configuration.ResponseProcessors.Remove(typeof(XmlProcessor));
                    _configuration.ResponseProcessors.Add(typeof(JsonProcessor));
                    _configuration.ResponseProcessors.Add(typeof(XmlProcessor));
                }
                return _configuration;
            }
        }

        //TODO find a dynamic way of remove originals, instead of asking for ContentType, so it can be extensible to other protocols.
        private List<Type> RemoveProcessors(IEnumerable<Type> types)
        {
            List<Type> toremove=new List<Type>();
            foreach (Type t in types)
            {
                object o = Activator.CreateInstance(t);
                ISerializer serializer = o as ISerializer;
                if (serializer != null)
                {
                    if (serializer.CanSerialize("application/json") || serializer.CanSerialize("text/xml"))
                        toremove.Add(t);
                }
                IBodyDeserializer deserializer = o as IBodyDeserializer;
                if (deserializer != null)
                {
                    if (deserializer.CanDeserialize("application/json",null) || deserializer.CanDeserialize("text/xml",null))
                        toremove.Add(t);
                }
            }
            return toremove;
        }

    }
}
