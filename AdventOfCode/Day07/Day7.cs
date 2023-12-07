using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day7(ILogger logger, string path) : Puzzle(logger, path)
{
    private enum HandType { HighCard, OnePair, TwoPair, ThreeOfAKind, FullHouse, FourOfAKind, FiveOfAKind };
    private readonly char[] _ranking = ['2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'];
    private readonly char[] _jokerRanking = ['J', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'Q', 'K', 'A'];
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
        int rank = 1;
        _hands.Sort((a, b) => CompareHands(a, b, _ranking));
        var total = _hands.Aggregate(0, (total, h) => total += h.Bid * rank++); ;
        _logger.Log(total);
    }

    public override void SolvePart2()
    {
        int rank = 1;
        _hands.Sort((a, b) => CompareHands(a, b, _jokerRanking, withJoker: true));
        var total = _hands.Aggregate(0, (total, h) => total += h.Bid * rank++); ;
        _logger.Log(total);
    }

    private static int CompareHands(Hand a, Hand b, char[] rankings, bool withJoker = false)
    {
        // First Compare by Hand Type
        var aHandType = withJoker ? a.HandTypeWithJoker : a.HandType;
        var bHandType = withJoker ? b.HandTypeWithJoker : b.HandType;
        if (aHandType != bHandType) return aHandType.CompareTo(bHandType);

        // Then Compare by Card according to provided Ranking
        for (int i = 0; i < a.Cards.Length; i++)
        {
            if (a.Cards[i] == b.Cards[i]) continue;
            return Array.IndexOf(rankings, a.Cards[i]).CompareTo(Array.IndexOf(rankings, b.Cards[i]));
        }
        return 0;
    }

    private record Hand(string Cards, int Bid)
    {
        public readonly HandType HandType = GetHandType(Cards);
        public readonly HandType HandTypeWithJoker = GetHandTypeWithJoker(Cards);

        private static HandType GetHandType(string cards) => cards.ToHashSet().Count switch
        {
            1 => HandType.FiveOfAKind,
            2 => cards.Count(c => c == cards[0]) is 2 or 3 ? HandType.FullHouse : HandType.FourOfAKind,
            3 => cards.Any(i => cards.Count(c => c == i) is 3) ? HandType.ThreeOfAKind : HandType.TwoPair,
            4 => HandType.OnePair,
            _ => HandType.HighCard
        };

        private static HandType GetHandTypeWithJoker(string cards) => cards.Count(c => c == 'J') switch
        {
            5 or 4 => HandType.FiveOfAKind,
            3 => cards.Count(c => c == cards.First(i => i != 'J')) is 2 ? HandType.FiveOfAKind : HandType.FourOfAKind,
            2 => cards.ToHashSet().Count switch
            {
                2 => HandType.FiveOfAKind,
                3 => HandType.FourOfAKind,
                4 or _ => HandType.ThreeOfAKind
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