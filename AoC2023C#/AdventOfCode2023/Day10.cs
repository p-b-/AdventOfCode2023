using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AdventOfCode2023
{
    // Part 1: 6842
    class MapElement
    {
        internal int X { get; private set; }
        internal int Y { get; private set; }
        internal char ElementChar { get; set; } 
        internal bool IsStart { get; private set; } = false;
        internal bool IsGround { get; private set; } = false;
        internal bool Initialised { get; private set; } = false;
        internal bool Invalid
        {
            get
            {
                if (IsStart)
                {
                    return false;
                }
                return ConnectedFrom == null || ConnectedTo == null;
            }
        }

        internal MapElement? ConnectedFrom { get; private set; } = null;
        internal MapElement? ConnectedTo { get; private set; } = null;
        internal bool IsPartOfLoop { get; set; } = false;

        internal MapElement(int x, int y)
        {
            X = x;
            Y = y;
        }

        internal void SetChar(char c)
        {
            ElementChar = c;
        }

        internal void ConnectFrom(MapElement? connectFrom)
        {
            ConnectedFrom = connectFrom;
        }

        internal void ConnectTo(MapElement? connectTo)
        {
            ConnectedTo = connectTo;
        }


        internal void SetToStart()
        {
            Initialised = true;
            IsStart = true;
        }

        internal void NoPipe()
        {
            Initialised = true;
            IsGround = true;
        }

        internal MapElement? GetOppositeConnection(MapElement oppositeFrom)
        {
            if (ConnectedFrom == oppositeFrom)
            {
                return ConnectedTo;
            }
            else if (ConnectedTo == oppositeFrom)
            {
                return ConnectedFrom;
            }
            return null;
        }
    }
    internal class Day10 : DayBase, IDay
    {

        // Part 1: 1877825184
        // Part 2: 1108
        List<List<Int64>> linesOfTestResults = new List<List<Int64>>();
        List<List<MapElement>> _mapElements = new List<List<MapElement>>();

        int? _startLocX = null;
        int? _startLocY = null;

        int _parseX = 0;
        int _parseY = 0;
        int? _width;
        int _height = 0;
        internal Day10()
        {
        }

        public void Execute(string filepath, int part)
        {
            ReadTextData(filepath, ParseLine, false);
            (Int64? steps, int direction) = FindLoop();
            if (part == 0)
            {
                Console.WriteLine($"Steps: {steps / 2}");
            }
            else
            {
                MarkLoop(direction);
                UpdateStartLocation();
                OutputLoop();
                Int64 result = CountInsideLoop();
                Console.WriteLine($"Inside loop {result}");
                
            }
        }

        (Int64, int) FindLoop()
        {
            int x = _startLocX!.Value;
            int y = _startLocY!.Value;
            MapElement startElement = GetMapElement(x, y);

            MapElement? connectedElement;

            for (int direction = 0; direction < 4; ++direction)
            {
                connectedElement = direction switch
                {
                    0 => GetMapElementWithBoundsCheck(x, y - 1),
                    1 => GetMapElementWithBoundsCheck(x + 1, y),
                    2 => GetMapElementWithBoundsCheck(x, y + 1),
                    3 => GetMapElementWithBoundsCheck(x - 1, y),
                    _ => throw new Exception("Invalidation direction index")
                }; ;
                if (connectedElement != null)
                {
                    MapElement? connectedTo = connectedElement.GetOppositeConnection(startElement);
                    if (connectedTo != null)
                    {
                        Int64? steps = FollowLoop(previous: startElement, startFrom: connectedElement);
                        if (steps != null && steps.Value!=0)
                        {
                            return (steps.Value, direction);
                        }
                    }

                }
            }

            return (0, 0);
        }

        void MarkLoop(int direction)
        { 
            int x = _startLocX!.Value;
            int y = _startLocY!.Value;
            MapElement startElement = GetMapElement(x, y);

            MapElement? connectedElement = direction switch
                {
                    0 => GetMapElementWithBoundsCheck(x, y - 1),
                    1 => GetMapElementWithBoundsCheck(x + 1, y),
                    2 => GetMapElementWithBoundsCheck(x, y + 1),
                    3 => GetMapElementWithBoundsCheck(x - 1, y),
                    _ => throw new Exception("Invalidation direction index")
                }; ;
            if (connectedElement != null)
            {
                MapElement? connectedTo = connectedElement.GetOppositeConnection(startElement);
                if (connectedTo != null)
                {
                    Int64? steps = MarkLoop(previous: startElement, startFrom: connectedElement);
                    if (steps != null && steps.Value != 0)
                    {
                        return;
                    }
                }

            }

        }

        Int64 FollowLoop(MapElement previous, MapElement startFrom)
        {
            MapElement getTo = previous;
            Dictionary<MapElement, bool> visited = new Dictionary<MapElement, bool>();
            Int64 steps = 0;

            MapElement? roving = startFrom;
            while (roving != null)
            {
                if (visited.ContainsKey(roving))
                {
                    // Looped onto itself without reach endpoint
                    return 0;
                }
                ++steps;
                if (roving == getTo)
                {
                    return steps;
                }
                visited[roving] = true;
                MapElement temp = roving;
                roving = roving.GetOppositeConnection(previous);
                previous = temp;
            }

            return 0;
        }

        Int64 MarkLoop(MapElement previous, MapElement startFrom)
        {
            previous.IsPartOfLoop = true;
            MapElement getTo = previous;
            Dictionary<MapElement, bool> visited = new Dictionary<MapElement, bool>();
            Int64 steps = 0;

            MapElement? roving = startFrom;
            while (roving != null)
            {
                roving.IsPartOfLoop = true;
                if (visited.ContainsKey(roving))
                {
                    // Looped onto itself without reach endpoint
                    return 0;
                }
                ++steps;
                if (roving == getTo)
                {
                    return steps;
                }
                visited[roving] = true;
                MapElement temp = roving;
                roving = roving.GetOppositeConnection(previous);
                previous = temp;
            }

            return 0;
        }
        Int64 CountInsideLoop()
        {
            Int64 sum = 0;
            int y = -1;
            foreach (List<MapElement> list in _mapElements)
            {
                ++y;
                bool insideLoop = false;
                bool lookingForEndChar = false;
                char flipOnChar = ' ';
                for (int i = 0; i < list.Count; ++i)
                {
                    MapElement element = list[i];
                    char thisChar= list[i].ElementChar;

                    if (lookingForEndChar)
                    {
                        if (thisChar == '-')
                        {
                            continue;
                        }
                        else if (thisChar == flipOnChar)
                        {
                            insideLoop = !insideLoop;
                        }
                        lookingForEndChar = false;
                    }
                    else if ((element.IsGround || element.IsPartOfLoop==false) && insideLoop)
                    {
                        ++sum;
                    }
                    else if(element.IsPartOfLoop) 
                    {
                        if (thisChar == '|')
                        {
                            insideLoop = !insideLoop;
                        }
                        else if (thisChar == 'F')
                        {
                            lookingForEndChar = true;
                            flipOnChar = 'J';
                        }
                        else if (thisChar == 'L')
                        {
                            lookingForEndChar = true;
                            flipOnChar = '7';
                        }
                    }
                }
            }
            return sum;
        }


        void OutputLoop()
        {
            foreach(List<MapElement> list in _mapElements)
            {
                for(int i=0;i<list.Count;++i)
                {
                    MapElement element = list[i];
                    if (element.IsPartOfLoop)
                    {
                        Console.Write(element.ElementChar);
                    }
                    else if (element.IsGround)
                    {
                        Console.Write(".");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }
        }

        void ParseLine(string line)
        {
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }
            if (_width == null)
            {
                _width = line.Length;
            }
            ++_height;

            foreach (char c in line)
            {
                MapElement element = GetMapElement(_parseX, _parseY);
                switch (c)
                {
                    case 'S':
                        if (_startLocX != null)
                        {
                            throw new Exception($"({_parseX},{_parseY}): Already found start location");
                        }
                        _startLocX = _parseX;
                        _startLocY = _parseY;
                        element.SetToStart();
                        break;
                    case '|':
                        ProcessNorthToSouth(c, element);
                        break;
                    case '-':
                        ProcessEastToWest(c, element);
                        break;
                    case 'L':
                        ProcessNorthToEast(c, element);
                        break;
                    case 'J':
                        ProcessNorthToWest(c, element);
                        break;
                    case '7':
                        ProcessSouthToWest(c, element);
                        break;
                    case 'F':
                        ProcessSouthToEast(c, element);
                        break;
                    default:
                    case '.':
                        element.NoPipe();
                        break;
                }
                ++_parseX;
            }


            ++_parseY;
            _parseX = 0;
        }

        void ProcessNorthToEast(char c,MapElement element)
        {
            ProcessMapElementWithDirections(c, element, 0, -1, 1, 0);
        }
        void ProcessNorthToWest(char c, MapElement element)
        {
            ProcessMapElementWithDirections(c, element, 0, -1, -1, 0);
        }

        void ProcessSouthToEast(char c, MapElement element)
        {
            ProcessMapElementWithDirections(c, element, 1, 0, 0, 1);
        }
        void ProcessSouthToWest(char c, MapElement element)
        {
            ProcessMapElementWithDirections(c, element, -1, 0, 0, 1);
        }

        void ProcessEastToWest(char c, MapElement element)
        {
            ProcessMapElementWithDirections(c, element, -1, 0, 1, 0);
        }
        void ProcessNorthToSouth(char c, MapElement element)
        {
            ProcessMapElementWithDirections(c, element, 0, -1, 0, 1);
        }


        void ProcessMapElementWithDirections(char c, MapElement element, int dxFrom, int dyFrom , int dxTo, int dyTo)
        {
            element.SetChar(c);
            MapElement? connectedFromElement = GetMapElementWithBoundsCheck(_parseX + dxFrom, _parseY + dyFrom);
            element.ConnectFrom(connectedFromElement);
            MapElement? connectedToElement = GetMapElementWithBoundsCheck(_parseX + dxTo, _parseY + dyTo);
            element.ConnectTo(connectedToElement);
        }

        MapElement GetMapElement(int x, int y)
        {
            if (y>=_mapElements.Count)
            {
                for(int addY =_mapElements.Count;addY<=y;++addY)
                {
                    List<MapElement> row = new List<MapElement>();
                    for(int addX = 0; addX < _width; ++addX)
                    {
                        row.Add(new MapElement(addX,addY));
                    }
                    _mapElements.Add(row);
                }
            }
            return _mapElements[y][x];
        }

        MapElement? GetMapElementWithBoundsCheck(int x, int y)
        {
            if (y<0 || x<0 || x >= _width) {
                return null;
            }
            return GetMapElement(x, y);
        }

        MapElement? GetMapElementWithBoundsCheckEnforcePipedom(int x, int y)
        {
            if (y < 0 || x < 0 || x >= _width || y>=_height)
            {
                return null;
            }
            MapElement element = GetMapElement(x, y);
            if (element==null || element.IsPartOfLoop==false)
            {
                return null;
            }
            return element;
        }

        void UpdateStartLocation()
        {
            int x = _startLocX!.Value;
            int y = _startLocY!.Value;
            MapElement startElement = GetMapElementWithBoundsCheckEnforcePipedom(x, y)!;

            MapElement? elementAbove = GetMapElementWithBoundsCheckEnforcePipedom(x, y - 1);
            MapElement? elementBelow = GetMapElementWithBoundsCheckEnforcePipedom(x, y + 1);
            MapElement? elementRight = GetMapElementWithBoundsCheckEnforcePipedom(x + 1, y);
            MapElement? elementLeft = GetMapElementWithBoundsCheckEnforcePipedom(x + 1, y);
            // 7 F
            // J L
            // -
            // |


            if (elementAbove != null &&
                (elementAbove.ElementChar == '|' || elementAbove.ElementChar == '7' || elementAbove.ElementChar == 'F'))
            {
                if (elementRight != null &&
                    (elementRight.ElementChar == 'J' || elementRight.ElementChar == '7' || elementRight.ElementChar == '-'))
                {
                    startElement.ElementChar = 'L';
                }
                else if (elementLeft != null &&
                    (elementLeft.ElementChar == 'F' || elementLeft.ElementChar == 'L' || elementLeft.ElementChar == '-'))
                {
                    startElement.ElementChar = 'J';
                }
                else if (elementBelow != null &&
                    (elementBelow.ElementChar == 'J' || elementBelow.ElementChar == 'L' || elementBelow.ElementChar == '|'))
                {
                    startElement.ElementChar = '|';
                }
                else
                {
                    throw new Exception("Couldn't match element from above for starting element");
                }
            }
            else if (elementBelow != null &&
                (elementBelow.ElementChar == '|' || elementBelow.ElementChar == 'J' || elementBelow.ElementChar == 'L'))
            {
                if (elementRight != null &&
                    (elementRight.ElementChar == 'J' || elementRight.ElementChar == '7' || elementRight.ElementChar == '-'))
                {
                    startElement.ElementChar = 'F';
                }
                else if (elementLeft != null &&
                    (elementLeft.ElementChar == 'F' || elementLeft.ElementChar == 'L' || elementLeft.ElementChar == '-'))
                {
                    startElement.ElementChar = '7';
                }
                else
                {
                    throw new Exception("Couldn't match element from below for starting element");
                }
            }
            else if (elementLeft != null &&
                (elementLeft.ElementChar=='L' || elementLeft.ElementChar=='F' || elementLeft.ElementChar=='-'))
            { 
                if (elementRight!=null &&
                    (elementRight.ElementChar =='J'|| elementRight.ElementChar=='7' || elementRight.ElementChar=='-'))
                {
                    startElement.ElementChar = '-';
                }
                else
                {
                    throw new Exception("Couldn't match element from left for starting element");
                }

            }
            else
            {
                throw new Exception("Couldn't match element for starting element");
            }
        }
    }
}
