using System.Collections.Generic;
using System.Reflection;
using Nancy.Rest.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nancy.Rest.Module.Filters.Serializers.Json
{
    internal class FilteredContractResolver : DefaultContractResolver
    {
        private int _level;
        private List<string> _excludeTags;
        public FilteredContractResolver(int level, List<string> excludetags)
        {
            _level = level;
            _excludeTags = excludetags;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (member.GetCustomAttribute<Ignore>() != null)
            {
                property.ShouldSerialize = i => false;
                return property;
            }
            Level lev = member.GetCustomAttribute<Level>();
            if (lev?.Value > _level)
            {
                property.ShouldSerialize = i => false;
                return property;
            }
            if (_excludeTags != null && _excludeTags.Count > 0)
            {
                Tags tags = member.GetCustomAttribute<Tags>();
                if (tags?.Values != null && tags.Values.Count > 0)
                {
                    foreach (string s in tags.Values)
                    {
                        if (_excludeTags.Contains(s))
                        {
                            property.ShouldSerialize = i => false;
                            return property;
                        }
                    }
                }
            }
            property.ShouldSerialize = i => true;
            return property;
        }
    }
}
