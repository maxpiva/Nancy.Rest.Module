using System.Reflection;
using Nancy.Rest.Annotations;

namespace Nancy.Rest.Module.Routes
{
    internal class RouteCacheItem
    {
        public Verbs Verb { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public bool IsAsync { get; set; }
        public string Route { get; set; }
        public string ContentType { get; set; }   
    }
}
