using System.Collections.Generic;

namespace Nancy.Rest.Module.Filters
{
    public class FilterCarrier
    {
        public int Level { get; set; }
        public List<string> ExcludeTags { get; set; }
        public object Object { get; set; }
    }

}
