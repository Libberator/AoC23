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

    public override void SolvePart1() => _logger.Log(TakeSteps(_grid, _startPos, 64)); // for test, use 6. Expected test result: 16

    // TODO: Change to quadratic extrapolation
    public override void SolvePart2()
    {
        int steps = 26501365;
        int size = _grid.Length; // 131, odd-sized square -> more even potential plots than odds
        var grids = steps / size; // 202300

        // get 3 mapped (x,y) points
        var x1 = steps % size; // 65
        var y1 = TakeSteps(_grid, _startPos, x1);
        var x2 = x1 + size; // 196
        var y2 = TakeSteps(_grid, _startPos, x2);
        var x3 = x2 + size; // 327
        var y3 = TakeSteps(_grid, _startPos, x3);

        // with 3 mapped (x,y) points, we can use the quadratic formula to extrapolate another y value given x
        // solve for a, b, and c in the equation y = a*x^2 + b*x + c
        // assumption: we simplify the formulas by saying that x1 is 0, x2 is 1, and x3 is 2
        var a = (y3 - 2 * y2 + y1) / 2;
        var b = y2 - y1 - a;
        var c = y1;

        var total = a * grids * grids + b * grids + c;
        _logger.Log(total); // 616951804315987
    }

    private static long TakeSteps(string[] grid, Vector2Int startPos, int steps)
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
        }

        return steps % 2 == 0 ? totalEvenPlots : totalOddPlots;
    }

    private static int Step(string[] grid, HashSet<Vector2Int> odd, HashSet<Vector2Int> even, int stepNumber)
    {
        var (nextSteps, startFrom) = stepNumber % 2 == 0 ? (even, odd) : (odd, even);
        var toRemove = new HashSet<Vector2Int>(nextSteps);

        List<Vector2Int> newEndPoints = [];
        foreach (var pos in startFrom)
        {
            foreach (var dir in Vector2Int.CardinalDirections)
            {
                var nextPos = pos + dir;
                if (nextSteps.Contains(nextPos)) continue;
                if (!CanVisit(grid, nextPos)) continue;

                nextSteps.Add(nextPos);
            }
        }
        nextSteps.ExceptWith(toRemove);
        return nextSteps.Count;
    }

    private static bool CanVisit(string[] grid, Vector2Int nextPos)
    {
        var xPos = nextPos.X.Mod(grid.Length);
        var yPos = nextPos.Y.Mod(grid[0].Length);
        return grid[xPos][yPos] != ROCK;
    }
}