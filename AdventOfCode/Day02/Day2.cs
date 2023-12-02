using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day2(ILogger logger, string path) : Puzzle(logger, path)
{
    private const string RED = "red";
    private const string GREEN = "green";
    private const string BLUE = "blue";
    private const int MAX_RED = 12, MAX_GREEN = 13, MAX_BLUE = 14;

    private readonly List<Game> _games = new();

    public override void Setup()
    {
        foreach (var line in ReadAllLines())
        {
            var lineSplit = line.Split(':');
            var gameID = int.Parse(lineSplit[0].Split(' ')[1]);
            var game = new Game(gameID);
            ParseSets(lineSplit[1].Split(';'), game);
            game.CalculatePower();
            _games.Add(game);
        }
    }

    private static void ParseSets(string[] sets, Game game)
    {
        foreach (var setText in sets)
        {
            var set = new Set();
            foreach (var revealed in setText.Split(',', StringSplitOptions.TrimEntries))
            {
                var draws = revealed.Split(' ', StringSplitOptions.TrimEntries);
                var amount = int.Parse(draws[0]);
                switch (draws[1])
                {
                    case RED: set.Red = amount; break;
                    case GREEN: set.Green = amount; break;
                    case BLUE: set.Blue = amount; break;
                    default: break;
                }
            }
            game.Sets.Add(set);
        }
    }

    public override void SolvePart1() => _logger.Log(_games.Sum(g => g.IsPossible() ? g.ID : 0));

    public override void SolvePart2() => _logger.Log(_games.Sum(g => g.Power));

    private class Game(int id)
    {
        public readonly int ID = id;
        public readonly List<Set> Sets = new();
        public int Power;

        public void CalculatePower()
        {
            int maxRed = Sets.Max(s => s.Red);
            int maxGreen = Sets.Max(s => s.Green);
            int maxBlue = Sets.Max(s => s.Blue);
            Power = maxRed * maxGreen * maxBlue;
        }

        public bool IsPossible()
        {
            foreach (var set in Sets)
            {
                if (set.Red > MAX_RED) return false;
                if (set.Green > MAX_GREEN) return false;
                if (set.Blue > MAX_BLUE) return false;
            }
            return true;
        }
    }

    private class Set(int red = 0, int green = 0, int blue = 0)
    {
        public int Red = red, Green = green, Blue = blue;
    }
}