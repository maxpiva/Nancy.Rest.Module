using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Rest.Annotations;

namespace Nancy.Rest.Module
{
    internal class RouteCache
    {
        public Verbs Verb { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public bool IsAsync { get; set; }
        public string Route { get; set; }
        public Func<dynamic, dynamic> SyncFunc { get; set; }
        public Func<dynamic, CancellationToken, Task<dynamic>> AsyncFunc { get; set; }
    }
}
