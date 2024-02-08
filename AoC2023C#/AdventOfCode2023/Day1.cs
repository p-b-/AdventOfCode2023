using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    internal class Day1
    {
        internal void Run(string filepath)
        {
            List<string> data = ReadTextData(filepath);
            int part1Sum = 0;
            foreach(string line in data)
            {
                part1Sum += ProcessLineForElvesOnlyDigits(line);
            }
            Console.WriteLine($"Elven output part 1 {part1Sum}");
            int part2Sum = 0;
            foreach (string line in data)
            {
                int sumPart = ProcessLineForElvesTextNumbersAndDigits(line);
                part2Sum += sumPart; 
            }

            Console.WriteLine($"Elven output part 2 {part2Sum}");
        }

        int ProcessLineForElvesOnlyDigits(string line)
        {
            char[] numberDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int index = line.IndexOfAny(numberDigits);
            if (index == -1)
            {
                return 0;
            }
            string digits = line.Substring(index, 1);
            index = line.LastIndexOfAny(numberDigits);
            digits+=  line.Substring(index, 1);
            return int.Parse(digits);
        }

        int ProcessLineForElvesTextNumbersAndDigits(string line)
        {
            line = line.ToLower();

            char[] numberDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            string[] textualDigits = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            int digit1 = GetDigitFromText(line, numberDigits, textualDigits, searchForwardsNotBackwards: true);
            int digit2 = GetDigitFromText(line, numberDigits, textualDigits, searchForwardsNotBackwards: false);
            return digit1 * 10 + digit2;
        }

        int GetDigitFromText(string inString, char[] numberDigits, string[] textualDigits, bool searchForwardsNotBackwards)
        {
            int digitIndex;
            if (searchForwardsNotBackwards)
            {
                digitIndex = inString.IndexOfAny(numberDigits);
            }
            else
            {
                digitIndex = inString.LastIndexOfAny(numberDigits);
            }
            int foundDigit = 0;

            if (digitIndex!=-1)
            {
                string digits = inString.Substring(digitIndex, 1);
                foundDigit = int.Parse(digits);
            }

            int? foundTextIndex = null;
            int? textIndexFoundAtElement = null;
            for (int i = 0;i<textualDigits.Length;++i)
            {
                string find = textualDigits[i];
                int findIndex;
                if (searchForwardsNotBackwards)
                {
                    findIndex = inString.IndexOf(find);
                }
                else
                {
                    findIndex = inString.LastIndexOf(find);
                }
                if (findIndex!=-1)
                {
                    if (foundTextIndex==null ||
                        (searchForwardsNotBackwards && foundTextIndex.Value > findIndex) ||
                        (!searchForwardsNotBackwards && foundTextIndex.Value < findIndex))
                    {
                        foundTextIndex = findIndex;
                        textIndexFoundAtElement = i;
                    }
                }
            }

            if (digitIndex==-1 && textIndexFoundAtElement.HasValue) 
            {
                return textIndexFoundAtElement.Value;
            }
            else if (digitIndex!=-1 && textIndexFoundAtElement == null)
            {
                return foundDigit;
            }
            else if (digitIndex!=-1 && textIndexFoundAtElement != null)
            {
                if (searchForwardsNotBackwards) 
                {
                    return foundTextIndex < digitIndex ? textIndexFoundAtElement.Value: foundDigit;
                }
                else
                {
                    return foundTextIndex > digitIndex ? textIndexFoundAtElement.Value : foundDigit;
                }
            }

            return 0;
        }

        List<string> ReadTextData(string filepath, int maxLines = 0)
        {
            List<string> toReturn = new List<string>();
            string? line;
            StreamReader sr = new StreamReader(filepath);
            line = sr.ReadLine();
            while (line != null)
            {
                toReturn.Add(line);
                line = sr.ReadLine();
                if (maxLines > 0 && toReturn.Count == maxLines)
                {
                    break;
                }
            }
            sr.Close();

            return toReturn;
        }
    }
}
