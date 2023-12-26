using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day21(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char ROCK = '#', START = 'S';
    private string[] _grid = [];
    private Vector2Int _startPos;

    public override void Setup()
    {
        _grid = ReadAllLines();
        _startPos = new Vector2Int(_grid.Length / 2, _grid[0].Length / 2); // in the center
    }

    // for test, use 6. Expected test result: 16
    public override void SolvePart1() => _logger.Log(TakeSteps(_grid, _startPos, 64));

    public override void SolvePart2()
    {
        int steps = 26501365;
        int size = _grid.Length; // 131
        var rem = steps % size; // 65

        // get 3 mapped (x, y) points at x1 = 65, x2 = 196, x3 = 327
        var seq = TakeSteps(_grid, _startPos, [rem, rem + size, rem + 2 * size]).ToArray();

        // we can use the quadratic formula to extrapolate another y value given x
        // solve for a, b, and c with 3 linear equations of y = a*x^2 + b*x + c
        // To simplify: assume that x1 is equal to 0, x2 is 1, and x3 is 2
        var a = (seq[2] - 2 * seq[1] + seq[0]) / 2;
        var b = seq[1] - seq[0] - a;
        var c = seq[0];

        var x = steps / size; // 202300
        var total = (a * x * x) + (b * x) + c;
        _logger.Log(total); // 616951804315987
    }

    private static long TakeSteps(string[] grid, Vector2Int startPos, int steps) =>
        TakeSteps(grid, startPos, [steps]).First();

    private static IEnumerable<long> TakeSteps(string[] grid, Vector2Int startPos, int[] steps)
    {
        long totalEvenPlots = 1;
        long totalOddPlots = 0;
        HashSet<Vector2Int> oddSteps = [];
        HashSet<Vector2Int> evenSteps = [startPos];

        var maxSteps = steps.MaxBy(x => x);
        for (int i = 1; i <= maxSteps; i++)
        {
            var newPlots = Step(grid, oddSteps, evenSteps, i);
            if (i % 2 == 0) totalEvenPlots += newPlots;
            else totalOddPlots += newPlots;

            if (steps.Contains(i))
                yield return i % 2 == 0 ? totalEvenPlots : totalOddPlots;
        }
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
        var xPos = nextPos.X.Mod(grid.Length); // this Mod method will loop around like Pacman
        var yPos = nextPos.Y.Mod(grid[0].Length); // and handles negatives better than %
        return grid[xPos][yPos] != ROCK;
    }
}