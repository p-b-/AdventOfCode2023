using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    // Part 1 24253
    // Part 2 12357789728873
    internal class Day8 : DayBase, IDay
    {
        enum ParseState
        {
            None,
            ParseDirections,
            ParseNodes
        }

        ParseState _state;

        List<bool> _directions_goRightNotLeft;
        Dictionary<string, string[]> _nodeIdToNode;
        List<int> _goLeftNodeIds;
        List<int> _goRightNodeIds;
        int _startNodeId;
        string _startNode;

        int _nextId = 1;
        Dictionary<string, int> _nodeNamesToIds;
        Dictionary<int, string> _nodeIdToName;

        List<string> _startNodes;
        List<string> _endNodes;

        internal Day8()
        {
            _state = ParseState.ParseDirections;
            _directions_goRightNotLeft = new List<bool>();
            _nodeIdToNode = new Dictionary<string, string[]>();
            _nodeNamesToIds = new Dictionary<string, int>();
            _nodeIdToName = new Dictionary<int, string>();
            _startNodes = new List<string>();
            _endNodes = new List<string>();

            _goLeftNodeIds = new List<int>();
            _goRightNodeIds = new List<int>();
            _startNode = String.Empty;
        }

        public void Execute(string filepath, int part)
        {
            List<Int64> findLCM = new List<Int64>();
            findLCM.Add(18);
            findLCM.Add(24);
            findLCM.Add(28);
            LCM lcm2 = new LCM(findLCM);
            lcm2.CalculateLCM();

            ReadTextData(filepath, ParseLine, false);

            if (part == 0)
            {
                Int64 stepCount = ProcessData_Part1_Version1("AAA", "ZZZ");
                Console.WriteLine($"Step count {stepCount}");
            }
            else
            {
                List<Int64> setOfResults  = new List<Int64>();
                for (int i = 0; i < _startNodes.Count(); ++i)
                {
                    Int64 stepCount = ProcessData_Part1_Version1(_startNodes[i], _endNodes[i]);
                    setOfResults.Add(stepCount);
                    Console.WriteLine($"Start {_startNodes[i]} => {_endNodes[i]} in {stepCount} steps");
                }
                LCM lcm = new LCM(setOfResults);
                Console.WriteLine(lcm.CalculateLCM());
            }
        }

        Int64 ProcessData_Part1_Version1(string startNode, string endNode)
        {
            int directionIndex = 0;
            string currentNode = startNode;

            bool loop = true;
            Int64 stepCount = 0;
            while (loop)
            {
                ++stepCount;
                bool goRightNotLeft = _directions_goRightNotLeft[directionIndex++];
                if (directionIndex == _directions_goRightNotLeft.Count())
                {
                    directionIndex = 0;
                }


                var node = _nodeIdToNode[currentNode];
                string nextNode = node[goRightNotLeft ? 1 : 0];
                currentNode = nextNode;
                if (currentNode.EndsWith('Z'))
                {
                    loop = false;
                }
            }
            return stepCount;
        }

        void ParseLine(string line)
        {
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }
            line = line.Trim();
            if (_state == ParseState.ParseDirections)
            {
                ParseDirections(line);
                _state = ParseState.ParseNodes;
            }
            else
            {
                ParseNode(line);
            }
        }

        void ParseDirections(string line)
        {
            for (int i = 0; i < line.Length; ++i)
            {
                char c = line[i];
                if (c == 'R')
                {
                    _directions_goRightNotLeft.Add(true);
                }
                else if (c == 'L')
                {
                    _directions_goRightNotLeft.Add(false);
                }
            }
        }

        void ParseNode(string line)
        {
            string[] lineSplitByEquals = line.Split('=');
            string nodeName = lineSplitByEquals[0].Trim();

            int nodeId = GetNodeIdFromNodeName(nodeName);

            string nodeData = lineSplitByEquals[1].Trim();

            string[] nodeItems = nodeData.Split(',');
            string nodeItem0 = nodeItems[0].Trim().Substring(1);

            string nodeItem1Precursor = nodeItems[1].Trim();
            string nodeItem1 = nodeItem1Precursor.Substring(0, nodeItem1Precursor.Length - 1);

            int nodeLeftId = GetNodeIdFromNodeName(nodeItem0);
            int nodeRightId = GetNodeIdFromNodeName(nodeItem1);

            if (nodeName == "AAA")
            {
                _startNodeId = nodeId;
                _startNode = nodeName;
            }

            if (nodeName.EndsWith("A"))
            {
                _startNodes.Add(nodeName);
                _endNodes.Add(nodeName.Substring(0, 2) + "Z");
            }
            SetGoLeftOrRightNodeId(_goLeftNodeIds, nodeId, nodeLeftId);
            SetGoLeftOrRightNodeId(_goRightNodeIds, nodeId, nodeRightId);

            _nodeIdToNode[nodeName] = new string[] { nodeItem0, nodeItem1 };
        }

        int GetNodeIdFromNodeName(string nodeName)
        {
            if (_nodeNamesToIds.ContainsKey(nodeName))
            {
                return _nodeNamesToIds[nodeName];
            }
            int idToReturn = _nextId++;
            _nodeNamesToIds[nodeName] = idToReturn;
            _nodeIdToName[idToReturn] = nodeName;
            return idToReturn;
        }

        void SetGoLeftOrRightNodeId(List<int> nodeIds, int nodeId, int setDirectionNodeId)
        {
            for (int i = nodeIds.Count; i <= nodeId; ++i)
            {
                nodeIds.Add(-1);
            }
            nodeIds[nodeId] = setDirectionNodeId;
        }
    }

    class LCM
    { 

        private List<Int64> NumberSet = new List<Int64>();
        private List<Int64> all_factors = new List<Int64>(); // factors common to our set_of_numbers

        private int index; // index into array common_factors

        public LCM(List<Int64> group)
        {
            //iterate through and retrieve members
            foreach (Int64 number in group)
            {
                NumberSet.Add(number);
            }

            NumberSet.Sort();
            NumberSet.Reverse();

        }

  
        private void FindLCMFactors()
        {
            NumberSet.Sort();
            NumberSet.Reverse();
            bool allUnary = false;

            while (!allUnary && index <= NumberSet[0])
            {
                bool resort = false;
                // Assume all numbers are now one, unless proven otherwise in loop
                allUnary = true;
                for (int j = 0; j < NumberSet.Count; j++)
                {
                    if (NumberSet[j]>1)
                    {
                        allUnary = false;
                    }
                    if (NumberSet[j] != 1 && (NumberSet[j] % index) == 0)
                    {
                        NumberSet[j] /= index;
                        if (!resort)
                        {
                            all_factors.Add(index);
                            resort = true;
                        }
                    }
                }
                if (resort)
                {
                    NumberSet.Sort();
                    NumberSet.Reverse();
                }
                else
                {
                    ++index;
                }
            }
        }

        public Int64 CalculateLCM()
        {
            index = 2;
            FindLCMFactors();

            Int64 result = 1;
            foreach (Int64 factor in all_factors)
            {
                result *= factor;
            }

            return result;
        }
    }
}
