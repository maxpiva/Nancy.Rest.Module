﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Nancy.Rest.Annotations;
using Nancy.Rest.Module.Filters;

namespace Nancy.Rest.Module
{
    internal static class Extensions
    {
        internal static List<T> GetCustomAttributesFromInterfaces<T>(this MethodInfo minfo) where T: Attribute
        {
            List<T> rests=new List<T>();
            List<Type> types = new List<Type> {minfo.DeclaringType};
            types.AddRange(minfo.DeclaringType?.GetInterfaces().ToList() ?? new List<Type>());
            foreach (Type t in types)
            {
                MethodInfo m = t.GetMethod(minfo.Name, minfo.GetParameters().Select(a => a.ParameterType).ToArray());
                if (m != null)
                    rests.AddRange(m.GetCustomAttributes(typeof(T)).Cast<T>().ToList());
            }
            return rests;
            
        }
        internal static List<T> GetCustomAttributesFromInterfaces<T>(this Type minfo) where T : Attribute
        {
            List<T> rests = new List<T>();
            List<Type> types = new List<Type> { minfo };
            types.AddRange(minfo.GetInterfaces());
            foreach (Type t in types)
            {
                rests.AddRange(t.GetCustomAttributes(typeof(T)).Cast<T>().ToList());
            }
            return rests;

        }


        internal static bool IsAsyncMethod(this MethodInfo minfo)
        {
            return (minfo.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null);
        }

        internal static bool IsNullable<T>(this T obj)
        {
            if (obj == null) return true;
            Type type = typeof(T);
            if (!type.IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true;
            return false;
        }

        internal static bool IsNullable(this Type type)
        {
            if (!type.IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true;
            return false;

        }

        internal static bool IsRouteable(this Type type)
        {
            if (type.IsValueType)
                return true;
            if (type == typeof(string) || type == typeof(Guid) || type==typeof(Guid?))
                return true;
            if (Nullable.GetUnderlyingType(type)?.IsValueType ?? false)
                return true;
            return false;
        }
        public static bool SerializerSupportFilter(this NancyModule module)
        {
            string contentType = module.Request.Headers.Accept?.ElementAt(0)?.Item1;
            if (contentType == null)
                return false;
            foreach (ISerializer serializer in module.Response.Serializers)
            {
                if (serializer.CanSerialize(contentType))
                {
                    if (serializer is IFilterSupport)
                        return true;
                    return false;
                }
            }
            return false;
        }

        public static NancyModule.RouteBuilder GetRouteBuilderForVerb(this NancyModule module, Verbs v)
        {
            NancyModule.RouteBuilder bld=null;
            switch (v)
            {
                case Verbs.Get:
                    bld = module.Get;
                    break;
                case Verbs.Post:
                    bld = module.Post;
                    break;
                case Verbs.Put:
                    bld = module.Put;
                    break;
                case Verbs.Delete:
                    bld = module.Delete;
                    break;
                case Verbs.Options:
                    bld = module.Options;
                    break;
                case Verbs.Patch:
                    bld = module.Patch;
                    break;
                case Verbs.Head:
                    bld = module.Head;
                    break;
            }
            return bld;
        } 
    }
}
