using System.Collections.Generic;

namespace Nancy.Rest.Module.Routes
{
    internal class RouteCache
    {
        public string ModulePath { get; set; }
        public bool HasFilterInterface { get; set; }
        public List<RouteCacheItem> Items { get; set; }
    }
}
