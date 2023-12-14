using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day14(ILogger logger, string path) : Puzzle(logger, path)
{
    enum Direction : byte { North, West, South, East }
    private const char ROUND = 'O', EMPTY = '.', SOLID = '#';
    private const int CYCLES = 1_000_000_000;
    
    private readonly Dictionary<int, List<int>> _solidRocksByRow = [];
    private readonly List<RoundRock> _roundRocks = [];
    private int _rows, _cols;

    // wrap the struct as a reference type
    private class RoundRock(Vector2Int position) { public Vector2Int Position = position; }

    public override void Setup()
    {
        var grid = ReadAllLines();
        _rows = grid.Length;
        _cols = grid[0].Length;
        for (int row = 0; row < _rows; row++)
        {
            var line = grid[row];
            List<int> solidRocks = [];
            for (int col = 0; col < _cols; col++)
            {
                switch (line[col])
                {
                    case SOLID:
                        solidRocks.Add(col); 
                        break;
                    case ROUND:
                        _roundRocks.Add(new(new Vector2Int(row, col)));
                        break;
                    default:
                        break;
                }
            }
            _solidRocksByRow.Add(row, solidRocks);
        }
    }

    public override void SolvePart1()
    {
        var copy = _roundRocks.Select(r => new RoundRock(r.Position)).ToList();
        ShiftNorth(copy, _solidRocksByRow);
        _logger.Log(copy.Sum(r => _rows - r.Position.X));
    }

    public override void SolvePart2()
    {
        for (int i = 0; i < CYCLES; i++)
        {
            DoCycle(_roundRocks, _solidRocksByRow);
            if (HasHappenedBefore(_roundRocks))
            {
                break;
            }
        }
        _logger.Log(GetScore(_roundRocks));
    }

    private readonly HashSet<int> _cache = [];
    private int GetScore(List<RoundRock> roundRocks) => roundRocks.Sum(r => _rows - r.Position.X);

    private bool HasHappenedBefore(List<RoundRock> roundRocks)
    {
        var hash = HashCode.Combine(GetScore(roundRocks),
            StructuralComparisons.StructuralEqualityComparer.GetHashCode(roundRocks.Select(r => r.Position).ToArray()));
        if (_cache.Contains(hash)) return true;

        _cache.Add(hash);
        return false;
    }

    private void DoCycle(List<RoundRock> roundRocks, Dictionary<int, List<int>> solidRocksByRow)
    {
        ShiftNorth(roundRocks, solidRocksByRow);
        ShiftWest(roundRocks, solidRocksByRow);
        ShiftSouth(roundRocks, solidRocksByRow);
        ShiftEast(roundRocks, solidRocksByRow);
    }

    private void ShiftNorth(List<RoundRock> roundRocks, Dictionary<int, List<int>> solidRocksByRow)
    {
        for (int col = 0; col < _cols; col++)
        {
            var roundRocksInColumn = roundRocks.Where(r => r.Position.Y == col).ToList();
            var solidRocksInColumn = new List<int>(solidRocksByRow.Where(kvp => kvp.Value.Contains(col)).Select(kvp => kvp.Key));
            ShiftLeft(roundRocksInColumn, solidRocksInColumn, col, Direction.North);
        }
    }

    private void ShiftWest(List<RoundRock> roundRocks, Dictionary<int, List<int>> solidRocksByRow)
    {
        for (int row = 0; row < _rows; row++)
        {
            var roundRocksInRow = roundRocks.Where(r => r.Position.X == row).ToList();
            var solidRocksInRow = solidRocksByRow[row];
            ShiftLeft(roundRocksInRow, solidRocksInRow, row, Direction.West);
        }
    }

    private void ShiftSouth(List<RoundRock> roundRocks, Dictionary<int, List<int>> solidRocksByRow)
    {
        for (int col = 0; col < _cols; col++)
        {
            var roundRocksInColumn = roundRocks.Where(r => r.Position.Y == col).ToList();
            var solidRocksInColumn = new List<int>(solidRocksByRow.Where(kvp => kvp.Value.Contains(col)).Select(kvp => kvp.Key).Reverse());
            ShiftRight(roundRocksInColumn, solidRocksInColumn, col, Direction.South);
        }
    }

    private void ShiftEast(List<RoundRock> roundRocks, Dictionary<int, List<int>> solidRocksByRow)
    {
        for (int row = 0; row < _rows; row++)
        {
            var roundRocksInRow = roundRocks.Where(r => r.Position.X == row).ToList();
            var solidRocksInRow = solidRocksByRow[row].AsEnumerable().Reverse().ToList();
            ShiftRight(roundRocksInRow, solidRocksInRow, row, Direction.East);
        }
    }

    // North, West
    private void ShiftLeft(List<RoundRock> roundRocks, List<int> solidRocks, int rowOrCol, Direction dir)
    {
        int solidIndex = 0;
        var nextSolid = solidRocks.Count > solidIndex ? solidRocks[solidIndex++] : _cols;

        for (int i = 0; i < _cols; i++)
        {
            if (i == nextSolid)
            {
                nextSolid = solidRocks.Count > solidIndex ? solidRocks[solidIndex++] : _cols;
                continue;
            }

            if (dir is Direction.West)
            {
                if (roundRocks.Any(r => r.Position.Y == i))
                    continue; // occupied
            }
            else if (roundRocks.Any(r => r.Position.X == i))
                continue; // occupied

            switch (dir)
            {
                case Direction.North:
                    var rockToMoveUp = roundRocks.FirstOrDefault(r => r.Position.X > i && r.Position.X < nextSolid);
                    if (rockToMoveUp != null)
                        rockToMoveUp.Position.X = i;
                    break;
                case Direction.West:
                    var rockToMoveLeft = roundRocks.FirstOrDefault(r => r.Position.Y > i && r.Position.Y < nextSolid);
                    if (rockToMoveLeft != null)
                        rockToMoveLeft.Position.Y = i;
                    break;
                default:
                    break;
            }
        }
    }

    // East, South
    private void ShiftRight(List<RoundRock> roundRocks, List<int> solidRocks, int rowOrCol, Direction dir)
    {
        int solidIndex = 0;
        var nextSolid = solidRocks.Count > solidIndex ? solidRocks[solidIndex++] : -1;

        for (int i = _cols - 1; i >= 0; i--)
        {
            if (i == nextSolid)
            {
                nextSolid = solidRocks.Count > solidIndex ? solidRocks[solidIndex++] : -1;
                continue;
            }

            if (dir is Direction.East)
            {
                if (roundRocks.Any(r => r.Position.Y == i))
                    continue; // occupied
            }
            else if (roundRocks.Any(r => r.Position.X == i))
                continue; // occupied

            switch (dir)
            {
                case Direction.South:
                    var rockToMoveDown = roundRocks.FirstOrDefault(r => r.Position.X < i && r.Position.X > nextSolid);
                    if (rockToMoveDown != null)
                        rockToMoveDown.Position.X = i;
                    break;
                case Direction.East:
                    var rockToMoveRight = roundRocks.FirstOrDefault(r => r.Position.Y < i && r.Position.Y > nextSolid);
                    if (rockToMoveRight != null)
                        rockToMoveRight.Position.Y = i;
                    break;
                default:
                    break;
            }
        }
    }
}