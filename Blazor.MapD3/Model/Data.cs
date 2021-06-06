using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeraWord.Blazor.MapD3
{
    public class Data
    {
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Link> Links { get; set; } = new List<Link>();
        public List<Group> Groups { get; set; } = new List<Group>();

        public Data() { }

        public Data(IEnumerable<Node> nodes)
        {
            Assign(nodes);
        }

        public void Assign(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var item = NewNode(node.Code, null);
                item.Color = node.Color;
                item.Data = node.Data;
                item.Footer = node.Footer;
                item.Group = node.Group;
                item.Header = node.Header;
                item.Label = node.Label;
                item.Parents = node.Parents;
                item.Tooltip = node.Tooltip;

                foreach (var parent in node.Parents)
                    if (!string.IsNullOrWhiteSpace(parent))
                        if (!item.Parents.Contains(parent))
                            node.Parents.Add(parent);
            }
        }

        internal Data Compile()
        {
            Links.Clear();
            
            for (int contChild = 0; contChild < Nodes.Count; contChild++)
            {
                var child = Nodes[contChild];

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
                                var link = new Link();
                                link.Source = contChild;
                                link.Target = contParent;
                                link.Code = $"{parent}-{child.Code}";
                                Links.Add(link);
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(child.Group))
                {
                    Group group = null;

                    for (int contGroup = 0; contGroup < Groups.Count; contGroup++)
                    {
                        if (Groups[contGroup].Code == child.Group)
                        {
                            group = Groups[contGroup];
                            break;
                        }
                    }

                    if (group == null)
                    {
                        //group = new ColaGroup();
                        //group.code = groupCode; 
                        //groups.Add(group);
                    }
                    else
                    {
                        group.Leaves.Add(contChild);
                    }
                }
            }

            Groups = (from g in Groups where g.Leaves.Count > 0 select g).ToList();
            Links = (from l in Links orderby l.Target, l.Source select l).ToList();

            return this;
        }

        public Group AddGroup(string code, string parentCode = null)
        {
            code = code.Trim().ToUpper();
            if (!String.IsNullOrEmpty(parentCode)) parentCode = parentCode.Trim().ToUpper();

            Group group = null;
            int groupId = 0;

            for (int cont = 0; cont < Groups.Count; cont++)
            {
                if (Groups[cont].Code == code)
                {
                    group = Groups[cont];
                    groupId = cont;
                    break;
                }
            }

            if (group == null)
            {
                group = new Group();
                group.Code = code;
                Groups.Add(group);
                groupId = Groups.Count - 1;
            }

            if (!String.IsNullOrEmpty(parentCode))
            {
                for (int cont = 0; cont < Groups.Count; cont++)
                {
                    if ((cont != groupId) && (Groups[cont].Code == parentCode))
                    {
                        Groups[cont].Groups.Add(groupId);
                        break;
                    }
                }
            }

            return group;
        }

        public Node NewNode(string code, string parent = null)
        {
            Node node = null;

            for (int cont = 0; cont < Nodes.Count; cont++)
            {
                if (Nodes[cont].Code == code)
                {
                    node = Nodes[cont];
                    break;
                }
            }

            if (node == null)
            {
                node = new Node();
                node.Code = code; 
                Nodes.Add(node);
            }
 
            if (!string.IsNullOrWhiteSpace(parent)) if (!node.Parents.Contains(parent)) node.Parents.Add(parent);

            return node;
        }
    }
}