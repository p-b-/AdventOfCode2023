using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    internal class Day9 : DayBase, IDay
    {
        // Part 1: 1877825184
        // Part 2: 1108
        List<List<Int64>> linesOfTestResults = new List<List<Int64>>();
        internal Day9()
        {
        }

        public void Execute(string filepath, int part)
        {
            ReadTextData(filepath, ParseLine, false);
            Int64 sum = 0;
            foreach(List<Int64> line in linesOfTestResults)
            {
                if (part == 0)
                {
                    sum += Extrapolate(line, true);
                }
                else
                {
                    sum += Extrapolate(line, false);
                }
            }
            Console.WriteLine($"Result is {sum}");
        }

        Int64 Extrapolate(List<Int64> inputData, bool forwardsNotBackwards)
        {
            List<List<Int64>> processing = new List<List<Int64>>();

            processing.Add(inputData);

            int lineIndex = 0;
            bool loop = true;
            while (loop)
            {
                ++lineIndex;
                List<Int64> processFrom = processing[lineIndex - 1];
                List<Int64> processInto = new List<Int64>();
                processing.Add(processInto);

                bool allZero = true;
                for (int i = 1; i < processFrom!.Count; i++)
                {
                    Int64 diff = processFrom![i] - processFrom![i - 1];
                    if (diff != 0)
                    {
                        allZero = false;
                    }
                    processInto.Add(diff);
                }
                if (allZero)
                {
                    loop = false;
                }
            }
            if (forwardsNotBackwards)
            {
                Int64 newDatum = ExtrapolateLastElement(processing);
                Console.WriteLine($"{newDatum}");

                //foreach (List<Int64> line in processing)
                //{
                //    foreach (Int64 element in line)
                //    {
                //        Console.Write($"{element}  ");
                //    }
                //    Console.WriteLine();
                //}
                return newDatum;
            }
            else
            {
                Int64 newDatum = ExtrapolateFirstElement(processing);
                Console.WriteLine($"{newDatum}");

                return newDatum;
            }
        }

        Int64 ExtrapolateLastElement(List<List<Int64>> processedData)
        {
            for(int lineIndex = processedData.Count-2; lineIndex >= 0;--lineIndex)
            {
                Int64 addOnto = processedData[lineIndex].Last();
                Int64 addOn = processedData[lineIndex + 1].Last();
                processedData[lineIndex].Add(addOnto + addOn);
            }

            return processedData[0].Last();
        }

        Int64 ExtrapolateFirstElement(List<List<Int64>> processedData)
        {
            for (int lineIndex = processedData.Count - 2; lineIndex >= 0; --lineIndex)
            {
                Int64 addOnto = processedData[lineIndex][0];
                Int64 addOn = -processedData[lineIndex + 1][0];
                processedData[lineIndex].Insert(0,addOnto + addOn);
            }

            return processedData[0][0];
        }

        void ParseLine(string line)
        {
            if (String.IsNullOrEmpty(line) ||
                String.IsNullOrEmpty(line.Trim()))
            {
                return;
            }
            List<Int64> elements = SplitStringIntoNumberList(line, ' ');
            linesOfTestResults.Add(elements);
        }
    }
}
