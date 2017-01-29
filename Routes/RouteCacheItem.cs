using System.Collections.Generic;
using System.Reflection;
using Nancy.Rest.Annotations.Enums;
using Nancy.Rest.Module.Helper;

namespace Nancy.Rest.Module.Routes
{
    internal class RouteCacheItem
    {
        public Verbs Verb { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public bool IsAsync { get; set; }
        public string Route { get; set; }
        public string ContentType { get; set; }   

        public List<ParamInfo> Parameters { get; set; }
    }
}
