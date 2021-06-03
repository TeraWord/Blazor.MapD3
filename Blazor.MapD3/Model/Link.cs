namespace TeraWord.Blazor.MapD3
{
    public class Link
    {
        public int Source { get; set; }

        public int Target { get; set; }

        public bool Selected { get; set; } = false;

        public string Code { get; set; }
    }
}