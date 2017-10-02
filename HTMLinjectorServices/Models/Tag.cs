using System.Collections.Generic;

namespace HTMLinjectorServices
{
    public class Tag
    {
        public string Name { get; set; }
        public string Contents { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public string SearchText { get; set; }
        public bool HasContent { get; set; }

        public TagPosition StartTagPosition { get; set; }
        public TagPosition EndTagPosition { get; set; }
    }
}
