using System.Collections.Generic;

namespace TeraWord.Blazor.MapD3
{
    public class D3Group
    {
        public int Index { get; internal set; }

        public string Code { get; set; }

        public List<int> Leaves { get; set; } = new();

        public List<int> Groups { get; set; } = new();

        internal List<string> _Groups { get; set; } = new();

        public string Color { get; set; } 
    }
}