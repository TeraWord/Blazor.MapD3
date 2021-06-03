using System.Collections.Generic;

namespace TeraWord.Blazor.MapD3
{
    public sealed class Node
    {
        public string Code { get; set; }

        public string Label { get; set; }

        public List<string> Parents { get; set; } = new();

        public string Group { get; set; }

        public string Tooltip { get; set; }

        public string Color { get; set; }

        public string Header { get; set; }

        public string Footer { get; set; }

        public dynamic Data { get; set; }
    }
}
