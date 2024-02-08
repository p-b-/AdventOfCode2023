using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    internal class DayBase
    {
        protected void ReadTextData(string filepath, Action<string> parseLine, bool makeLowerCase = true)
        {
            string? line;
            StreamReader sr = new StreamReader(filepath);

            line = sr.ReadLine();
            while (line != null)
            {
                if (makeLowerCase)
                {
                    parseLine(line.ToLower());
                }
                else
                {
                    parseLine(line);
                }
                
                line = sr.ReadLine();

            }
            sr.Close();
        }

        protected List<Int64> SplitStringIntoNumberList(string separatedNumbers, char separator)
        {
            List<Int64> result = new List<Int64>();
            if (!String.IsNullOrEmpty(separatedNumbers) && !String.IsNullOrEmpty(separatedNumbers.Trim()))
            {
                separatedNumbers = separatedNumbers.Trim();
                string[] numericElements = separatedNumbers.Split(separator);
                foreach(string element in numericElements)
                {
                    if (!String.IsNullOrEmpty(element.Trim()))
                    {
                        result.Add(Int64.Parse(element));
                    }
                }
            }

            return result;
        }
    }
}
