using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    // Part 1: 36725
    // Part 2: 97874103749720
    enum Day18ElementType
    {
        Unknown,
        Ground,
        Trench,
        Inside
    }

    enum Day18TrenchType
    {
        None,
        NorthToSouth,
        EastToWest,
        SouthToEast,
        SouthToWest,
        NorthToEast,
        NorthToWest
    }
    class Day18Element
    {
        internal Day18ElementType ElementType { get; set; }
        internal Day18TrenchType TrenchType { get; set; }
        internal string Colour {  get; set; }
        internal int Depth { get; set; }
        internal char DrawWith { get; set; }

        internal Day18Element()
        {
            ElementType = Day18ElementType.Unknown;
            Depth = 0;
            Colour = "#000000";
            DrawWith = ' ';
            TrenchType = Day18TrenchType.None;
        }
    }

    class Day18Command
    {
        internal char Command { get; set; }
        internal int Length { get; set; }
        internal string Colour { get; set; }
    
        internal Day18Command(char command, int length, string colour)
        {
            Command = command;
            Length = length;
            Colour = colour;
        }
    }
    internal class Day18 : DayBase, IDay
    {
        int _width;
        int _height;
        int _digX;
        int _digY;

        List<List<Day18Element>> _data= new List<List<Day18Element>>();
        List<Day18Command> _cmds = new List<Day18Command>();

        public void Execute(string filepath, int part)
        {
            if (part == 0)
            {
                ReadTextData(filepath, ParseLinePart1, false);
            }
            else
            {
                ReadTextData(filepath, ParseLinePart2, false);
            }

            if (part == 0)
            {
                ExecutePart1();
            }
            else
            {
                ExecutePart2();
            }
        }

        void ParseLinePart1(string line)
        {
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }
            string[] splitBySpaces = line.Split(' ');
            char cmdChar = splitBySpaces[0][0];
            int length = int.Parse(splitBySpaces[1]);
            string colour = splitBySpaces[2].Substring(1, 7);

            Day18Command cmd = new Day18Command(cmdChar, length, colour);
            _cmds.Add(cmd);
        }

        void ParseLinePart2(string line)
        {
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }

            string[] splitBySpaces = line.Split(' ');

            string lengthInHex= splitBySpaces[2].Substring(2, splitBySpaces[2].Length-4);
            int length = Convert.ToInt32(lengthInHex, 16);

            char cmdChar = splitBySpaces[2][splitBySpaces[2].Length - 2];
            cmdChar = cmdChar switch
            {
                '0' => 'R',
                '1' => 'D',
                '2' => 'L',
                '3' => 'U',
                _ => throw new Exception($"Unknown cmd character {cmdChar}")
            };
            Console.WriteLine($"Length in {length}, cmd {cmdChar}");

            Day18Command cmd = new Day18Command(cmdChar, length, "#x11");
            _cmds.Add(cmd);
        }

        void ExecutePart1()
        {
            Console.WriteLine("Part 1");
            DetermineSize();
            DigTrench();
            MarkInsideSpaces();
            OutputData("C:\\Misc\\AdventOfCode2023\\Day18\\output.txt");
            Int64 count = CountTrenchDepth(false);
            Console.WriteLine($"Size is {count}");
        }

        void ExecutePart2()
        {
            Console.WriteLine("Part 2");
            Shoelace();

            Int64 area = Shoelace();
            Console.WriteLine($"Size is {area}");
        }

        Int64 Shoelace()
        {
            Int64 startX = 0;
            Int64 startY = 0;

            Int64 x = startX;
            Int64 y = startY;
            Int64 nextX = 0;
            Int64 nextY = 0;

            Int64 sum = 0;
            Int64 perimiter = 0; 
            for(int cmdIndex=0; cmdIndex < _cmds.Count;++cmdIndex)
            {
                Day18Command cmd = _cmds[cmdIndex];
                (nextX, nextY) = GetNextCoordsForCmd(cmd, x, y);
                perimiter += cmd.Length;
                Console.WriteLine($"({x},{y}) ({nextX},{nextY})");

                sum += x * nextY - nextX * y;

                x = nextX;
                y = nextY;
            }
            sum += x - y;

            Console.WriteLine($"({x},{y}) ({nextX},{nextY})");

            Int64 area = sum/2;

            area = area + perimiter / 2 + 1;
            return area;
        }

        (Int64 nextX, Int64 nextY) GetNextCoordsForCmd(Day18Command cmd, Int64 x, Int64 y)
        {
            Int64 nextX = x;
            Int64 nextY = y;
            if (cmd.Command=='U')
            {
                nextY -= cmd.Length;
            }
            else if (cmd.Command == 'D')
            {
                nextY += cmd.Length;
            }
            else if (cmd.Command == 'L')
            {
                nextX -= cmd.Length;
            }
            else if (cmd.Command == 'R')
            {
                nextX += cmd.Length;
            }

            return (nextX, nextY);
        }


        void OutputData(string filepath)
        {
            using (StreamWriter writeText = new StreamWriter(filepath))
            {

                foreach (List<Day18Element> row in _data)
                {
                    foreach (Day18Element element in row)
                    {
                        writeText.Write(element.DrawWith);
                        //if (element.ElementType==Day18ElementType.Border)
                        //{
                        //    Console.Write("#");
                        //}
                        //else
                        //{
                        //    Console.Write(" ");
                        //}
                    }
                    writeText.WriteLine();
                }
            }
        }

        void DetermineSize()
        {
            int westX = 0;
            int eastX= 0;
            int northY= 0;
            int southY = 0; 
            int digX = 0;
            int digY = 0;
            foreach(Day18Command cmd in _cmds)
            {
                switch(cmd.Command)
                {
                    case 'l':
                    case 'L':
                        digX -= cmd.Length;
                        if (digX<westX)
                        {
                            westX = digX;
                        }
                        break;
                    case 'r':
                    case 'R':
                        digX += cmd.Length;
                        if (digX> eastX)
                        {
                            eastX = digX;
                        }
                        break;
                    case 'u':
                    case 'U':
                        digY -= cmd.Length;
                        if (digY<northY)
                        {
                            northY = digY;
                        }
                        break;
                    case 'd':
                    case 'D':
                        digY += cmd.Length;
                        if (digY>southY)
                        {
                            southY = digY;
                        }
                        break;
                }
            }

            Console.WriteLine($"({westX},{northY}) -> ({eastX},{southY})");

            _width = eastX - westX + 1;
            _digX = -westX;
            _height = southY - northY + 1;
            _digY = -northY;

            for(int y =0; y < _height; ++y)
            {
                _data.Add(new List<Day18Element>());
                for(int x=0;x<_width; ++x)
                {
                    _data[y].Add(new Day18Element());
                }
            }

            Console.WriteLine($"Size {_width} x {_height}");
            Console.WriteLine($"Start dig at ({_digX}, {_digY})");
        }

        void DigTrench()
        {
            for(int cmdIndex = 0;cmdIndex<_cmds.Count;++cmdIndex)
            { 
                Day18Command cmd = _cmds[cmdIndex];
                Day18Command nextCmd = _cmds[(cmdIndex + 1) % _cmds.Count];
                switch (cmd.Command)
                {
                    case 'l':
                    case 'L':
                        DigTrenchAt(_digX - cmd.Length, _digY, _digX - 1, _digY, cmd.Colour, '<');
                        if (nextCmd.Command == 'U')
                        {
                            SetTrenchTo(_digX - cmd.Length, _digY, 'L', Day18TrenchType.NorthToEast);
                        }
                        else
                        {
                            SetTrenchTo(_digX - cmd.Length, _digY, 'F', Day18TrenchType.SouthToEast);
                        }
                        _digX -= cmd.Length;

                        break;
                    case 'r':
                    case 'R':
                        DigTrenchAt(_digX +1, _digY, _digX +cmd.Length, _digY, cmd.Colour, '>');
                        if (nextCmd.Command == 'U')
                        {
                            SetTrenchTo(_digX + cmd.Length, _digY, 'J', Day18TrenchType.NorthToWest);
                        }
                        else
                        {
                            SetTrenchTo(_digX + cmd.Length, _digY, '7', Day18TrenchType.SouthToWest);
                        }
                        _digX += cmd.Length;
                        break;
                    case 'u':
                    case 'U':
                        DigTrenchAt(_digX, _digY-cmd.Length, _digX, _digY-1,cmd.Colour, '^');
                        if (nextCmd.Command == 'L')
                        {
                            SetTrenchTo(_digX, _digY - cmd.Length, '7', Day18TrenchType.SouthToWest);
                        }
                        else
                        {
                            SetTrenchTo(_digX, _digY - cmd.Length, 'F', Day18TrenchType.SouthToEast);
                        }
                        _digY -= cmd.Length;
                        break;
                    case 'd':
                    case 'D':
                        DigTrenchAt(_digX, _digY + 1, _digX, _digY + cmd.Length, cmd.Colour, 'v');
                        if (nextCmd.Command == 'L')
                        {
                            SetTrenchTo(_digX, _digY + cmd.Length, 'J', Day18TrenchType.NorthToWest);
                        }
                        else
                        {
                            SetTrenchTo(_digX, _digY + cmd.Length, 'L', Day18TrenchType.NorthToEast);
                        }
                        _digY += cmd.Length;
                        break;
                }
            }
        }

        void SetTrenchTo(int x, int y, char representWith, Day18TrenchType trenchType)
        {
            _data[y][x].DrawWith = representWith;
            _data[y][x].TrenchType = trenchType;
        }

        void DigTrenchAt(int lx, int ty, int rx, int by, string colour, char representTrenchWith)
        {
            if (ty==by)
            {
                for(int x=lx; x<=rx;++x)
                {
                    _data[ty][x].Colour = colour;
                    _data[ty][x].ElementType = Day18ElementType.Trench;
                    _data[ty][x].Depth = -1;
                    _data[ty][x].DrawWith = representTrenchWith;
                    _data[ty][x].TrenchType = Day18TrenchType.EastToWest;
                }
            }
            else
            {
                for(int y=ty;y<=by;++y)
                {
                    _data[y][lx].Colour = colour;
                    _data[y][lx].ElementType = Day18ElementType.Trench;
                    _data[y][lx].Depth = -1;
                    _data[y][lx].DrawWith = representTrenchWith;
                    _data[y][lx].TrenchType = Day18TrenchType.NorthToSouth;
                }
            }
        }

        void MarkInsideSpaces()
        {
            for (int y = 0; y < _height; ++y)
            {
                Day18TrenchType searchFor = Day18TrenchType.None;
                bool inside = false;

                for (int x = 0; x < _width; ++x)
                {
                    Day18TrenchType trenchType = _data[y][x].TrenchType;
                    if (searchFor!=Day18TrenchType.None)
                    {
                        if (trenchType == Day18TrenchType.EastToWest)
                        {
                            continue;
                        } 
                        else if (trenchType == searchFor)
                        {
                            inside = !inside;
                            searchFor = Day18TrenchType.None;
                        }
                        else
                        {
                          //  Console.WriteLine($"({x},{y}) found {trenchType}, looking for {searchFor}");
                            searchFor = Day18TrenchType.None;
                        }
                    }
                    else if (trenchType == Day18TrenchType.NorthToSouth)
                    {
                        inside = !inside;
                    }
                    else if (trenchType == Day18TrenchType.SouthToEast)
                    {
                        searchFor = Day18TrenchType.NorthToWest;
                       // Console.WriteLine($"({x},{y}) found {_data[y][x].DrawWith}, looking for {searchFor}");
                    }
                    else if (trenchType == Day18TrenchType.NorthToEast)
                    {
                        searchFor = Day18TrenchType.SouthToWest;
                      //  Console.WriteLine($"({x},{y}) found {_data[y][x].DrawWith}, looking for {searchFor}");
                    }
                    else if (_data[y][x].ElementType == Day18ElementType.Unknown && inside)
                    {
                        _data[y][x].ElementType = Day18ElementType.Inside;
                        _data[y][x].DrawWith = '.';
                    }
                }
            }
        }
        Int64 CountTrenchDepth(bool outputToConsole)
        {
            Int64 count = 0;
            for (int y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    if (_data[y][x].ElementType != Day18ElementType.Unknown)
                    {
                        if (outputToConsole)
                        {
                            Console.Write("#");
                        }
                        count++;
                    }
                    else
                    {
                        if (outputToConsole)
                        {
                            Console.Write(" ");
                        }
                    }
                }
                if (outputToConsole)
                {
                    Console.WriteLine();
                }
            }
            return count;
        }
    }
}
