using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nancy.ModelBinding;
using Nancy.Rest.Annotations.Atributes;
using Nancy.Rest.Annotations.Enums;

using Nancy.Rest.Module.Exceptions;
using Nancy.Rest.Module.Filters;
using Nancy.Rest.Module.Helper;
using Nancy.Rest.Module.Interfaces;
using Nancy.Rest.Module.Routes;

namespace Nancy.Rest.Module
{
    public class RestModule : NancyModule
    {

        [ThreadStatic]
        public static RestModule CurrentModule;


        private static Dictionary<Type, RouteCache> _cache = new Dictionary<Type, RouteCache>();

        public string DefaultLevelQueryParameterName { get; set; } = "level";
        public string DefaultExcludeTagsQueryParameterName { get; set; } = "excludetags";

        public static bool ShouldSerialize<T, TS>(Func<T, TS> a)
        {
            PropertyInfo p = typeof(T).GetProperty(typeof(TS).Name);
            Level l = p.GetCustomAttribute<Level>();
            Tags t = p.GetCustomAttribute<Tags>();
            if (l != null || t != null)
            {
                Tuple<int, List<string>> tup = CurrentModule.GetFilterData();
                if (l != null && l.Value > tup.Item1)
                    return false;
                if (t != null)
                {
                    foreach (string s in tup.Item2)
                    {
                        if (t.Values.Contains(s, StringComparer.InvariantCultureIgnoreCase))
                            return false;
                    }
                }
            }
            return true;
        }

        public RestModule() : base("/")
        {
        }

        public RestModule(string modulepath) : base(modulepath)
        {
            
        }
        internal void MapRoute(RouteCache cached, object cls, RouteCacheItem c)
        {
            if (c.IsAsync)
            {
                AddRoute(c.Verb.ToString().ToUpperInvariant(), c.Route, async (o, token) => await RouteAsync(cls, c, o, token), _ => true, c.MethodInfo.Name);
            }
            else
            {
                AddRoute<dynamic>(c.Verb.ToString().ToUpperInvariant(), c.Route, (o,token) => Task.FromResult(Route(cls, c, o)), _ => true, c.MethodInfo.Name);
            }
        }
        private static Regex rpath = new Regex("\\{(.*?)\\}", RegexOptions.Compiled);

