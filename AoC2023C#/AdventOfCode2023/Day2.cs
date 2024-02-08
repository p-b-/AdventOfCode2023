using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    internal class Day2
    {
        class Day2SubGame
        {
            internal int BlueCount { get; private set; }
            internal int RedCount { get; private set; }
            internal int GreenCount { get; private set; }

            public Day2SubGame(string data)
            {
                if (!String.IsNullOrEmpty(data))
                {
                    ProcessData(data);
                }
            }

            void ProcessData(string data)
            {
                string[] cubeCounts = data.Split(',');
                foreach (string cubeDatum in cubeCounts)
                {
                    if (!string.IsNullOrEmpty(cubeDatum))
                    {
                        string trimmedDatum = cubeDatum.Trim();

                        string[] pieces = trimmedDatum.Split(" ");
                        if (pieces.Length >= 2)
                        {
                            int count = int.Parse(pieces[0]);
                            switch (pieces[1].ToLower())
                            {
                                case "blue": BlueCount = count; break;
                                case "red": RedCount = count; break;
                                case "green": GreenCount = count; break;
                            }
                        }
                    }
                }
            }
        }
        class Day2Game
        {
            internal int GameId { get; private set; }
            internal List<Day2SubGame> SubGames { get; private set; }
            public Day2Game(string line)
            {
                SubGames = new List<Day2SubGame>();
                if (!String.IsNullOrEmpty(line))
                {
                    ProcessLine(line);
                }
            }

            internal bool IsPossible(int redCount, int greenCount, int blueCount)
            {
                foreach (Day2SubGame subGame in SubGames)
                {
                    if (redCount < subGame.RedCount ||
                        greenCount < subGame.GreenCount ||
                        blueCount < subGame.BlueCount)
                    {
                        return false;
                    }
                }
                return true;
            }

            internal int GamePower()
            {
                int maxRed = 0;
                int maxGreen = 0;
                int maxBlue = 0;
                foreach (Day2SubGame subGame in SubGames)
                {
                    if (subGame.RedCount > maxRed)
                    {
                        maxRed = subGame.RedCount;
                    }
                    if (subGame.GreenCount > maxGreen)
                    {
                        maxGreen = subGame.GreenCount;
                    }
                    if (subGame.BlueCount > maxBlue)
                    {
                        maxBlue = subGame.BlueCount;
                    }
                }
                return maxRed * maxGreen * maxBlue;
            }

            void ProcessLine(string line)
            {
                int indexOfColon = line.IndexOf(":");
                int indexOfSpace = line.LastIndexOf(" ", indexOfColon);
                string gameIdAsString = line.Substring(indexOfSpace + 1, indexOfColon - indexOfSpace - 1);
                GameId = int.Parse(gameIdAsString);
                string subGamesAsString = line.Substring(indexOfColon + 2);

                string[] subGamesStringData = subGamesAsString.Split(';');
                foreach (string subGame in subGamesStringData)
                {
                    Day2SubGame day2SubGame = new Day2SubGame(subGame);
                    SubGames.Add(day2SubGame);
                }
            }

        }
        internal void Run(string filepath)
        {
            List<string> data = ReadTextData(filepath);
            List<Day2Game> games = new List<Day2Game>();
            foreach (string line in data)
            {
                Day2Game game = new Day2Game(line);
                games.Add(game);
            }
            int summedIdCount = CountValidGames(games, redCount: 12, greenCount: 13, blueCount: 14);
            Console.WriteLine($"Summed game ids {summedIdCount}");

            int summedPower = SumGamePowers(games);
            Console.WriteLine($"Summed powers is {summedPower}");
        }

        int CountValidGames(List<Day2Game> gameData, int redCount, int greenCount, int blueCount)
        {
            int sum = 0;
            foreach (Day2Game game in gameData)
            {
                if (game.IsPossible(redCount: redCount, greenCount: greenCount, blueCount: blueCount))
                {
                    sum += game.GameId;
                }
            }
            return sum;
        }

        int SumGamePowers(List<Day2Game> gameData)
        {
            int sum = 0;
            foreach (Day2Game game in gameData)
            {
                sum += game.GamePower();
            }
            return sum;
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
