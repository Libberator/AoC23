using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day7(ILogger logger, string path) : Puzzle(logger, path)
{
    private enum HandType { HighCard, OnePair, TwoPair, ThreeOfAKind, FullHouse, FourOfAKind, FiveOfAKind };
    private static readonly char[] _ranking = ['2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'];
    private static readonly char[] _jokerRanking = ['J', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'Q', 'K', 'A'];
    private readonly List<Hand> _hands = [];

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            var split = line.Split(' ');
            var hand = new Hand(split[0], int.Parse(split[1]));
            _hands.Add(hand);
        }
    }

    public override void SolvePart1()
    {
        _hands.Sort(CompareHands);
        _logger.Log(GetTotalWinnings(_hands));
    }

    public override void SolvePart2()
    {
        _hands.Sort(CompareJokerHands);
        _logger.Log(GetTotalWinnings(_hands));
    }

    private static int GetTotalWinnings(List<Hand> hands)
    {
        int rank = 1;
        return hands.Aggregate(0, (total, h) => total += h.Bid * rank++);
    }

    private static int CompareHands(Hand a, Hand b)
    {
        if (a.HandType != b.HandType) return a.HandType.CompareTo(b.HandType);

        for (int i = 0; i < a.Cards.Length; i++)
        {
            if (a.Cards[i] == b.Cards[i]) continue;
            return Array.IndexOf(_ranking, a.Cards[i]).CompareTo(Array.IndexOf(_ranking, b.Cards[i]));
        }
        return 0;
    }

    private static int CompareJokerHands(Hand a, Hand b)
    {
        if (a.JokerHandType != b.JokerHandType) return a.JokerHandType.CompareTo(b.JokerHandType);

        for (int i = 0; i < a.Cards.Length; i++)
        {
            if (a.Cards[i] == b.Cards[i]) continue;
            return Array.IndexOf(_jokerRanking, a.Cards[i]).CompareTo(Array.IndexOf(_jokerRanking, b.Cards[i]));
        }
        return 0;
    }

    private record Hand(string Cards, int Bid)
    {
        public readonly HandType HandType = GetHandType(Cards);
        public readonly HandType JokerHandType = GetJokerHandType(Cards);

        private static HandType GetHandType(string cards) => cards.ToHashSet().Count switch
        {
            1 => HandType.FiveOfAKind,
            2 => cards.Count(c => c == cards[0]) is 1 or 4 ? HandType.FourOfAKind : HandType.FullHouse,
            3 => cards.Any(i => cards.Count(c => c == i) == 3) ? HandType.ThreeOfAKind : HandType.TwoPair,
            4 => HandType.OnePair,
            _ => HandType.HighCard
        };

        private static HandType GetJokerHandType(string cards) => cards.Count(c => c == 'J') switch
        {
            5 or 4 => HandType.FiveOfAKind,
            3 => cards.ToHashSet().Count is 2 ? HandType.FiveOfAKind : HandType.FourOfAKind,
            2 => cards.ToHashSet().Count switch
            {
                2 => HandType.FiveOfAKind,
                3 => HandType.FourOfAKind,
                _ => HandType.ThreeOfAKind
            },
            1 => cards.ToHashSet().Count switch
            {
                2 => HandType.FiveOfAKind,
                3 => cards.Count(c => c == cards.First(i => i != 'J')) is 1 or 3 ? HandType.FourOfAKind : HandType.FullHouse,
                4 => HandType.ThreeOfAKind,
                _ => HandType.OnePair
            },
            _ => GetHandType(cards)
        };
    }
}