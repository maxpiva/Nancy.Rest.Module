using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Nancy.Rest.Annotations;

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
                MethodInfo m = t.GetMethods().FirstOrDefault(a => a.Name == minfo.Name);
                if (m != null)
                {
                    rests.AddRange(m.GetCustomAttributes(typeof(T)).Cast<T>().ToList());
                }
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
                MethodInfo m = t.GetMethods().FirstOrDefault(a => a.Name == minfo.Name);
                if (m != null)
                {
                    rests.AddRange(m.GetCustomAttributes(typeof(T)).Cast<T>().ToList());
                }
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

        internal static void NullWithAttribute(this object obj, Type attrtype)
        {
            if (obj == null)
                return;
            Type objType = obj.GetType();
            foreach (PropertyInfo property in objType.GetProperties())
            {
                object propValue = property.GetValue(obj, null);
                IList list = propValue as IList;
                if (list != null)
                {
                    foreach (object item in list)
                    {
                        item.NullWithAttribute(attrtype);
                    }
                }
                else
                {
                    if (property.PropertyType.Assembly == objType.Assembly)
                    {
                        propValue.NullWithAttribute(attrtype);
                    }
                    else
                    {
                        if (Attribute.IsDefined(property, attrtype))
                        {
                            if (propValue.IsNullable())
                            {
                                property.SetValue(obj, null);
                            }
                        }
                    }
                }
            }
            foreach (FieldInfo finfo in objType.GetFields())
            {
                object propValue = finfo.GetValue(obj);
                IList list = propValue as IList;
                if (list != null)
                {
                    foreach (object item in list)
                    {
                        item.NullWithAttribute(attrtype);
                    }
                }
                else
                {
                    if (finfo.FieldType.Assembly == objType.Assembly)
                    {
                        propValue.NullWithAttribute(attrtype);
                    }
                    else
                    {
                        if (Attribute.IsDefined(finfo, attrtype))
                        {
                            if (propValue.IsNullable())
                            {
                                finfo.SetValue(obj, null);
                            }
                        }
                    }
                }
            }
        }
        internal static void NullWithLevelAttribute(this object obj, int level)
        {
            if (obj == null)
                return;
            Type objType = obj.GetType();
            foreach (PropertyInfo property in objType.GetProperties())
            {
                object propValue = property.GetValue(obj, null);
                IList list = propValue as IList;
                if (list != null)
                {
                    foreach (object item in list)
                    {
                        item.NullWithLevelAttribute(level);
                    }
                }
                else
                {
                    if (property.PropertyType.Assembly == objType.Assembly)
                    {
                        propValue.NullWithLevelAttribute(level);
                    }
                    else
                    {
                        if (Attribute.IsDefined(property, typeof(Level)))
                        {
                            if (propValue.IsNullable())
                            {
                                Level lv=property.GetCustomAttribute<Level>();
                                if (lv.Value>level)
                                    property.SetValue(obj, null);
                            }
                        }
                    }
                }
            }
            foreach (FieldInfo finfo in objType.GetFields())
            {
                object propValue = finfo.GetValue(obj);
                IList list = propValue as IList;
                if (list != null)
                {
                    foreach (object item in list)
                    {
                        item.NullWithLevelAttribute(level);
                    }
                }
                else
                {
                    if (finfo.FieldType.Assembly == objType.Assembly)
                    {
                        propValue.NullWithLevelAttribute(level);
                    }
                    else
                    {
                        if (Attribute.IsDefined(finfo, typeof(Level)))
                        {
                            if (propValue.IsNullable())
                            {
                                Level lv = finfo.GetCustomAttribute<Level>();
                                if (lv.Value > level)
                                    finfo.SetValue(obj, null);
                            }
                        }
                    }
                }
            }
        }
    }
}
