using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdventOfCode2023
{
    internal class Day3
    {
        List<string> _data = new List<string>();
        int _width;
        int _height;

        internal void Run(string filepath)
        {
            _data = ReadTextData(filepath);
            if (!CalculateWidthAndHeight())
            {
                Console.WriteLine("Input data is invalid");
                return;
            }

            int sum = ProcessData_2();
            Console.WriteLine($"Sum is {sum}");
        }

        bool CalculateWidthAndHeight()
        {
            _height = _data.Count();
            if (_height == 0)
            {
                return false;
            }
            if (String.IsNullOrEmpty(_data[0]))
            {
                return false;
            }
            _width = _data[0].Length;

            return true;
        }

        int ProcessData()
        {
            int sum = 0;
            for (int rowIndex = 0; rowIndex < _height; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < _width; ++colIndex)
                {
                    char element = GetElement(colIndex, rowIndex);
                    if (element=='.')
                    {
                        continue;
                    }
                    if (IsNumeric(element))
                    {
                        continue;
                    }
                    // Element is a symbol
                    if (rowIndex>0)
                    {
                        sum+=FindAdjacentNumbersInRowAboveOrBelow(colIndex, rowIndex, -1);
                    }
                    if (rowIndex<_height-1)
                    {
                        sum += FindAdjacentNumbersInRowAboveOrBelow(colIndex, rowIndex, 1);
                    }
                    if (colIndex>0)
                    {
                        sum += FindAdjacentNumbersToLeft(colIndex, rowIndex);
                    }
                    if (colIndex<_width-1)
                    {
                        sum += FindAdjacentNumbersToRight(colIndex, rowIndex);
                    }
                }
            }
            return sum;
        }

        int ProcessData_2()
        {
            int sum = 0;
            for (int rowIndex = 0; rowIndex < _height; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < _width; ++colIndex)
                {
                    char element = GetElement(colIndex, rowIndex);
                    if (element == '.')
                    {
                        continue;
                    }
                    if (IsNumeric(element))
                    {
                        continue;
                    }
                    List<int> adjacentNumbers = new List<int>();
                    // Element is a symbol
                    if (rowIndex > 0)
                    {
                        List<int> returnedList = ReturnListOfAdjacentNumbersInRowAboveOrBelow(colIndex, rowIndex, -1);
                        adjacentNumbers.InsertRange(0,returnedList);
                    }
                    if (rowIndex < _height - 1)
                    {
                        List<int> returnedList = ReturnListOfAdjacentNumbersInRowAboveOrBelow(colIndex, rowIndex, 1);
                        adjacentNumbers.InsertRange(0, returnedList);
                    }
                    if (colIndex > 0)
                    {
                        List<int> returnedList = ReturnListOfAdjacentNumbersToLeft(colIndex, rowIndex);
                        adjacentNumbers.InsertRange(0, returnedList);
                    }
                    if (colIndex < _width - 1)
                    {
                        List<int> returnedList = ReturnListOfAdjacentNumbersToRight(colIndex, rowIndex);
                        adjacentNumbers.InsertRange(0, returnedList);
                    }
                    if (adjacentNumbers.Count==2)
                    {
                        int product = 1;
                        foreach(int i in adjacentNumbers)
                        {
                            product *= i;
                        }
                        sum += product;
                    }
                }
            }
            return sum;
        }

        int FindAdjacentNumbersToLeft(int colIndex, int rowIndex)
        {
            int sumToReturn = 0;
            char element = GetElement(colIndex - 1, rowIndex);
            if (IsNumeric(element))
            {
                int number = GetNumber(colIndex - 1, rowIndex);
                sumToReturn += number;
            }

            return sumToReturn;
        }

        int FindAdjacentNumbersToRight(int colIndex, int rowIndex)
        {
            int sumToReturn = 0;
            char element = GetElement(colIndex + 1, rowIndex);
            if (IsNumeric(element))
            {
                int number = GetNumber(colIndex + 1, rowIndex);
                sumToReturn += number;
            }
            return sumToReturn;
        }


        int FindAdjacentNumbersInRowAboveOrBelow(int colIndex, int rowIndex, int dy)
        {
            int sumToReturn = 0;
            rowIndex += dy;
            char element;
            if (colIndex>0)
            {
                element = GetElement(colIndex-1, rowIndex);
                if (IsNumeric(element))
                {
                    int number = GetNumber(colIndex - 1, rowIndex);
                    sumToReturn+= number;
                }
            }
            element = GetElement(colIndex, rowIndex);
            if (IsNumeric(element))
            {
                int number = GetNumber(colIndex, rowIndex);
                sumToReturn += number;
            }
            if (colIndex < _width-1)
            {
                element = GetElement(colIndex + 1, rowIndex);
                if (IsNumeric(element))
                {
                    int number = GetNumber(colIndex + 1, rowIndex);
                    sumToReturn += number;
                }
            }


            return sumToReturn;
        }

        List<int> ReturnListOfAdjacentNumbersToLeft(int colIndex, int rowIndex)
        {
            List<int> toReturn = new List<int>();
            string row = _data[rowIndex];

            char element = GetElement(row, colIndex - 1, rowIndex);
            if (IsNumeric(element))
            {
                (int number,row) = GetNumber(row, colIndex - 1, rowIndex);
                toReturn.Add(number);
            }

            return toReturn;
        }

        List<int> ReturnListOfAdjacentNumbersToRight(int colIndex, int rowIndex)
        {
            List<int> toReturn = new List<int>();
            string row = _data[rowIndex];
            char element = GetElement(row, colIndex + 1, rowIndex);
            if (IsNumeric(element))
            {
                (int number,row) = GetNumber(row, colIndex + 1, rowIndex);
                toReturn.Add(number);
            }
            return toReturn;
        }


        List<int> ReturnListOfAdjacentNumbersInRowAboveOrBelow(int colIndex, int rowIndex, int dy)
        {
            List<int> toReturn = new List<int>();

            rowIndex += dy;
            string row = _data[rowIndex];

            char element;
            if (colIndex > 0)
            {
                element = GetElement(row, colIndex - 1, rowIndex);
                if (IsNumeric(element))
                {
                    (int number, row) = GetNumber(row, colIndex - 1, rowIndex);
                    toReturn.Add(number);
                }
            }
            element = GetElement(row, colIndex, rowIndex);
            if (IsNumeric(element))
            {
                (int number,row) = GetNumber(row, colIndex, rowIndex);
                toReturn.Add(number);
            }
            if (colIndex < _width - 1)
            {
                element = GetElement(row, colIndex + 1, rowIndex);
                if (IsNumeric(element))
                {
                    (int number,row)= GetNumber(row, colIndex + 1, rowIndex);
                    toReturn.Add(number);
                }
            }

            return toReturn;
        }

        int GetNumber(int colIndex, int rowIndex)
        {
            string numberString = "";
            while(colIndex>0)
            {
                char preceedingElement= GetElement(colIndex - 1, rowIndex);
                if (IsNumeric(preceedingElement))
                {
                    --colIndex;
                }
                else
                {
                    break;
                }
            }
            while(colIndex<_width)
            {
                char element = GetElement(colIndex, rowIndex);
                if (IsNumeric(element))
                {
                    numberString += element;
                    ReplaceElement(colIndex, rowIndex, '.');
                }
                else
                {
                    break;
                }

                ++colIndex;
            }

            return int.Parse(numberString);

        }

        (int, string) GetNumber(string row, int colIndex, int rowIndex)
        {
            string numberString = "";
            while (colIndex > 0)
            {
                char preceedingElement = GetElement(row, colIndex - 1, rowIndex);
                if (IsNumeric(preceedingElement))
                {
                    --colIndex;
                }
                else
                {
                    break;
                }
            }
            while (colIndex < _width)
            {
                char element = GetElement(row, colIndex, rowIndex);
                if (IsNumeric(element))
                {
                    numberString += element;
                    row = ReplaceElement(row, colIndex, rowIndex, '.');
                }
                else
                {
                    break;
                }

                ++colIndex;
            }

            return (int.Parse(numberString), row);

        }

        char GetElement(int colIndex, int rowIndex)
        {
            string row = _data[rowIndex];
            return row[colIndex];
        }

        char GetElement(string row, int colIndex, int rowIndex)
        {
            return row[colIndex];
        }

        void ReplaceElement(int colIndex, int rowIndex, char replaceWith)
        {
            string row = _data[rowIndex];
            string newRow = "";
            if (colIndex > 0)
            {
                newRow = row.Substring(0, colIndex);
            }
            newRow += replaceWith;
            if (colIndex< _width-1)
            {
                newRow += row.Substring(colIndex + 1);
            }
            _data[rowIndex]=newRow;
        }

        string ReplaceElement(string row, int colIndex, int rowIndex, char replaceWith)
        {
            string newRow = "";
            if (colIndex > 0)
            {
                newRow = row.Substring(0, colIndex);
            }
            newRow += replaceWith;
            if (colIndex < _width - 1)
            {
                newRow += row.Substring(colIndex + 1);
            }
            return newRow;
        }

        bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
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
