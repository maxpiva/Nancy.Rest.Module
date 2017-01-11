using System.Collections.Generic;

namespace Nancy.Rest.Module
{
    internal class TypeCache
    {
        public string ModulePath { get; set; }
        public bool HasLevelInterface { get; set; }
        public List<RouteCache> Items { get; set; }
    }
}
