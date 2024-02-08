using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    internal class Day4
    {
        List<string> _data = new List<string>();

        internal void Run(string filepath)
        {
            _data = ReadTextData(filepath);

            ProcessData_Phase2();
        }

        void ProcessData()
        {
            int sum = 0;
            foreach(string line in _data)
            {
                (int cardNumber, int matchingNumbers) = ProcessLine(line);
                int cardPoints = 0;
                if (matchingNumbers>0)
                {
                    cardPoints = (int)(Math.Pow(2, matchingNumbers - 1));
                }
                Console.WriteLine($"Card: {cardNumber} has {cardPoints} points");
                sum += cardPoints;
            }
            Console.WriteLine(sum);
        }

        void ProcessData_Phase2()
        {
            Dictionary<int, int> cardNumber_ToCount= new Dictionary<int, int>();
            int maxCardNumber = -1;
            foreach (string line in _data)
            {
                (int cardNumber, int cardPoints) = ProcessLine(line);
                cardNumber_ToCount[cardNumber] = cardPoints;
                maxCardNumber = cardNumber;
            }

            int cardCount = maxCardNumber;
            for (int cardNumber = 1; cardNumber <= maxCardNumber; ++cardNumber)
            {
                cardCount = HowManyCards(cardCount, cardNumber_ToCount, cardNumber);
            }
            Console.WriteLine($"Card count: {cardCount}");
        }

        int HowManyCards(int currentCount, Dictionary<int, int> cardNumber_ToCount, int calculateForCardNumber)
        {
            int winningNumbers = cardNumber_ToCount[calculateForCardNumber];
            if (winningNumbers>0)
            {
                currentCount += winningNumbers;
                for(int cardNumberToConsiderNext = calculateForCardNumber+1;cardNumberToConsiderNext< calculateForCardNumber + 1+winningNumbers;++cardNumberToConsiderNext)
                {
                    currentCount=HowManyCards(currentCount, cardNumber_ToCount, cardNumberToConsiderNext);
                }
            }

            return currentCount;
        }


        (int cardNumber, int matchingNumbers) ProcessLine(string line)
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex==-1)
            {
                return (0,0);
            }
            int cardNumber = ProcessCardNumber(line.Substring(0, colonIndex));
            string restOfLine = line.Substring(colonIndex + 2);
            int pipeIndex = restOfLine.IndexOf("|");

            string winningNumbers = restOfLine.Substring(0, pipeIndex - 1).Trim();
            string cardNumbers = restOfLine.Substring(pipeIndex + 2).Trim();

            string[] winningNumbersArray = winningNumbers.Split(' ');
            Dictionary<string, bool> winningNumbersAsDict = new Dictionary<string, bool>();
            foreach(string winningNumberStr in winningNumbersArray)
            {
                winningNumbersAsDict[winningNumberStr] = true;
            }

            List<string> cardNumbersList = cardNumbers.Split(" ").ToList();

            int matchingNumbers = 0;
            foreach(string cardAAttemptStr in cardNumbersList)
            {
                if (String.IsNullOrEmpty(cardAAttemptStr)) 
                {
                    continue; 
                }
                if (winningNumbersAsDict.ContainsKey(cardAAttemptStr))
                {
                    ++matchingNumbers;
                }
            }

            return (cardNumber, matchingNumbers);
        }

        int ProcessCardNumber(string cardNumberAsString)
        {
            int spaceIndex = cardNumberAsString.IndexOf(' ');
            if (spaceIndex==-1)
            {
                return -1;
            }

            int cardNumber = int.Parse(cardNumberAsString.Substring(spaceIndex+1));

            return cardNumber;
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
