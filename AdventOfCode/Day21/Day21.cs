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

    public override void SolvePart1() => _logger.Log(TakeSteps(_grid, _startPos, 64).Even);

    public override void SolvePart2()
    {
        // start: 65, 65
        // width = 131: 65th spot (1), then 65 on each side (130)
        // steps / size = 202300 + 65/131
        // 5 * 11 * 481843

        long steps = 26501365;
        int size = _grid.Length; // 131, odd-sized square -> more evens than odds
        
        // -------- FULL GRIDS ------------
        // fill up the grid to get the even and odd distributions
        var stepsToFillFromCenter = size - 1; // ignoring starting point, step 65 laterally, then 65 more to the corner
        // _startPos is at the center of the grid
        var (even, odd) = TakeSteps(_grid, _startPos, stepsToFillFromCenter); // 7566, 7509 (total: 15075)

        // figure out how many total grids get filled up - split into even and odd grids
        long gridsTraversedWidth = (steps - (size / 2)) / size; // 202,300
        var fullOddGrids = gridsTraversedWidth * gridsTraversedWidth; // 40925290000
        var fullEvenGrids = (gridsTraversedWidth - 1) * (gridsTraversedWidth - 1); // 40924885401

        // --------- PARTIAL GRIDS ------------
        // partially fill up the 4 tips of the furthest traveled diamond corners
        var partialTop = TakeSteps(_grid, new Vector2Int(_startPos.X, size - 1), size - 1).Even;
        var partialRight = TakeSteps(_grid, new Vector2Int(0, _startPos.Y), size - 1).Even;
        var partialBottom = TakeSteps(_grid, new Vector2Int(_startPos.X, 0), size - 1).Even;
        var partialLeft = TakeSteps(_grid, new Vector2Int(size - 1, _startPos.Y), size - 1).Even;





        var total = fullEvenGrids * even + fullOddGrids * odd;

        long remainingSteps = size / 2 + steps % size;

        // 15066 (130), 14839 (129) +227
        // full grid has (7566 even, 7509 odd: 15075 total) plots out of 17161


        var startFromUpperLeftCorner = Vector2Int.Zero;
        var stepsToFillFromCorner = 2 * stepsToFillFromCenter;
        var (cornereven, cornerodd) = TakeSteps(_grid, startFromUpperLeftCorner, stepsToFillFromCorner);

        int[] runs = new int[] { 55, 65, 121, 128, 129, 130, 165, 195 };
        // 55 * 1, 2, 3
        // 55 * 481 843
        long[] results = new long[runs.Length];

        for (int i = 0; i < runs.Length; i++)
        {
            var step = runs[i];
            var (even2, odd2) = TakeSteps(_grid, _startPos, step);
            Console.WriteLine($"{step}. Even: {even2}, Odd: {odd2}");
            results[i] = step % 2 == 0 ? even2 : odd2;
        }

        //var odd = 0;
        //var (_, odd) = TakeSteps(_grid, _startPos, steps);
        _logger.Log(0);
    }

    private static (long Even, long Odd) TakeSteps(string[] grid, Vector2Int startPos, int steps)
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

            if (i % 100_000 == 0)
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
        if (nextPos.X <0 || nextPos.Y <0) return false;
        if (nextPos.X >= grid.Length || nextPos.Y >= grid[0].Length) return false;

        return grid[nextPos.X][nextPos.Y] != ROCK;
        //var xPos = nextPos.X.Mod(grid.Length); // loops around
        //var ypos = nextPos.Y.Mod(grid[0].Length);
        //return grid[xPos][ypos] != ROCK;
    }
}