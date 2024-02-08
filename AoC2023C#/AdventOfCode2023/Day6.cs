using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    // Answer 1 771628
    // Answer 2 27363861
    class RaceData
    {
        internal Int64 TimeAllowed { get; private set; }
        internal Int64 CurrentDistanceRecord {  get; private set; }
        internal RaceData(Int64 time, Int64 currentDistRecord)
        {
            TimeAllowed = time;
            CurrentDistanceRecord = currentDistRecord;
        }
    }
    internal class Day6
    {
        List<RaceData> _races = new List<RaceData>();

        string _timeLine = String.Empty;

        internal void Run(string filepath)
        {
            ReadTextData(filepath);

            Int64 result = ProcessRaces();
            Console.WriteLine($"Winning races is {result}");
        }

        Int64 ProcessRaces()
        {
            Int64? winningProduct = null;
            foreach (RaceData race in _races)
            {
                double plusOrMinusBit = Math.Sqrt(race.TimeAllowed * race.TimeAllowed - 4 * (race.CurrentDistanceRecord+0.1)) / 2;
                double minusBBit = race.TimeAllowed / 2.0;

                double d1 = minusBBit + plusOrMinusBit;
                double d2 = minusBBit - plusOrMinusBit;
                int from = (int)Math.Ceiling(d2);
                int to = (int)Math.Floor(d1);

                int racesThatWin = to - from + 1;
                if (winningProduct==null)
                {
                    winningProduct = racesThatWin;
                }
                else
                {
                    winningProduct *= racesThatWin;
                }
                //Console.WriteLine($"Races that win: {racesThatWin}");
            }
            return winningProduct.GetValueOrDefault();
        }

        void ReadTextData(string filepath)
        {
            string? line;
            StreamReader sr = new StreamReader(filepath);

            line = sr.ReadLine();
            while (line != null)
            {
                ParseLine(line.ToLower());
                line = sr.ReadLine();

            }
            sr.Close();
        }

        void ParseLine(string line)
        {
            string[] splitOnColon = line.Split(':');
            if (splitOnColon[0]=="time")
            {
                _timeLine = splitOnColon[1].Trim();
            }
            else
            {
                ParseTimeAndDistances(_timeLine, splitOnColon[1].Trim());
            }
        }

        void ParseTimeAndDistances(string time, string distance)
        {
            List<Int64> timeIntegers = SplitIntoInts(time);
            List<Int64> distIntegers = SplitIntoInts(distance);

            if (timeIntegers.Count!=distIntegers.Count)
            {
                throw new Exception($"Unmatched input: {timeIntegers.Count} time integers, and {distIntegers.Count} distance integers.");
            }
            for(int i=0;i<timeIntegers.Count;++i)
            {
                Int64 timeInt = timeIntegers[i];
                Int64 distInt = distIntegers[i];
                _races.Add(new RaceData(timeInt, distInt));
            }
        }

        List<Int64> SplitIntoInts(string toSplit)
        {
            List<Int64> toReturn = new List<Int64>();
            string[] splitArray = toSplit.Split(' ');

            foreach (string str in splitArray)
            {
                String strTrimmed = str.Trim();
                if (strTrimmed.Length>0)
                {
                    Int64 strAsInt = Int64.Parse(strTrimmed);
                    toReturn.Add(strAsInt);
                }
            }
            return toReturn;
        }
    }
}
