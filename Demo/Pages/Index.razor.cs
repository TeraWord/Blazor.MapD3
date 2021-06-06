﻿using System;
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

        private string NodeJson { get; set; }

        private MapD3 MapD3 { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
                        
            var data = new TeraWord.Blazor.MapD3.Data();

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

            items.Add(new
            {
                Code = child,
                Label = "Child",
                Parent = root,
                Description = "Descrizione",
                Status = 2,
                Header = (string)null,
                Footer = (string)null
            });

            items.Add(new
            {
                Code = Guid.NewGuid(),
                Label = "Lonely",
                Parent = Guid.Empty,
                Description = "Solo",
                Status = 2,
                Header = (string)null,
                Footer = (string)null
            });

            foreach (var item in items)
            {
                var node = data.NewNode(item.Code.ToString(), item.Parent.ToString());
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

            Data = data;
        }

        private async Task OnNodeClick(Node node)
        {
            var json = JsonSerializer.Serialize(node, new JsonSerializerOptions { WriteIndented = true });
            NodeJson = json;
            await Task.CompletedTask;
        }

        private void OnRootClick(dynamic e)
        {
            var newRoot = Guid.NewGuid();

            var node = Data.NewNode($"{newRoot}", $"{root}");

            root = newRoot;
            parent = newRoot;

            node.Label = "Root";
            node.Tooltip = node.Label + " - " + "Descrizione";
            node.Color = 0 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            Data = Data;
        }

        private void OnParentClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.NewNode($"{child}", $"{parent}");

            node.Label = "Parent";
            node.Tooltip = node.Label + " - " + "Descrizione";
            node.Color = 1 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            parent = child;

            Data = Data;
        }

        private void OnChildClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.NewNode($"{child}", $"{parent}");

            node.Label = "Child";
            node.Tooltip = node.Label + " - " + "Descrizione";
            node.Color = 2 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            Data = Data;
        }

        private void OnLinkClick(dynamic e)
        {
            Data.NewNode($"{child}", $"{root}");
            Data = Data;
        }

        private void OnLonelyClick(dynamic e)
        {
            child = Guid.NewGuid();

            var node = Data.NewNode($"{child}", null);

            node.Label = "Lonely";
            node.Tooltip = node.Label + " - " + "Lonely";
            node.Color = 2 switch { 0 => "red", 1 => "green", 2 => "blue", _ => "black" };

            Data = Data;
        }

        private void OnRemoveClick(dynamic e)
        {
            var node = Data.Nodes.FirstOrDefault(x => Guid.Parse(x.Code).Equals(child));
            
            if (node is not null)
            {
                Data.Nodes.Remove(node);
                child = Guid.Parse(Data.Nodes.LastOrDefault()?.Code ?? Guid.Empty.ToString());
                Data = Data;
            }
        }

        private async void OnZoomToFitClick(dynamic e)
        {
            await MapD3.ZoomToFit();
        }
    }
}
