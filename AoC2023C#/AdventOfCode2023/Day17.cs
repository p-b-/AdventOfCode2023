using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    // Part 1: 1155
    // Part 2: 1286 is too high
    //         1283
    enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    class Day17Node
    {
        internal int X { get; set; }
        internal int Y { get; set; }
        internal Direction Direction { get; set; }
        internal int StepsTaken { get; set; }

        internal Day17Node(int x, int y, Direction dirToNode, int steps) 
        {
            X = x;
            Y = y;
            Direction = dirToNode;
            StepsTaken = steps;
        }
    }

  
    internal class Day17 : DayBase, IDay
    {
        int? _width;
        int _height;
        List<List<Day17Node>> _nodes = new List<List<Day17Node>>();
        List<List<int>> _data = new List<List<int>>();

        PriorityQueue<Day17Node, int> _queue = new System.Collections.Generic.PriorityQueue<Day17Node, int> ();
        HashSet<string> _visited = new HashSet<string>();

        public void Execute(string filepath, int part)
        {
            ReadTextData(filepath, ParseLine, false);

            if (part == 0)
            {
                ExecutePart1();
            } 
            else
            {
                ExecutePart2();
            }
        }

        void ParseLine(string line)
        {
            _data.Add(new List<int>());
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }
            if (_width == null)
            {
                _width = line.Length;
            }
            for (int n = 0; n < line.Length; ++n)
            {
                int v = Int32.Parse(line.Substring(n, 1));
                _data[_height].Add(v);
            }
            ++_height;
        }

        void ExecutePart1()
        {
            int n = Solve(0,3);

            Console.WriteLine($"Lowest cost is {n}");
        }

        void ExecutePart2()
        {
            int n = Solve(4, 10);

            Console.WriteLine($"Lowest cost is {n}");
        }

        int Solve(int minSteps, int maxSteps)
        {
            this._queue.Enqueue(new Day17Node(0, 0, Direction.Right, maxSteps-1), 0);
            this._queue.Enqueue(new Day17Node(0, 0, Direction.Down, maxSteps - 1), 0);

            var directions = new List<Direction>();

            while (this._queue.TryDequeue(out Day17Node? node, out int cost))
            {
                if (node.X == _width - 1 && node.Y == _height - 1 && node.StepsTaken >= minSteps - 1)
                {
                    return cost;
                }
                directions.Clear();

                if (node.StepsTaken < minSteps - 1)
                {
                    // Can't change direction before moving the minimum number of steps
                    directions.Add(node.Direction);
                }
                else
                {
                    GetDirections(directions, node.Direction);
                }


                foreach(Direction dir in directions)
                {
                    int stepsTakenInDirection = 0;
                    if (dir == node.Direction)
                    {
                        // Continuing in the same direction as the node
                        stepsTakenInDirection = node.StepsTaken + 1;
                    }
                    if (stepsTakenInDirection == maxSteps)
                    {
                        continue;
                    }

                    (int x, int y) = MoveInDirection(node.X, node.Y, dir);
                    if (x<0 || x==_width || y<0 || y==_height)
                    {
                        continue;
                    }
                   // Console.WriteLine($" From node({node.X},{node.Y}) move {dir} to ({x},{y})");

                    string key = $"{node.X},{node.Y},{x},{y},{stepsTakenInDirection}";
                    if (_visited.Add(key))
                    {
                        int costToQueue = cost + _data[y][x];
                      //  Console.WriteLine($" Enqueue new node at {x},{y},{dir},{stepsTakenInDirection} with cost {costToQueue}");
                        _queue.Enqueue(new Day17Node(x, y, dir, stepsTakenInDirection), costToQueue);
                    }
                }
            }
            return 0;
        }

        (int x, int y) MoveInDirection(int currentX, int currentY, Direction d)
        {
            switch (d)
            {
                case Direction.Right:
                    return (currentX + 1, currentY);
                case Direction.Down:
                    return (currentX, currentY + 1);
                case Direction.Left:
                    return (currentX - 1, currentY);
                case Direction.Up:
                    return (currentX, currentY - 1);
            }
            throw new Exception("Unknown direction");
        }


        void GetDirections(List<Direction> directions, Direction nodeDirection)
        {
            if (nodeDirection != Direction.Down)
            {
                directions.Add(Direction.Up);
            }
            if (nodeDirection != Direction.Right)
            {
                directions.Add(Direction.Left);
            }
            if (nodeDirection != Direction.Up)
            {
                directions.Add(Direction.Down);
            }
            if (nodeDirection != Direction.Left)
            {
                directions.Add(Direction.Right);
            }
        }
    }
}
