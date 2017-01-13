using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy.Rest.Annotations;
using YAXLib;

namespace Nancy.Rest.Module.Filters.Serializers.Xml
{
    internal class XmlFilteredSerializeProcessor : IAttributesPreprocessor
    {
        private int _level;
        private List<string> _excludeTags;
        public XmlFilteredSerializeProcessor(int level, List<string> excludetags)
        {
            _level = level;
            _excludeTags = excludetags;
        }
        public Attribute[] PreProcessAttributes(MemberInfo member)
        {
            List<Attribute> attributes = Attribute.GetCustomAttributes(member, true).ToList();
            bool shouldAddIgnore = false;
            if (attributes.Any(a=>a is Ignore))
                shouldAddIgnore=true;
            else
            {
                Level lev = attributes.FirstOrDefault(a => a is Level) as Level;
                if (lev?.Value > _level)
                    shouldAddIgnore=true;
            }
            if (_excludeTags != null && _excludeTags.Count > 0)
            {
                Tags tags = attributes.FirstOrDefault(a => a is Tags) as Tags;
                if (tags?.Values != null && tags.Values.Count > 0)
                {
                    foreach (string s in tags.Values)
                    {
                        if (_excludeTags.Contains(s))
                            shouldAddIgnore=true;
                    }
                }
            }
			if (shouldAddIgnore && !attributes.Any(a=>a is YAXDontSerializeAttribute))
				attributes.Add(new YAXDontSerializeAttribute());
            return attributes.ToArray();
        }
    }
}
