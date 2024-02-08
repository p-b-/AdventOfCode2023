using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    // part 1 score: 256448566
    // part 2 score: 254412181   
    internal class Day7
    {
        class Hand
        {
            internal HandType HandType { get; private set; }
            internal string HandStr { get; private set; }
            internal Int64 Bid { get; private set; }
            internal int Rank { get; private set; }
            internal bool Unscored { get; private set; }
            internal Hand(HandType handType, string hand, int rank, Int64 bid)
            {
                HandType = handType;
                HandStr = hand;
                Bid = bid;
                Rank = rank;
                Unscored = true;
            }

            internal void Scored()
            {
                Unscored = false;
            }
        }
        enum HandType
        {
            None,
            FiveOfAKind,
            FourOfAKind,
            FullHouse,
            ThreeOfAKind,
            TwoPair,
            OnePair,
            HighCard
        }

        List<Hand> _hands = new List<Hand>();

        bool _phase1Not2 = true;

        internal void Run(string filepath)
        {
            _phase1Not2 = false;
            ReadTextData(filepath);

            Int64 score = RankHands();

            Console.WriteLine($"Score is {score}");
        }

        internal Int64 RankHands()
        {
            Dictionary<HandType, List<Hand>> handsRankedByType = CoallesceHandsIntoAHandTypeList();

            Int64 currentRank = 1;
            Int64 currentScore = 0;
            for (HandType handType = HandType.HighCard; handType >= HandType.FiveOfAKind; --handType)
            {
                List<Hand>? hands;
                if (handsRankedByType.TryGetValue(handType, out hands))
                {
                    (Int64 scoreForHands, currentRank) = ScoreHands(hands!, currentRank);
                    currentScore += scoreForHands;
                }
            }
            return currentScore;
        }

        private Dictionary<HandType, List<Hand>> CoallesceHandsIntoAHandTypeList()
        {
            Dictionary<HandType, List<Hand>> handsRankedByType = new Dictionary<HandType, List<Hand>>();

            foreach (Hand h in _hands)
            {
                List<Hand>? hands;
                if (!handsRankedByType.TryGetValue(h.HandType, out hands))
                {
                    hands = new List<Hand>();
                    handsRankedByType[h.HandType] = hands;
                }
                hands.Add(h);
            }
            return handsRankedByType;
        }

        private (Int64 score, Int64 currentRank) ScoreHands(List<Hand> hands, Int64 currentRank)
        {
            Int64 score = 0;
            var sortedHands = hands.OrderBy(x => x.Rank).ToList();

            foreach(Hand h in sortedHands)
            {
                Console.WriteLine($"Cards {h.HandStr}, internal score {h.Rank}, scores {h.Bid} x {currentRank}");
                score += h.Bid * currentRank++;
            }

            return (score, currentRank) ;
        }


        void ReadTextData(string filepath)
        {
            string? line;
            StreamReader sr = new StreamReader(filepath);

            line = sr.ReadLine();
            while (line != null)
            {
                Hand? h = ParseLine(line.ToLower());
                if (h != null)
                {
                    _hands.Add(h);
                }

                line = sr.ReadLine();

            }
            sr.Close();
        }

        Hand? ParseLine(string line)
        {
            Hand? h = null;
            if (!String.IsNullOrEmpty(line))
            {
                string[] split = line.Split(' ');
                Int64 bid = Int64.Parse(split[split.Length - 1]);

                h = ProcessHand(split[0].Trim(), bid);
            }
            return h;
        }

        Hand ProcessHand(string hand, Int64 bid)
        {
            Dictionary<char, int> handValues = new Dictionary<char, int>();
            // Count the number of specific cards in the hand.  
            for (int i = 0; i < hand.Length; ++i)
            {
                char c = hand[i];
                int currentCount;
                if (handValues.TryGetValue(c, out currentCount))
                {
                    ++currentCount;
                }
                else
                {
                    currentCount = 1;
                }
                handValues[c] = currentCount;
            }
            int jokerCount = 0;
            if (handValues.ContainsKey('j'))
            {
                jokerCount = handValues['j'];
            }

            Dictionary<int, int> potentialCounts = new Dictionary<int, int>();
            for (int potentialCountIndex = 5; potentialCountIndex >= 1; --potentialCountIndex)
            {
                potentialCounts[potentialCountIndex] = 0;
                foreach (int value in handValues.Values)
                {
                    if (value == potentialCountIndex)
                    {
                        potentialCounts[potentialCountIndex] = potentialCounts[potentialCountIndex] + 1;
                    }
                }
            }

            HandType handType = DetermineHandType(potentialCounts);
            
            int cardRank;

            if (_phase1Not2) {
                cardRank = DetermineCardRank(hand);
            }
            else
            {
                cardRank = DetermineCardRank_Phase2(hand);
            }

            if (!_phase1Not2 && jokerCount>0 && jokerCount<5)
            {
                HandType prevType = handType;
                handType = ConsiderUsingJokers(handType, jokerCount);
                Console.WriteLine($"{hand} changed from {prevType} to {handType}");
            }

            return new Hand(handType, hand, cardRank, bid);
        }

        // Must have at least one joke to call this
        HandType ConsiderUsingJokers(HandType handType, int jokerCount)
        {
            switch(handType)
            {
                case HandType.FourOfAKind: 
                    // Turn aaaaj into aaaaa
                    return HandType.FiveOfAKind;
                case HandType.FullHouse: 
                    // Turn aaajj into aaaaa
                    // Turn jjjbb into bbbbb
                    return HandType.FiveOfAKind;
                case HandType.ThreeOfAKind:
                    // Turn JJJxy into xxxxy or yyyxy  (x and y have to be different here)
                    // Turn xxxJy into xxxxy
                    return HandType.FourOfAKind;
                case HandType.TwoPair:
                    // Turn JJbbc into bbbbc
                    // Turn aaJJc into aaaac
                    // Turn aabbJ into aabba
                    // Turn aabbJ into aabbb

                    if (jokerCount == 2)
                    {
                        return HandType.FourOfAKind;
                    }
                    else if (jokerCount == 1)
                    {
                        return HandType.FullHouse;
                    }
                    break;
                case HandType.OnePair:
                    // Turn aabcJ into aabca (three of a kind)
                    // Turn JJbcd into bbbcd (three of a kind)
                    return HandType.ThreeOfAKind;
                case HandType.HighCard:
                    // Turn abcdJ into abcdd (one pair)
                    return HandType.OnePair;
            }

            return handType;
        }

        int DetermineCardRank(string hand)
        {
            int rank = 0;
            int multiplier = 13 * 13 * 13 * 13;
            foreach(char c  in hand)
            {
                int scoreForCard = c switch
                {
                    '2' => 0,
                    '3' => 1,
                    '4' => 2,
                    '5' => 3,
                    '6' => 4,
                    '7' => 5,
                    '8' => 6,
                    '9' => 7,
                    't' => 8,
                    'j' => 9,
                    'q' => 10,
                    'k' => 11,
                    'a' => 12,
                    _ => 0
                };
                rank += scoreForCard * multiplier;
                multiplier /= 13;
            }
            return rank;
        }

        int DetermineCardRank_Phase2(string hand)
        {
            int rank = 0;
            int multiplier = 13 * 13 * 13 * 13;
            foreach (char c in hand)
            {
                int scoreForCard = c switch
                {
                    'j' => 0,
                    '2' => 1,
                    '3' => 2,
                    '4' => 3,
                    '5' => 4,
                    '6' => 5,
                    '7' => 6,
                    '8' => 7,
                    '9' => 8,
                    't' => 9,
                    'q' => 10,
                    'k' => 11,
                    'a' => 12,
                    _ => 0
                };
                rank += scoreForCard * multiplier;
                multiplier /= 13;
            }
            return rank;
        }

        private static HandType DetermineHandType(Dictionary<int, int> potentialCounts)
        {
            HandType handType;
            if (potentialCounts[5] == 1)
            {
                handType = HandType.FiveOfAKind;
            }
            else if (potentialCounts[4] == 1)
            {
                handType = HandType.FourOfAKind;
            }
            else if (potentialCounts[3] == 1 && potentialCounts[2] == 1)
            {
                handType = HandType.FullHouse;
            }
            else if (potentialCounts[3] == 1)
            {
                handType = HandType.ThreeOfAKind;
            }
            else if (potentialCounts[2] == 2)
            {
                handType = HandType.TwoPair;
            }
            else if (potentialCounts[2] == 1)
            {
                handType = HandType.OnePair;
            }
            else
            {
                handType = HandType.HighCard;
            }

            return handType;
        }
    }
}
