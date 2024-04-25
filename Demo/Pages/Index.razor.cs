using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TeraWord.Blazor.MapD3;

namespace Demo.Pages
{
    public partial class Index
    {
        private TeraWord.Blazor.MapD3.Data Data { get; set; }

        private Guid root = new Guid("{646DBB8D-B1D2-43F2-BD9C-4FE3E27BD0BA}");
        private Guid parent;
        private Guid child;
        private Guid groupA = new Guid("{E58A93B4-0016-479E-AE83-FCE8415B2BE5}");

        private string NodeJson { get; set; }

        private int LinkDistance { get; set; } = 50;

        private int LinkLengths { get; set; } = 20;

        private MapD3 MapD3 { get; set; }

        private Random rnd = new();
                 
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender) Init();
        }

        private void Init()
        {
            Data = new TeraWord.Blazor.MapD3.Data();

            var items = (new[] {
                new {
                    Code = root,
                    Label = "Root",
                    Parent = Guid.Empty,
                    Description = "Descrizione",
                    Status = 0,
                    Header = "header",
                    Footer = "footer"
                }
            }).ToList();

            parent = Guid.NewGuid();

            items.Add(new
            {
                Code = parent,
                Label = "Parent",
                Parent = root,
                Description = "Descrizione",
                Status = 1,
                Header = (string)null,
                Footer = (string)null
            });

            child = Guid.NewGuid();

            items.Add(new
            {
                Code = child,
                Label = "Child",
                Parent = parent,
                Description = "Descrizione",
                Status = 2,
                Header = (string)null,
                Footer = (string)null
            });

            Node node;

            foreach (var item in items)
            {
                node = Data.AddNode(item.Code.ToString(), item.Parent.ToString());
                node.Label = item.Label;
                node.Tooltip = item.Label + " - " + item.Description;
                node.Color = item.Status switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };
                node.Header = item.Header;
                node.Footer = item.Footer;

                //node.Data = new
                //{
                //    Alfa = 5,
                //    Beta = 2
                //};
            }

            //var group = data.AddGroup($"{groupA}");
            //group.Color = "#FF5555";

            //node = data.AddNode($"{Guid.NewGuid()}", $"{parent}");
            //node.Label = "Inside";
            //node.Group = $"{groupA}";
            //node.Color = "orange";

            //node = data.AddNode($"{Guid.NewGuid()}", $"{parent}");
            //node.Label = "Inside";
            //node.Group = $"{groupA}";
            //node.Color = "orange";

            MapD3.Data = Data;
        }

        private async Task OnNodeClick(Node node)
        {
            var json = JsonSerializer.Serialize(node, new JsonSerializerOptions { WriteIndented = true });
            NodeJson = json;
            await Task.CompletedTask;
        }

        private async Task OnRootClick(dynamic e)
        {
            var data = new TeraWord.Blazor.MapD3.Data();
            var node = data.AddNode($"{root}");

            node.Label = "Root";
            node.Tooltip = node.Label + " - " + "Descrizione";
            node.Color = 0 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };
            node.Icon = "archive";

            MapD3.Data = data;

            await MapD3.Update();
        }

        private async Task OnParentClick(dynamic e)
        {
            child = Guid.NewGuid();

            parent = Data.ExistsNode(parent.ToString()) ? parent : root;

            var node = Data.AddNode($"{child}", $"{parent}");

            node.Label = "Parent";
            node.Tooltip = node.Label + " - " + "Descrizione";
            node.Color = 1 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };
            node.Icon = "handshake";

            parent = child;

            MapD3.Data = Data;

            await MapD3.Update();
        }

        private string RndIcon
        {
            get
            {
                var icons = new string[] { "coffee", "cog", "cogs", "comment", "comments", "copyright", "credit-card", "crop", "crosshairs", "cube", "cubes", "database", "deaf", "desktop" };

                return icons[rnd.Next(icons.Length)];
            }
        }

        private async Task OnChildClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.AddNode($"{child}", $"{parent}");
            var icon = RndIcon;

            //node.Label = "Child";
            node.Tooltip = $"Icon: {icon}";
            node.Color = 2 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };
            node.Icon = RndIcon;
            node.Width = 16;
            node.Height = 16;
            node.RoundX = 16;
            node.RoundY = 16;
            node.IconX = 4;
            node.IconY = 2;

            MapD3.Data = Data;
            await MapD3.Update();
        }

        private async Task OnLinkClick(dynamic e)
        {
            Data.AddLink($"{child}", $"{root}");

            MapD3.Data = Data;

            await MapD3.Update();
        }

        private async Task OnLonelyClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.AddNode($"{child}", null);

            node.Label = "Lonely";
            node.Tooltip = node.Label + " - " + "Lonely";
            node.Color = 2 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            MapD3.Data = Data;

            await MapD3.Update();
        }

        private async Task OnRemoveClick(dynamic e)
        {
            var node = Data.Nodes.FirstOrDefault(x => Guid.Parse(x.Code).Equals(child));

            if (node is not null)
            {
                Data.Nodes.Remove(node);
                child = Guid.Parse(Data.Nodes.LastOrDefault()?.Code ?? Guid.Empty.ToString());
               
                MapD3.Data = Data;

                await MapD3.Update();
            }
        }

        private async Task OnZoomToFitClick(dynamic e)
        {
            await MapD3.ZoomToFit();
        }

        private string NewColor
        {
            get
            {
                return $"#{rnd.Next(256).ToString("x2")}{rnd.Next(256).ToString("x2")}{rnd.Next(256).ToString("x2")}";
            }
        }

        private async Task OnGroupClick(dynamic e)
        {
            groupA = Guid.NewGuid();
            var group = Data.AddGroup($"{groupA}");
            group.Color = NewColor;

            child = Guid.NewGuid();

            var node = Data.AddNode($"{child}", $"{parent}");
            node.Label = "Inside";
            node.Group = $"{groupA}";
            node.Color = "orange";

            MapD3.Data = Data;

            await MapD3.Update();
        }

        private async Task OnInsideClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.AddNode($"{child}", $"{parent}");
            node.Label = "Inside";
            node.Group = $"{groupA}";
            node.Color = "lime";

            MapD3.Data = Data;

            await MapD3.Update();
        }

        private async Task OnLinkDistanceChange(dynamic e)
        {
           await MapD3.SetLinkDistance(int.Parse(e.Value));
        }

        private async Task OnLinkLengthsChange(dynamic e)
        {
           await MapD3.SetLinkLengths(int.Parse(e.Value));
        }
    }
}
