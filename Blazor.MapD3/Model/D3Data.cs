using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeraWord.Blazor.MapD3
{
    public class D3Data
    {
        public List<D3Node> Nodes { get; set; } = new();
        public List<D3Link> Links { get; set; } = new();
        public List<D3Group> Groups { get; set; } = new();

        public D3Data() { }

        public D3Data(IEnumerable<D3Node> nodes) => Assign(nodes);
        
        public void Assign(IEnumerable<D3Node> nodes)
        {
            foreach (var node in nodes) AddNode(node);
        }

        internal D3Data Compile()
        {
            Links.Clear();

            int index = 0; // Nodes.Count;

            foreach (var group in Groups)
            {
                group.Index = index++;
                group.Leaves.Clear();
                group.Groups.Clear();
            };

            index = 0;

            for (int contChild = 0; contChild < Nodes.Count; contChild++)
            {
                var child = Nodes[contChild];

                child.Index = index++;

                foreach (var parent in child.Parents)
                {
                    for (int contParent = 0; contParent < Nodes.Count; contParent++)
                    {
                        if ((contChild != contParent) && (Nodes[contParent].Code == parent))
                        {
                            var foundIn = from l in Links where l.Source == contChild && l.Target == contParent select l;
                            var foundOut = from l in Links where l.Source == contParent && l.Target == contChild select l;

                            if ((foundIn.Count() == 0) && (foundOut.Count() == 0))
                            {
                                var link = new D3Link();
                                link.Source = contChild;
                                link.Target = contParent;
                                link.Code = $"{parent}-{child.Code}";
                                Links.Add(link);
                            }
                        }
                    }
                }
                                
                var group = Groups.FirstOrDefault(x => x.Code.Equals(child.Group));

                if (group != null)
                {
                    if (!group.Leaves.Contains(child.Index)) group.Leaves.Add(child.Index);
                }                 
            }

            foreach(var group in Groups)
            {
                foreach(var child in group._Groups)
                {
                    var childGroup = Groups.FirstOrDefault(x => x.Code.Equals(child));

                    if (childGroup is not null)
                        if (!group.Groups.Contains(childGroup.Index))
                            group.Groups.Add(childGroup.Index);
                }

                group.Groups = group.Groups.OrderBy(x => x).ToList();
            }

            //Groups = (from g in Groups where g.Leaves.Count > 0 select g).ToList();
            Links = (from l in Links orderby l.Target, l.Source select l).ToList();

#if DEBUG
            //System.IO.File.WriteAllText("D:/Tests/Blazor.MapD3/Data.json", System.Text.Json.JsonSerializer.Serialize (this, new() { WriteIndented = true }));
#endif

            return this;
        }

        public D3Group AddGroup(string code, string parent = null)
        { 
            D3Group group = Groups.FirstOrDefault(x => x.Code.Equals(code));

            if (group == null)
            {
                group = new D3Group();
                group.Code = code;
                Groups.Add(group);
            }

            if (!String.IsNullOrEmpty(parent))
                foreach (var parentGroup in Groups.Where(x => x.Code.Equals(parent)))
                    if (!parentGroup._Groups.Contains(code))
                        parentGroup._Groups.Add(code);

            return group;
        }

        public D3Node FindNode(string code)
        {
            return Nodes?.FirstOrDefault(x => x.Code.Equals(code));
        }

        public bool ExistsNode(string code)
        {
            return FindNode(code) is not null;
        }

        public D3Node AddNode(string code, string parent = null)
        {
            var node = FindNode(code);

            if (node is null)
            {
                node = new D3Node();
                node.Code = code;
                Nodes.Add(node);
            }

            return AddLink(node.Code, parent);
        }

        public D3Node AddNode(D3Node node)
        { 
            var result = AddNode(node.Code);

            result.Color = node.Color;
            result.Data = node.Data;
            result.Footer = node.Footer;
            result.Group = node.Group;
            result.Header = node.Header;
            result.Label = node.Label;
            result.Tooltip = node.Tooltip;

            return AddLinks(result.Code, node.Parents);
        }

        public D3Node AddLink(string code, string parent)
        {
            return AddLinks(code, new string[] { parent });
        }

        public D3Node AddLinks(string code, IEnumerable<string> parents)
        { 
            var node = FindNode(code);

            if (node is not null && parents is not null)
                foreach (var parent in parents)
                    if (!string.IsNullOrWhiteSpace(parent))
                        if (!node.Parents.Contains(parent))
                            node.Parents.Add(parent);

            node.Parents = new(node.Parents);

            return node;
        }
    }
}