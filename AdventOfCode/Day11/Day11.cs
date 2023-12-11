using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day11(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char GALAXY = '#';
    private const long SPACE_MULTIPLIER = 1_000_000L;

    private readonly List<Vector2Int> _galaxies = [];
    private readonly List<int> _emptyRows = [];
    private readonly List<int> _emptyCols = [];

    private long _unexpandedDistance = 0;
    private long _emptySpaceCrossings = 0;

    public override void Setup()
    {
        var data = ReadAllLines();
        for (int i = 0; i < data.Length; i++)
        {
            // check cols for empty space. this assumes square dataset
            if (!data.Any(line => line[i] == GALAXY))
                _emptyCols.Add(i);

            // check rows for empty space
            var line = data[i];
            if (!line.Contains(GALAXY))
            {
                _emptyRows.Add(i);
                continue;
            }

            // locate galaxies
            for (int j = 0; j < line.Length; j++)
            {
                if (line[j] == GALAXY)
                    _galaxies.Add(new Vector2Int(i, j));
            }
        }
    }

    public override void SolvePart1()
    {
        for (int i = 0; i < _galaxies.Count - 1; i++)
        {
            var first = _galaxies[i];
            for (int j = i + 1; j < _galaxies.Count; j++)
            {
                var second = _galaxies[j];
                _unexpandedDistance += Vector2Int.DistanceManhattan(first, second);
                _emptySpaceCrossings += EmptySpacesBetween(first, second);
            }
        }
        _logger.Log(_unexpandedDistance + _emptySpaceCrossings);
    }

    public override void SolvePart2() => _logger.Log(_unexpandedDistance + (SPACE_MULTIPLIER - 1) * _emptySpaceCrossings);

    private int EmptySpacesBetween(Vector2Int a, Vector2Int b)
    {
        var rowStart = Math.Min(a.X, b.X);
        var rowStop = Math.Max(a.X, b.X);
        var colStart = Math.Min(a.Y, b.Y);
        var colStop = Math.Max(a.Y, b.Y);

        return _emptyRows.Count(r => r > rowStart && r < rowStop) +
            _emptyCols.Count(c => c > colStart && c < colStop);
    }
}