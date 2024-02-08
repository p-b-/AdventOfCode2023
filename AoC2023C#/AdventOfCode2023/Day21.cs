using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AdventOfCode2023
{
    // Part 1: 3532
    // Part 2: 590104708070703
    enum Day21Dir
    {
        Up,
        Down, 
        Left,
        Right
    }

    class Position
    {
        internal int PrevX { get; set; }
        internal int PrevY { get; set; }
        internal int X { get; set; }
        internal int Y { get; set; }

        internal Position(int prevX, int prevY, int x, int y)
        {
            PrevX = prevX;
            PrevY = prevY;
            X = x;
            Y = y;
        }

        internal Position? Move(Day21Dir dir)
        {
            Position toReturn = dir switch
            {
                Day21Dir.Up => MoveUp(),
                Day21Dir.Down => MoveDown(),
                Day21Dir.Left => MoveLeft(),
                Day21Dir.Right => MoveRight(),
                _ => throw new Exception("Unknown direction")
            };

            if (toReturn.X == PrevX && toReturn.Y == PrevY)
            {
                return null;
            }
            return toReturn;
        }

        public Position MoveUp()
        {
            return new Position(X,Y,X, Y - 1);
        }

        public Position MoveDown()
        {
            return new Position(X,Y, X, Y + 1);
        }

        public Position MoveLeft()
        {
            return new Position(X,Y, X-1, Y);
        }

        public Position MoveRight()
        {
            return new Position(X,Y, X + 1, Y);
        }

        public override bool Equals(object? other)
        {
            if (other == null)
            {
                return false;
            }
            return other is Position p
                && p.X == X
                && p.Y == Y;
        }

        public override int GetHashCode()
        {
            unchecked 
            {
                int hashcode = 1430287;
                hashcode = hashcode * 7302013 ^ X.GetHashCode();
                hashcode = hashcode * 7302013 ^ Y.GetHashCode();
                return hashcode;
            }
        }
    }

    internal class Day21 : DayBase, IDay
    {
        List<string> _data = new List<string>();
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
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }
            _data.Add(line);
        }

        void ExecutePart1()
        {

        }

        void ExecutePart2()
        {
            const long totalSteps = 26501365L;

            var sequenceCounts = new List<(int X, int Y)>();
            var startPosition = _data.Count() / 2; // 65
            var start = new Position(startPosition, startPosition, startPosition, startPosition);
            var visitedPositions = new HashSet<Position> { start };
            var steps = 0;
            HashSet<Position> toVisit = new HashSet<Position> ();

            HashSet<Position> futurePositions = new HashSet<Position>();
            for (var run = 0; run < 3; run++)
            {
                for (; steps < run * 131 + 65; steps++) // First run is 65 steps, the rest are 131 each.
                {
                    var nextOpen = futurePositions;
                    futurePositions = new HashSet<Position>();
                    foreach (Position? visitedPos in visitedPositions)
                    {
                        foreach (Day21Dir dir in new[] { Day21Dir.Up, Day21Dir.Down, Day21Dir.Left, Day21Dir.Right })
                        {
                            Position? dest = visitedPos!.Move(dir);
                            if (dest == null)
                            { 
                                continue; 
                            }
                            int modX = ModToGridSize(dest!.X);
                            int modY = ModToGridSize(dest!.Y);
                            if (_data[modY][modX] != '#')
                            {
                                nextOpen.Add(dest);
                                // If we allowed backtracking, we would come back to the position visitedPos in the next iteration
                                //  However, backtracking is disallowed by preventing the .Move method from returning to a previous position
                                // To take this into account, we add this position to the futurePositions, and start from that set of open positions
                                //  instead of a null set
                                futurePositions.Add(visitedPos);
                            }
                        }
                    }

                    visitedPositions = nextOpen;

                    if (steps == 63)
                    {
                        Console.WriteLine($"Part 1: {visitedPositions.Count}");
                    }
                }
                sequenceCounts.Add((steps, visitedPositions.Count));
            }

            double result = LangrangeInterpolation(totalSteps, sequenceCounts);

            Console.WriteLine($"Part 2: {result}");

        }

        private static double LangrangeInterpolation(long totalSteps, List<(int X, int Y)> sequenceCounts)
        {
            double result = 0;

            for (var i = 0; i < 3; i++)
            {
                Console.WriteLine($"Sequence {i} = {sequenceCounts[i].X}, {sequenceCounts[i].Y}");
            }

            for (var i = 0; i < 3; i++)
            {
                // Compute individual terms of formula
                double term = sequenceCounts[i].Y;

                for (var j = 0; j < 3; j++)
                {
                    if (j != i)
                    {
                        term = term * (totalSteps - sequenceCounts[j].X) / (sequenceCounts[i].X - sequenceCounts[j].X);
                    }
                }

                // Add current term to result
                result += term;
            }

            return result;
        }

        static int ModToGridSize(int number)
        {
            var toReturn =  ((number % 131) + 131) % 131;
            return toReturn;
        }
    }
}
