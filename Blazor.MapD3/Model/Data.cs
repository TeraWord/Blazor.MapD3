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

        public Data Compile()
        {
            for (int contChild = 0; contChild < Nodes.Count; contChild++)
            {
                var child = Nodes[contChild];

                if (!String.IsNullOrEmpty(child.Parent))
                {
                    for (int contParent = 0; contParent < Nodes.Count; contParent++)
                    {
                        if ((contChild != contParent) && (Nodes[contParent].Code == child.Parent))
                        {
                            var foundIn = from l in Links where l.Source == contChild && l.Target == contParent select l;
                            var foundOut = from l in Links where l.Source == contParent && l.Target == contChild select l;

                            if ((foundIn.Count() == 0) && (foundOut.Count() == 0))
                            {
                                var link = new Link();
                                link.Source = contChild;
                                link.Target = contParent;
                                link.Code = child.Code;
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

        public Node AddNode(string code, string name, string parentCode, string groupCode)
        {
            code = code.Trim().ToUpper();
            if (!String.IsNullOrEmpty(parentCode)) parentCode = parentCode.Trim().ToUpper();
            if (!String.IsNullOrEmpty(groupCode)) groupCode = groupCode.Trim().ToUpper();

            Node node = null;
            int nodeId = 0;

            for (int cont = 0; cont < Nodes.Count; cont++)
            {
                if (Nodes[cont].Code == code)
                {
                    node = Nodes[cont];
                    nodeId = cont;
                    break;
                }
            }

            if (node == null)
            {
                node = new Node();
                node.Code = code;

                Nodes.Add(node);
                nodeId = Nodes.Count - 1;
            }

            node.Name = name;
            node.Parent = parentCode;
            node.Group = groupCode;

            return node;
        }
    }
}