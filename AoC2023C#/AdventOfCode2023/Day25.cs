using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdventOfCode2023
{
    // Part 1: 474 * 1073= 508602 too low
    // Part 2: no solution required

    class Day25Edge
    {
        internal Day25Node Node1 { get; private set; }
        internal Day25Node Node2 { get; private set; }

        internal Day25Edge(Day25Node node1, Day25Node node2)
        {
            //Console.WriteLine($"Create edge {node1.Id}:{node1.Name} -> {node2.Id}:{node2.Name}");
            Node1 = node1;
            Node2 = node2;
        }

        internal bool UpdateNode(Day25Node fromNode, Day25Node toNode)
        {
            if (Node1 == fromNode)
            {
                Node1 = toNode;
            }
            else
            {
                Node2 = toNode;
            }
            return Node1 != Node2;
        }

        static public bool operator!=(Day25Edge? a, Day25Edge? b)
        {
            return !(a == b);
        }

        static public bool operator==(Day25Edge? a, Day25Edge? b)
        {
            if (a is null && b is null)
            {
                return true;
            }
            else if (a is null || b is null)
            {
                return false;
            }
            return (a.Node1.Id == b.Node1.Id && a.Node2.Id == b.Node2.Id) ||
                (a.Node1.Id == b.Node2.Id && a.Node2.Id == b.Node1.Id);
        }

        public override string ToString()
        {
            return $"{Node1.Id}:{Node1.Name} -> {Node2.Id}:{Node2.Name}";
        }
    }

    class Day25Node
    {
        internal int Id { get; private set; }
        internal string Name { get; private set; }
        internal int SubNodeCount { get; private set; }

        static internal int s_nextId = 1;

        public Day25Node(string name)
        {
            Name = name;
            SubNodeCount = 1;
            Id = s_nextId++;
        }

        public Day25Node(Day25Node srcNodeA, Day25Node srcNodeB)
        {
            Name = srcNodeA.Name + "+" + srcNodeB.Name;
            SubNodeCount = srcNodeA.SubNodeCount+ srcNodeB.SubNodeCount;
            Id = s_nextId++;
        }

    }

    internal class Day25 : DayBase, IDay
    {
        Dictionary<string, Day25Node> _nodeGraph = new Dictionary<string, Day25Node>();
        Dictionary<int, Day25Node> _nodesById = new Dictionary<int, Day25Node>();
        List<Day25Node> _nodes = new List<Day25Node>();
        List<Day25Edge> _edges = new List<Day25Edge> ();
        Dictionary<int, List<Day25Edge>> _nodeIdToEdgeList = new Dictionary<int, List<Day25Edge>>();

        Random _rnd = new Random(555);

        public void Execute(string filepath, int part)
        {
            ReadTextData(filepath, ParseLine, false);

            Dictionary<int, int> outputs = new Dictionary<int, int> ();
            if (part == 0)
            {
                Console.WriteLine($"Execute part 1");
                Console.WriteLine($"\n{_nodes.Count()} nodes");

                for (int i = 0; i < 1000; i++)
                {
                    _nodeGraph = new Dictionary<string, Day25Node>();
                    _nodes = new List<Day25Node>();
                    _edges = new List<Day25Edge>();
                    ReadTextData(filepath, ParseLine, false);
                    int output = ExecutePart1();
                    if (output==-1)
                    {
                        continue;
                    }

                    if (outputs.ContainsKey(output)==false)
                    {
                        outputs[output] = 0;
                    }
                    Console.WriteLine($"{output}=");
                    foreach (Day25Node node in _nodes)
                    {
                        Console.WriteLine($" {node.SubNodeCount}");
                    }
                    outputs[output]++;
                }
                foreach(int output in outputs.Keys)
                {
                    Console.WriteLine($"Output {output} occurred {outputs[output]} times");
                }
            }
            else
            {
                ExecutePart2();
            }
        }

        void ParseLine(string line)
        {
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }
            string[] splitByColon = line.Split(':');

            Day25Node parentNode = GetOrCreateNode(splitByColon[0]);
            string[] nodes = splitByColon[1].Split(" ");
            foreach (string nodeName in nodes)
            {
                string childName = nodeName.Trim();
                if (!String.IsNullOrEmpty(childName))
                {
                    Day25Node childNode = GetOrCreateNode(childName);

                    Day25Edge edge = new Day25Edge(parentNode, childNode);
                    _edges.Add(edge);
                    AddEdgeToDictionary(edge);
                }
            }
        }

        void AddEdgeToDictionary(Day25Edge edge)
        {
            List<Day25Edge>? edges;
            if (_nodeIdToEdgeList.TryGetValue(edge.Node1.Id, out edges)==false)
            {
                edges = new List<Day25Edge>();
                _nodeIdToEdgeList[edge.Node1.Id] = edges;
            }
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
            }

            if (_nodeIdToEdgeList.TryGetValue(edge.Node2.Id, out edges) == false)
            {
                edges = new List<Day25Edge>();
                _nodeIdToEdgeList[edge.Node2.Id] = edges;
            }
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
            }
        }

        void RemoveEdgeFromDictionary(Day25Edge edgeToDelete)
        {
            if (_nodeIdToEdgeList.ContainsKey(edgeToDelete.Node1.Id))
            {
                _nodeIdToEdgeList[edgeToDelete.Node1.Id].Remove(edgeToDelete);
            }
            if (_nodeIdToEdgeList.ContainsKey(edgeToDelete.Node2.Id))
            {
                _nodeIdToEdgeList[edgeToDelete.Node2.Id].Remove(edgeToDelete);
            }
        }

        Day25Node GetOrCreateNode(string name)
        {
            Day25Node? node;
            if (_nodeGraph.TryGetValue(name,out node) == false)
            {
                node = new Day25Node(name);
                _nodeGraph[name] = node;
                _nodes.Add(node);
                _nodesById[node.Id] = node;
            }
            return node;
        }

        int ExecutePart1()
        {
            if (!Karger())
            {
                return -1;
            }
            return CalculateOutput();
        }

        void ExecutePart2()
        {
            // Part 2 of day 25 is just kicking the contraption and pressing a red button (as long as solved previous 24 days)
        }

        int CalculateOutput()
        {
            int product = 1;
            foreach (Day25Node n in _nodes)
            {
                product *= n.SubNodeCount;
            }
            return product;

        }

        void OutputNodes()
        {
            int product = 1;
            foreach(Day25Node n in _nodes)
            {
                Console.WriteLine($"Node contains: {n.SubNodeCount}");
                product*= n.SubNodeCount;
            }
            
            Console.WriteLine($"Product of counts is: {product}");
        }

        void OutputEdge(int edgeIndex)
        {
            Day25Edge edge = _edges[edgeIndex];
            Day25Node nodeA = edge.Node1;
            Day25Node nodeB = edge.Node2;

            Console.WriteLine($"\nEdge index: {edgeIndex}:");
            Console.WriteLine($"             {nodeA.Id}:{nodeA.Name}:{nodeA.SubNodeCount} and: ");
            Console.WriteLine($"             {nodeB.Id}:{nodeB.Name}:{nodeB.SubNodeCount}");
        }

        bool Karger()
        {
            while(_edges.Count!=3)
            {
                if (_edges.Count < 3)
                {
                    return false;
                }
                int edgeIndex = _rnd.Next(0,_edges.Count);
                Day25Edge edge = _edges[edgeIndex];
                Day25Node nodeA = edge.Node1;
                Day25Node nodeB = edge.Node2;

                Day25Node newNode = new Day25Node(nodeA, nodeB);

                Day25Node node = nodeA;
                Day25Edge? deleteSelfReferentialEdge = null;
                for (int i = 0; i < 2; i++)
                {
                    List<Day25Edge> nodeEdges = _nodeIdToEdgeList[node.Id];

                    foreach (Day25Edge e in nodeEdges)
                    {
                        if (!e.UpdateNode(node, newNode))
                        {
                            deleteSelfReferentialEdge = e;
                        }
                        
                        AddEdgeToDictionary(e);
                    }
                    node = nodeB;
                }
                DeleteEdge(deleteSelfReferentialEdge);

                DeleteNode(nodeA);
                DeleteNode(nodeB) ;
                _nodes.Add(newNode);
                _nodesById[newNode.Id] = newNode;
            }
            return true;
        }

        void EnsureNodeIsDeleted(Day25Node node)
        {
            int index = 0;
            foreach(Day25Edge edge in _edges)
            {
                index++;
            }

        }

        void DeleteNode(Day25Node nodeToDelete)
        {
            _nodeGraph.Remove(nodeToDelete.Name);
            _nodesById.Remove(nodeToDelete.Id);
            _nodes.Remove(nodeToDelete);

            // Delete list of edges that start or end with this node
            _nodeIdToEdgeList.Remove(nodeToDelete.Id);
        }

        void DeleteEdge(Day25Edge? edgeToDelete)
        {
            if (edgeToDelete != null)
            {
                int index = _edges.IndexOf(edgeToDelete);

                for (int edgeIndex = 0; edgeIndex < _edges.Count; edgeIndex++)
                {
                    if (_edges[edgeIndex] == edgeToDelete)
                    {
                        _edges.RemoveAt(edgeIndex);
                        edgeIndex--;
                    }
                }
                _nodeIdToEdgeList[edgeToDelete.Node1.Id].Remove(edgeToDelete);
                _nodeIdToEdgeList[edgeToDelete.Node2.Id].Remove(edgeToDelete);
            }
        }
    }
}
