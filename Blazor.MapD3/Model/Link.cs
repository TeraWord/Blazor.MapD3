namespace TeraWord.Blazor.MapD3
{
    public class Link
    {
        public string Code { get; set; }

        public int Source { get; set; }

        public int Target { get; set; }

        public bool Selected { get; set; } = false;
    }
}