using System;
using System.Collections.Generic;
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

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
                        
            var data = new TeraWord.Blazor.MapD3.Data();

            var items = (new[] {
                new {
                    ID = root,
                    Name = "Root",
                    ParentID = Guid.Empty,
                    Description = "Descrizione",
                    PhysicStatusCode = 0,
                    TemperatureSensorValue = (int?)36,
                    LanBandwidthUtilization = (int?)0
                }
            }).ToList();

            parent = Guid.NewGuid();

            items.Add(new
            {
                ID = parent,
                Name = "Perent",
                ParentID = root,
                Description = "Descrizione",
                PhysicStatusCode = 1,
                TemperatureSensorValue = (int?)null,
                LanBandwidthUtilization = (int?)null
            });

            child = Guid.NewGuid();

            items.Add(new
            {
                ID = child,
                Name = "Child",
                ParentID = parent,
                Description = "Descrizione",
                PhysicStatusCode = 2,
                TemperatureSensorValue = (int?)null,
                LanBandwidthUtilization = (int?)null
            });

            foreach (var item in items)
            {
                var node = data.AddNode(item.ID.ToString(), item.Name, item.ParentID.ToString(), null);

                node.ID = item.ID.ToString();
                node.Hint = item.Name + " - " + item.Description;
                node.Color = item.PhysicStatusCode switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };
                node.Header = item.TemperatureSensorValue.ToString();
                node.Footer = item.LanBandwidthUtilization.ToString();

                //node.Data = new
                //{
                //    Pecore = 5,
                //    Capre = 2
                //};
            }

            Data = data.Compile();
        }

        private async Task OnNodeClick(Node node)
        {
            Console.WriteLine(JsonSerializer.Serialize(node, new JsonSerializerOptions { WriteIndented = true }));
            await Task.CompletedTask;
        }

        private void OnRootClick(dynamic e)
        {
            parent = Guid.NewGuid();

            var node = Data.AddNode($"{parent}", "Parent", $"{root}", null);

            node.ID = $"{parent}";
            node.Hint = node.Name + " - " + "Descrizione";
            node.Color = 1 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            Data = Data.Compile();
        }

        private void OnParentClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.AddNode($"{child}", "Parent", $"{parent}", null);

            node.ID = $"{child}";
            node.Hint = node.Name + " - " + "Descrizione";
            node.Color = 1 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            parent = child;

            Data = Data.Compile();
        }

        private void OnChildClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.AddNode($"{child}", "Child", $"{parent}", null);

            node.ID = $"{child}";
            node.Hint = node.Name + " - " + "Descrizione";
            node.Color = 2 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            Data = Data.Compile();
        }
    }
}
