using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC;

public class Day4(ILogger logger, string path) : Puzzle(logger, path)
{
    private const int BUFFER_SIZE = 10; // most any Card can win is 10
    private readonly List<Card> _cards = new();

    public override void Setup()
    {
        foreach (var line in ReadAllLines())
        {
            var matches = Regex.Match(line, @"Card\s+(\d+):(.+)\|(.+)");
            var winningNumbers = matches.Groups[2].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToIntArray();
            var cardNumbers = matches.Groups[3].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToIntArray();
            _cards.Add(new Card(winningNumbers, cardNumbers));
        }
    }

    public override void SolvePart1() => _logger.Log(_cards.Sum(c => c.Matches > 0 ? (int)Math.Pow(2, c.Matches - 1) : 0));

    public override void SolvePart2()
    {
        var numberOfCards = _cards.Count; // start with 1 of each original Card
        int[] circularBuffer = new int[BUFFER_SIZE];

        for (int i = 0; i < _cards.Count; i++)
        {
            var matches = _cards[i].Matches;
            var extraCopies = circularBuffer[i % BUFFER_SIZE];
            circularBuffer[i % BUFFER_SIZE] = 0;

            numberOfCards += extraCopies;

            for (int j = 0; j < matches; j++)
                circularBuffer[(i + 1 + j) % BUFFER_SIZE] += 1 + extraCopies;
        }

        _logger.Log(numberOfCards);
    }

    private class Card(int[] winningNumbers, int[] cardNumbers)
    {
        public readonly int[] WinningNumbers = winningNumbers;
        public readonly int[] CardNumbers = cardNumbers;
        private int _matches = -1;
        public int Matches => _matches == -1 ? _matches = CardNumbers.Count(n => WinningNumbers.Contains(n)) : _matches;
    }
}