using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    // Part 1: 25261
    // Part 2: 549873212220117

    class D2Vector
    {

        internal Int64 X {  get; set; }
        internal Int64 Y { get; set; }
        internal Int64 Z { get; set; }

        internal D2Vector(Int64 x, Int64 y, Int64 z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static D2Vector operator-(D2Vector lhs, D2Vector rhs)
        {
            return new D2Vector(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        public static D2Vector operator +(D2Vector lhs, D2Vector rhs)
        {
            return new D2Vector(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        public static bool operator!=(D2Vector lhs, D2Vector rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator==(D2Vector lhs, D2Vector rhs)
        {
            if (lhs is null && rhs is null)
            {
                return true;
            }
            else if (lhs is null || rhs is null)
            {
                return false;
            }
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
        }

        public static D2Vector operator *(D2Vector lhs, Int64 rhs)
        {
            return new D2Vector(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        }

        public override string ToString()
        {
            return $"({X},{Y},{Z})";
        }

        internal bool IsZero()
        {
            return X == 0 && Y == 0 && Z == 0;
        }
    }
    class D24Coord
    {
        internal D2Vector Coord { get; set; }
        internal D24Coord(D2Vector vector)
        {
            Coord = vector;
        }
        internal D24Coord(Int64 x64, Int64 y64, Int64 z64)
        {
            Coord = new D2Vector(x64, y64, z64);
        }

        internal D24Coord(string coordAsString)
        {
            string[] elements = coordAsString.Split(',');

            Int64 x64 = Int64.Parse(elements[0].Trim());
            Int64 y64 = Int64.Parse(elements[1].Trim());
            Int64 z64 = Int64.Parse(elements[2].Trim());

            Coord = new D2Vector(x64, y64, z64);
        }

        public override string ToString()
        {
            return $"({Coord.X},{Coord.Y},{Coord.Z})";
        }
    }

    class Hailstone
    {
        internal D24Coord Coord { get; set; }
        internal D24Coord Velocity { get; set; }

        internal D24Coord AdjustedVelocity { get; set; }

        internal Int64 X64 { get { return Coord.Coord.X; } }
        internal Int64 Y64 { get { return Coord.Coord.Y; } }
        internal Int64 Z64 { get { return Coord.Coord.Z; } }

        internal Int64 VX64 { get { return Velocity.Coord.X; } }
        internal Int64 VY64 { get { return Velocity.Coord.Y; } }
        internal Int64 VZ64 { get { return Velocity.Coord.Z; } }

        internal Int64 VXAdjusted { get { return AdjustedVelocity.Coord.X; } }
        internal Int64 VYAdjusted { get { return AdjustedVelocity.Coord.Y; } }
        internal Int64 VZAdjusted { get { return AdjustedVelocity.Coord.Z; } }

        internal Hailstone(string line)
        {
            string[] elements = line.Split(" @ ");
            Coord = new D24Coord(elements[0]);
            Velocity = new D24Coord(elements[1]);
            AdjustedVelocity = Velocity;
        }

        internal void OffsetWithRockVelocity(D2Vector rockVelocity)
        {
            AdjustedVelocity = new D24Coord(Velocity.Coord - rockVelocity);
        }

        internal void OffsetWithRockVelocity(int vx, int vy, int vz)
        {
            AdjustedVelocity = new D24Coord(Velocity.Coord.X - vx, Velocity.Coord.Y - vy, Velocity.Coord.Z - vz);
        }


        public override string ToString()
        {
            return $"{Coord}, {Velocity}";
        }
    }
    internal class Day24 : DayBase, IDay
    {
        List<Hailstone> _stones = new List<Hailstone>();
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
            _stones.Add(new Hailstone(line));
        }

        void ExecutePart1()
        {
            float lowerBounds = 7;
            float upperBounds = 27;

            lowerBounds = 200000000000000;
            upperBounds = 400000000000000;
            int index = 1;
            int count = 0;
            for(int h1Index=0;h1Index< _stones.Count;++h1Index)
            {
                Hailstone stone1 = _stones[h1Index];
                for (int h2Index=h1Index+1;h2Index< _stones.Count; ++h2Index) {
                    Hailstone stone2 = _stones[h2Index];
                    Console.WriteLine($"{index} Checking stone {stone1} against {stone2}");
                    (D2Vector? v, double t) = HailstoneXYIntersection( stone1, stone2 );
                    double tStone2 = 0;
                    if (t>=0 && 
                        v!.X >= lowerBounds && v!.Y >= lowerBounds && v!.X <= upperBounds && v!.Y <= upperBounds)
                    {
                        tStone2 = CalculateTimeStoneReachesPosition(stone2, v);
                        if (tStone2>0)
                        {
                            count++;
                        }
                    }

                    ++index;
                }
            }
            Console.WriteLine($"{count} stones crossed correctly");
        }

        void ExecutePart2()
        {
            int range = 300;

            foreach(int rockVY in Range(300))
            {
                foreach (int rockVX in Range(range))
                {
                    (bool intersects1 , D2Vector? v1, Int64 t1) = HailstoneIntersection_AdjustVelocity(_stones[1], _stones[0], rockVX, rockVY);
                    (bool intersects2, D2Vector? v2, Int64 t2) = HailstoneIntersection_AdjustVelocity(_stones[2], _stones[0], rockVX, rockVY);
                    (bool intersects3, D2Vector? v3, Int64 t3) = HailstoneIntersection_AdjustVelocity(_stones[3], _stones[0], rockVX, rockVY);

                    if (intersects1 && v1! == v2! && v1! == v3!)
                    {
                        foreach(int rockVZ in Range(range))
                        {
                            Int64 intersectZ1 = _stones[1].Z64 + t1 * (_stones[1].VZ64+ rockVZ);
                            Int64 intersectZ2 = _stones[2].Z64 + t2 * (_stones[2].VZ64 + rockVZ);
                            Int64 intersectZ3 = _stones[3].Z64 + t3 * (_stones[3].VZ64 + rockVZ);
                            if (intersectZ1==intersectZ2 && intersectZ2==intersectZ3)
                            {
                                Console.WriteLine($"Found result.  Velocity ({rockVX},{rockVY},{rockVZ})");
                                Int64 sum1 = v1!.X + v1!.Y + intersectZ1;
                                Int64 sum2 = v2!.X + v2!.Y + intersectZ2;
                                Int64 sum3 = v3!.X + v3!.Y + intersectZ3;
                                Console.WriteLine($"               Position ({v1.X},{v1.Y},{intersectZ1}): {sum1}");
                                Console.WriteLine($"               Position ({v2.X},{v2.Y},{intersectZ2}): {sum2}");
                                Console.WriteLine($"               Position ({v3.X},{v3.Y},{intersectZ3}): {sum3}");
                                return;
                            }
                        }
                    }
                }
            }


            Console.WriteLine("Could not find a valid position");
        }

        public IEnumerable<int> Range(int max)
        {
            int i = 0;
            yield return i;

            while (i < max)
            {
                if (i >= 0)
                {
                    i++;
                }
                i *= -1;
                yield return i;
            }
        }

        (D2Vector?, double) HailstoneXYIntersection(Hailstone stone1, Hailstone stone2)
        {
            Int64 dx = stone1.X64 - stone2.X64;
            Int64 dy = stone1.Y64 - stone2.Y64;

            double divisor = (stone2.VX64 * stone1.VY64 - stone1.VX64 * stone2.VY64);
            if (divisor ==0)
            {
                return (null, -1);
            }
            double k = (stone2.VY64 * dx - stone2.VX64 * dy) / divisor;

            D2Vector toReturn = new D2Vector((Int64)(stone1.X64+ k * stone1.VX64), (Int64)(stone1.Y64 + k * stone1.VY64), 0);

            return (toReturn, k);
        }

        (bool, D2Vector?, Int64) HailstoneIntersection_AdjustVelocity(Hailstone stone1, Hailstone stone2, int rockVX, int rockVY)
        {
            Int64 dx = stone1.X64 - stone2.X64;
            Int64 dy = stone1.Y64 - stone2.Y64;

            stone1.OffsetWithRockVelocity(rockVX, rockVY, 0);
            stone2.OffsetWithRockVelocity(rockVX, rockVY, 0);

            Int64 divisor = (stone2.VXAdjusted * stone1.VYAdjusted - stone1.VXAdjusted * stone2.VYAdjusted);
            if (divisor==0)
            {
                return (false, null, 0);
            }
            double k = (stone2.VYAdjusted * dx - stone2.VXAdjusted * dy) / divisor;
            if (double.IsNaN(k) || double.IsInfinity(k) || double.IsNegativeInfinity(k))
            {
                return (false, null, (Int64)k);
            }

            D2Vector toReturn = new D2Vector((Int64)(stone1.X64 + k * stone1.VXAdjusted), (Int64)(stone1.Y64 + k * stone1.VYAdjusted), 0);

            return (true, toReturn, (Int64)k);
        }

        double CalculateTimeStoneReachesPosition(Hailstone stone, D2Vector v)
        {
            if (stone.VX64!=0)
            {
                return (v.X - stone.X64) / stone.VX64;
            }
            else
            {
                return (v.Y - stone.Y64) / stone.VY64;
            }
        }
    }
}