        private Tuple<string, List<ParamInfo>> CheckMethodAssign(MethodInfo minfo, Annotations.Atributes.Rest attribute)
        {
            List<ParamInfo> parms=new List<ParamInfo>();
            MatchCollection collection = rpath.Matches(attribute.Route);
            foreach (Match m in collection)
            {
                if (m.Success)
                {
                    string value = m.Groups[1].Value;
                    bool optional = false;
                    string constraint = null;
                    int idx = value.LastIndexOf("?",StringComparison.InvariantCulture);
                    if (idx > 0)
                    {
                        value = value.Substring(0, idx);
                        optional = true;
                    }
                    idx = value.LastIndexOf(':');
                    if (idx >= 0)
                    {
                        constraint = value.Substring(idx + 1);
                        value = value.Substring(0, idx);
                        idx = constraint.LastIndexOf("(", StringComparison.InvariantCulture);
                        if (idx > 0)
                            constraint = constraint.Substring(0, idx);
                    }
                    ParamInfo info = new ParamInfo();
                    info.Name = value;
                    info.Optional = optional;
                    if (constraint != null)
                    {
                        ParameterType ptype = ParameterType.InstanceTypes.FirstOrDefault(a => a.Name == constraint);
                        if (ptype == null)
                            return new Tuple<string, List<ParamInfo>>("Method with Name: '" + minfo.Name + "' and Route: '" + attribute.Route + "' has an unknown constraint '" + constraint + "'",null);
                        info.Constraint = ptype;
                    }
                    parms.Add(info);
                }
            }
            List<ParameterInfo> infos = minfo.GetParameters().ToList();
            foreach (ParamInfo p in parms)
            {
                ParameterInfo pinf = infos.FirstOrDefault(a => a.Name == p.Name);
                if (pinf == null)
                    return new Tuple<string, List<ParamInfo>>("Method with Name: '" + minfo.Name + "' and Route: '" + attribute.Route + "' has an unknown variable in the route path '" + p.Name + "'", null);
                if (p.Optional && !pinf.ParameterType.IsNullable())
                    return new Tuple<string, List<ParamInfo>>("Method with Name: '" + minfo.Name + "' and Route: '" + attribute.Route + "' with variable '"+p.Name+"' is marked in the route path as nullable, but the method variable is not", null);
                if (p.Constraint != null && !p.Constraint.Types.Contains(pinf.ParameterType))
                    return new Tuple<string, List<ParamInfo>>("Method with Name: '" + minfo.Name + "' and Route: '" + attribute.Route + "' with variable '" + p.Name + "' is constrained to type " + p.Constraint.BaseType + " but the method variable is not of the same type", null);
                if (!pinf.ParameterType.IsRouteable())
                    return new Tuple<string, List<ParamInfo>>("Method with Name: '" + minfo.Name + "' and Route: '" + attribute.Route + "' the variable '" + p.Name + "' is not a value type and is the route path", null);
                infos.Remove(pinf);
            }
            if (infos.Count > 0 && attribute.Verb == Verbs.Get)
            {
                return new Tuple<string, List<ParamInfo>>("Method with Name: '" + minfo.Name + "' and Route: '" + attribute.Route + "' has post variables in a GET operation", null);
            }
            if (infos.Count > 1)
            {
                return new Tuple<string, List<ParamInfo>>("Method with Name: '" + minfo.Name + "' and Route: '" + attribute.Route + "' has more than one Body object", null);
            }
            return new Tuple<string, List<ParamInfo>>(null, parms);
        }
        public void SetRestImplementation(object cls)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(cls.GetType()))
                {
                    RouteCache tc2 = _cache[cls.GetType()];
                    if (tc2.ModulePath != null)
                        ModulePath = tc2.ModulePath;
                    tc2.Items.ForEach(a => MapRoute(tc2, cls, a));
                    return;
                }
                RouteCache tc = new RouteCache();
                tc.Items = new List<RouteCacheItem>();
                List<RestBasePath> paths = cls.GetType().GetCustomAttributesFromInterfaces<RestBasePath>().ToList();
                if (paths.Count > 0)
                    tc.ModulePath = ModulePath = paths[0].BasePath;
                _cache.Add(cls.GetType(),tc);
                List<string> errors = new List<string>();
                foreach (MethodInfo m in cls.GetType().GetMethods())
                {
                    Annotations.Atributes.Rest r = m.GetCustomAttributesFromInterfaces<Annotations.Atributes.Rest>().FirstOrDefault();
                    if (r == null)
                        continue;
//                    if (r.Verb == Verbs.Head && !StaticConfiguration.EnableHeadRouting)
//                        StaticConfiguration.EnableHeadRouting = true;
                    Type[] types = m.GetParameters().Select(a => a.ParameterType).ToArray();
                    MethodInfo method = cls.GetType().GetInterfaces().FirstOrDefault(a => a.GetMethod(m.Name, types) != null)?.GetMethod(m.Name, types);
                    if (method == null)
                        method = m;
                    Tuple<string,List<ParamInfo>> result = CheckMethodAssign(method, r);
                    if (result.Item1!=null)
                        errors.Add(result.Item1);
                    RouteCacheItem c = new RouteCacheItem
                    {
                        Verb = r.Verb,
                        Route = r.Route,
                        IsAsync = method.IsAsyncMethod(),
                        MethodInfo = method,
                        ContentType = r.ResponseContentType,
                        Parameters = result.Item2
                    };
                    tc.Items.Add(c);
                    MapRoute(tc, cls, c);
                }
                if (errors.Count > 0)
                {
                    StringBuilder bld=new StringBuilder();
                    bld.AppendLine("Unable to mount service '"+cls.GetType().Name+"'");
                    errors.ForEach(a=>bld.AppendLine(a));
                    _cache.Clear();
                    throw new NancyRestModuleException(bld.ToString());
                }
            }
        }

        private Tuple<int, List<string>> GetFilterData()
        {
            int level = int.MaxValue;
            List<string> tags = null;
            string val = Request.Query[DefaultLevelQueryParameterName];
            if (!string.IsNullOrEmpty(val))
            {
                int.TryParse(val, out level);
            }
            val = Request.Query[DefaultExcludeTagsQueryParameterName];
            if (!string.IsNullOrEmpty(val))
            {
                string[] tg = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (tg.Length > 0)
                {
                    tags = tg.ToList();
                }
            }
            return new Tuple<int, List<string>>(level, tags);
        }

        private dynamic Filter(dynamic ret, string responsecontenttype)
        {
            if (ret is Response)
                return ret;
            if (ret is HttpStatusCode)
                return ret;
            if (ret is Stream)
            {
                if (ret is IStreamWithResponse)
                    return Response.FromIStreamWithResponse((IStreamWithResponse) ret, responsecontenttype);
                if (string.IsNullOrEmpty(responsecontenttype))
                    responsecontenttype = "application/octet-stream";
                return Response.FromStream((Stream)ret, responsecontenttype);
            }
            if (this.SerializerSupportFilter())
            {
                Tuple<int, List<string>> tup=GetFilterData();
                FilterCarrier carrier=new FilterCarrier();
                carrier.Level = tup.Item1;
                carrier.ExcludeTags = tup.Item2;
                carrier.Object = (object) ret;
                return carrier;
            }
            return ret;
        }

        private async Task<object> RouteAsync(object cls, RouteCacheItem ci, dynamic d, CancellationToken token)
        {
            CurrentModule = this;
            object[] pars = GetParametersFromDynamic(ci, ci.MethodInfo, d,token);
            dynamic ret = await (dynamic) ci.MethodInfo.Invoke(cls, pars);
            return Filter(ret, ci.ContentType);
        }
        private object Route(object cls, RouteCacheItem ci, dynamic d)
        {
            CurrentModule = this;
            object[] pars = GetParametersFromDynamic(ci, ci.MethodInfo, d);
            dynamic ret = ci.MethodInfo.Invoke(cls, pars);
            return Filter(ret, ci.ContentType);
        }
        private static Regex datetimeplusfix=new Regex("(.*?)-(.*?)-(.*?)T(.*?):(.*?):(.*?)\\s(.*?)");

        private void AddObject(object ob, ParameterInfo p, List<object> objs)
        {
            dynamic obj = ob;
            if (obj.Value.GetType() == p.ParameterType)
            {
                objs.Add(obj.Value);
                return;
            }
            if (p.ParameterType == typeof(DateTime))
            {
                string s = (string)obj;
                Match m = datetimeplusfix.Match(s);
                if (m.Success)
                {
                    int a = s.LastIndexOf(" ");
                    s = s.Substring(0, a) + "+" + s.Substring(a + 1);
                }
                //TODO add better handling and normalized DateTime Type
                try
                {

                    objs.Add(DateTime.ParseExact(s, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
                }
                catch
                {
                    objs.Add(DateTime.ParseExact(s, "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal));
                }
                return;
            }
            Type ftype = Nullable.GetUnderlyingType(p.ParameterType);
            //Handle empty nullables
            if (ftype != null)
            {
                if (obj.Value == null)
                {
                    objs.Add(null);
                    return;
                }
                if (obj.Value is string)
                {
                    if (string.IsNullOrEmpty(obj.Value))
                    {
                        objs.Add(null);
                        return;
                    }
                }
            }

            TypeConverter c = TypeDescriptor.GetConverter(p.ParameterType);
            if (c.CanConvertFrom(obj.Value.GetType()))
                objs.Add(c.ConvertFrom(null,CultureInfo.InvariantCulture, obj.Value));
            else
            {
                if (p.DefaultValue != null && p.DefaultValue != DBNull.Value)
                    objs.Add(p.DefaultValue); //TODO SANITIZE OR ERROR CHECK
                else
                    objs.Add(p.ParameterType == typeof(string) ? string.Empty : GetDefault(p.ParameterType));
            }
        }

        private object[] GetParametersFromDynamic(RouteCacheItem ci, MethodInfo minfo, dynamic data, CancellationToken token=default(CancellationToken))
        {
            List<object> objs = new List<object>();
            List<ParameterInfo> pars = minfo.GetParameters().ToList();
            IDictionary<string, object> dict = (IDictionary<string, object>)data;
            IDictionary<string, object> formdict = null;
            if (this.Request.Form != null)
            {
                formdict= (IDictionary<string, object>)this.Request.Form;
            }
            foreach (ParameterInfo p in pars)
            {
                if (p.ParameterType == typeof(CancellationToken))
                {
                    objs.Add(token);
                    continue;
                }
                if (ci.Parameters.Any(a => a.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (dict.ContainsKey(p.Name))
                    {
                        AddObject(dict[p.Name],p,objs);
                    }
                    else
                    {
                        Type ftype = Nullable.GetUnderlyingType(p.ParameterType);
                        if (ftype != null)
                            objs.Add(null);
                    }
                }
                else if (formdict!=null && formdict.ContainsKey(p.Name))
                {
                    AddObject(formdict[p.Name], p, objs);
                }
                else
                {
                    objs.Add(p.ParameterType == typeof(Stream) ? Request.Body : Bind(p.ParameterType));
                }
            }
            return objs.ToArray();
        }
        public object GetDefault(Type t)
        {
            return GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
        }
        public object Bind(Type t)
        {
            if (t.IsValueType || t.IsPrimitive)
            {
                throw new ArgumentException("The current system fails on Value and primitive types due to lack of valid constructor.");
            }

            MethodInfo method = typeof(Nancy.ModelBinding.ModuleExtensions).GetMethods()
                .First(a => a.Name == "Bind" && a.ContainsGenericParameters && a.GetParameters().Length == 1);
            return method.MakeGenericMethod(t).Invoke(null, new object[] {this});
        }

        public T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }
}