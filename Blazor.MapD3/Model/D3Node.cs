using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TeraWord.Blazor.MapD3
{
    public sealed class D3Node
    {
        public int Index { get; internal set; }

        public string Code { get; set; }

        public string Label { get; set; }

        public List<string> Parents { get; set;  } = new();

        public string Group { get; set; }

        public string Tooltip { get; set; }

        public string Icon { get; set; }

        public string Color { get; set; }

        public string Header { get; set; }

        public string Footer { get; set; }

        public dynamic Data { get; set; }

        public double Width { get; set; } = 12;

        public double Height { get; set; } = 12;

        public double RoundX { get; set; } = 5;

        public double RoundY { get; set; } = 5; 

        public double IconX { get; set; } = 3;

        public double IconY { get; set; } = 0.75;
    }
}
