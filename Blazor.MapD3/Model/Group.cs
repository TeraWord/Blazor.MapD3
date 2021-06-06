using System.Collections.Generic;

namespace TeraWord.Blazor.MapD3
{
    public class Group
    {
        public string Code { get; set; }

        public List<int> Leaves { get; set; } = new List<int>();

        public List<int> Groups { get; set; } = new List<int>();

        public string Color { get; set; } 
    }
}