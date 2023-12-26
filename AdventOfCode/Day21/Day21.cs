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

    // TODO: Change to quadratic extrapolation
    public override void SolvePart2()
    {
        long steps = 26501365;
        int size = _grid.Length; // 131, odd-sized square -> more even potential plots than odds

        // -------- FULL GRIDS ------------
        // figure out how many total grids get filled up - split into even and odd grids
        long metaGridWidth = steps / size; // 202,300
        var fullEvenMetaGrids = metaGridWidth * metaGridWidth; // 40925290000
        var fullOddMetaGrids = (metaGridWidth - 1) * (metaGridWidth - 1); // 40924885401

        // fill up the grid to get the even and odd distributions
        var stepsToFillFromCenter = size - 1; // ignoring starting point, step 65 laterally, then 65 more to the corner
        // _startPos is at the center of the grid
        var (even, odd, _) = TakeSteps(_grid, _startPos, stepsToFillFromCenter); // 7566, 7509 (total: 15075)

        // --------- PARTIAL GRIDS (edge cases, literally) ------------
        // partially fill up the 4 tips of the furthest traveled diamond corners
        var partialTop = TakeSteps(_grid, new Vector2Int(_startPos.X, size - 1), size - 1).Value;
        var partialRight = TakeSteps(_grid, new Vector2Int(0, _startPos.Y), size - 1).Value;
        var partialBottom = TakeSteps(_grid, new Vector2Int(_startPos.X, 0), size - 1).Value;
        var partialLeft = TakeSteps(_grid, new Vector2Int(size - 1, _startPos.Y), size - 1).Value;

        // small cutout triangles of a grid along the edge
        var smallTR = TakeSteps(_grid, new Vector2Int(size - 1, 0), size / 2 - 1).Value;
        var smallTL = TakeSteps(_grid, new Vector2Int(size - 1, size - 1), size / 2 - 1).Value;
        var smallBR = TakeSteps(_grid, new Vector2Int(0, 0), size / 2 - 1).Value;
        var smallBL = TakeSteps(_grid, new Vector2Int(0, size - 1), size / 2 - 1).Value;

        // larger "home-plate"-shaped cutout
        var largeTR = TakeSteps(_grid, new Vector2Int(size - 1, 0), 3 * size / 2 - 1).Value;
        var largeTL = TakeSteps(_grid, new Vector2Int(size - 1, size - 1), 3 * size / 2 - 1).Value;
        var largeBR = TakeSteps(_grid, new Vector2Int(0, 0), 3 * size / 2 - 1).Value;
        var largeBL = TakeSteps(_grid, new Vector2Int(0, size - 1), 3 * size / 2 - 1).Value;

        // ---------- TOTAL -----------
        var total = fullOddMetaGrids * odd + fullEvenMetaGrids * even
            + partialTop + partialRight + partialBottom + partialLeft
            + metaGridWidth * (smallTR + smallTL + smallBR + smallBL)
            + (metaGridWidth - 1) * (largeTR + largeTL + largeBR + largeBL);

        _logger.Log(total); // 616951804315987
    }

    private static (long Even, long Odd, long Value) TakeSteps(string[] grid, Vector2Int startPos, int steps)
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

        return (totalEvenPlots, totalOddPlots, steps % 2 == 0 ? totalEvenPlots : totalOddPlots);
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

    private static bool CanVisit(Vector2Int nextPos, string[] grid) => 
        nextPos.X >= 0 && nextPos.Y >= 0
        && nextPos.X < grid.Length && nextPos.Y < grid[0].Length
        && grid[nextPos.X][nextPos.Y] != ROCK;
}