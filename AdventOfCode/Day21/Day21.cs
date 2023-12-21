using System;
using System.Collections.Generic;
using System.Numerics;

namespace AoC;

public class Day21(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char START = 'S', ROCK = '#';
    private string[] _grid = [];
    private Vector2Int _startPos;

    public override void Setup()
    {
        _grid = ReadAllLines();
        for (int row = 0; row < _grid.Length; row++)
        {
            var line = _grid[row];
            var startIndex = line.IndexOf(START);
            if (startIndex != -1)
            {
                _startPos = new Vector2Int(row, startIndex);
                break;
            }
        }
    }

    public override void SolvePart1()
    {
        int steps = 64;
        var (even, _) = TakeSteps(_grid, _startPos, steps);
        _logger.Log(even);
    }

    public override void SolvePart2()
    {
        int steps = 26501365;
        var (_, odd) = TakeSteps(_grid, _startPos, steps);
        _logger.Log(odd);
    }

    private static (long evenPlots, long oddPlots) TakeSteps(string[] grid, Vector2Int startPos, int steps)
    {
        long totalEvenPlots = 1;
        long totalOddPlots = 0;
        HashSet<Vector2Int> oddSteps = [];
        HashSet<Vector2Int> evenSteps = [startPos];

        for (int i = 1; i <= steps; i++)
        {
            var newPlots = Step(grid, oddSteps, evenSteps, i);
            if (i % 2 == 0) totalEvenPlots += newPlots;
            else totalOddPlots += newPlots;

            if (i % 64 == 0 || i % 65 == 0 || i % 66 == 0)
            {
                Console.WriteLine($"{i}. even: {totalEvenPlots}, odd: {totalOddPlots}");
            }
        }

        return (totalEvenPlots, totalOddPlots);
    }

    private static int Step(string[] grid, HashSet<Vector2Int> odd, HashSet<Vector2Int> even, int stepNumber)
    {
        var (nextSteps, startFrom) = stepNumber % 2 == 0 ?
            (even, odd) : (odd, even);
        var toRemove = new HashSet<Vector2Int>(nextSteps);

        List<Vector2Int> newEndPoints = [];
        foreach (var pos in startFrom)
        {
            foreach (var dir in Vector2Int.CardinalDirections)
            {
                var nextPos = pos + dir;
                if (nextSteps.Contains(nextPos)) continue;
                if (!CanVisit(nextPos, grid)) continue;

                nextSteps.Add(nextPos);
            }
        }
        nextSteps.ExceptWith(toRemove);
        return nextSteps.Count;
    }

    private static bool CanVisit(Vector2Int nextPos, string[] grid)
    {
        var xPos = nextPos.X.Mod(grid.Length); // loops around
        var ypos = nextPos.Y.Mod(grid[0].Length);

        return grid[xPos][ypos] != ROCK;
    }
}