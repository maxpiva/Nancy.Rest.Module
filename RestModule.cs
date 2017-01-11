using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Force.DeepCloner;
using Nancy.ModelBinding;
using Nancy.Rest.Annotations;

namespace Nancy.Rest.Module
{
    public class RestModule : NancyModule
    {
        [ThreadStatic]
        public static RestModule CurrentModule;
        private static Dictionary<Type, TypeCache> cache = new Dictionary<Type, TypeCache>();

        public RestModule() : base("/")
        {
        }

        public RestModule(string modulepath) : base(modulepath)
        {
            
        }

        public void SetRestImplementation(object cls)
        {
            if (cache.ContainsKey(cls.GetType()))
            {
                TypeCache cached = cache[cls.GetType()];
                if (cached.ModulePath!=null)
                    ModulePath = cached.ModulePath;
                foreach (RouteCache c in cached.Items)
                {
                    RouteBuilder bld;
                    switch (c.Verb)
                    {
                        case Verbs.Get:
                            bld = Get;
                            break;
                        case Verbs.Post:
                            bld = Post;
                            break;
                        case Verbs.Put:
                            bld = Put;
                            break;
                        case Verbs.Delete:
                            bld = Delete;
                            break;
                        case Verbs.Options:
                            bld = Options;
                            break;
                        case Verbs.Patch:
                            bld = Patch;
                            break;
                        default:
                            bld = Head;
                            break;
                    }
                    if (c.IsAsync)
                    {
                        bld[c.Route, true] = c.AsyncFunc;
                    }
                    else
                    {
                        bld[c.Route] = c.SyncFunc;
                    }
                }
                return;
            }
            lock (cache)
            {
                TypeCache cached = new TypeCache();
                cached.Items = new List<RouteCache>();
                List<RestBasePath> paths = cls.GetType().GetCustomAttributesFromInterfaces<RestBasePath>().ToList();
                if (paths.Count > 0)
                    cached.ModulePath = ModulePath = paths[0].BasePath;
                cache.Add(cls.GetType(),cached);
                foreach (MethodInfo m in cls.GetType().GetMethods())
                {
                    List<Annotations.Rest> attributes = m.GetCustomAttributesFromInterfaces<Annotations.Rest>().ToList();
                    foreach (Annotations.Rest r in attributes)
                    {

                        RouteBuilder bld;
                        switch (r.Verb)
                        {
                            case Verbs.Get:
                                bld = Get;
                                break;
                            case Verbs.Post:
                                bld = Post;
                                break;
                            case Verbs.Put:
                                bld = Put;
                                break;
                            case Verbs.Delete:
                                bld = Delete;
                                break;
                            case Verbs.Options:
                                bld = Options;
                                break;
                            case Verbs.Patch:
                                bld = Patch;
                                break;
                            default:
                                bld = Head;
                                break;
                        }
                        RouteCache c;
                        if (m.IsAsyncMethod())
                        {
                            c = new RouteCache
                            {
                                Verb = r.Verb, Route=r.Route, IsAsync = true, MethodInfo = m, AsyncFunc = async (o, token) => await RouteAsync(cls, cached, m, o, r.ResponseContentType, token)
                            };                            
                            bld[r.Route, true] = c.AsyncFunc;
                        }
                        else
                        {
                            c = new RouteCache
                            {
                                Verb = r.Verb,
                                Route = r.Route,
                                IsAsync = false,
                                MethodInfo = m,
                                SyncFunc = o => Route(cls, cached, m, o, r.ResponseContentType)
                            };
                            bld[r.Route] = c.SyncFunc;
                        }
                        cached.Items.Add(c);

                    }
                }
            }
        }


        private dynamic Filter(dynamic ret, TypeCache tcache, string responsecontenttype)
        {
            if (ret is Stream)
            {
                if (ret is IStreamContentType)
                {
                    string ct = ((IStreamContentType)ret).ContentType;
                    if (!string.IsNullOrEmpty(ct))
                        responsecontenttype = ct;
                }
                if (responsecontenttype == null)
                    responsecontenttype = "application/octet-stream";
                return Response.FromStream((Stream)ret, responsecontenttype);
            }
            object n = ((object) ret).DeepClone();
            n.NullWithAttribute(typeof(Ignore));
            if (tcache.HasLevelInterface)
            {
                string val = Request.Query["level"];
                int level;
                if (string.IsNullOrEmpty(val) && int.TryParse(val, out level))
                {
                    n.NullWithLevelAttribute(level);
                }
            }
            return ret;
        }

        private async Task<object> RouteAsync(object cls, TypeCache tcache, MethodInfo m, dynamic d, string responsecontenttype, CancellationToken token)
        {
            CurrentModule = this;
            object[] pars = GetParametersFromDynamic(m, d,token);
            dynamic ret = await (dynamic) m.Invoke(cls, pars);
            return Filter(ret, tcache,responsecontenttype);
        }
        private object Route(object cls, TypeCache tcache, MethodInfo m, dynamic d, string responsecontenttype)
        {
            CurrentModule = this;
            object[] pars = GetParametersFromDynamic(m, d);
            dynamic ret = m.Invoke(cls, pars);
            return Filter(ret,tcache,responsecontenttype);
        }
        private object[] GetParametersFromDynamic(MethodInfo minfo, dynamic data, CancellationToken token=default(CancellationToken))
        {
            List<object> objs = new List<object>();
            List<ParameterInfo> pars = minfo.GetParameters().ToList();
            IDictionary<string, object> dict = (IDictionary<string, object>)data;
            foreach (ParameterInfo p in pars)
            {
                if (p.ParameterType == typeof(CancellationToken))
                {
                    objs.Add(token);
                    continue;
                }
                if (dict.ContainsKey(p.Name))
                {
                    dynamic obj = dict[p.Name];
                    if (obj.Value.GetType() == p.ParameterType)
                    {
                        objs.Add(obj.Value);
                        continue;
                    }
                    TypeConverter c = TypeDescriptor.GetConverter(p.ParameterType);
                    if (c.CanConvertFrom(obj.Value.GetType()))
                        objs.Add(c.ConvertFrom(obj.Value));
                    else
                        objs.Add(p.DefaultValue); //TODO SANITIZE OR ERROR CHECK
                }
                else 
                {
                    if (!p.ParameterType.IsValueType)
                    {
                        object n = Activator.CreateInstance(p.ParameterType);
                        this.BindTo(n);
                        objs.Add(n);
                    }
                    else
                        objs.Add(p.DefaultValue); //TODO SANITIZE OR ERROR CHECK
                }
            }
            return objs.ToArray();
        }
    }
}
