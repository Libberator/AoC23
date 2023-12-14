using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day14(ILogger logger, string path) : Puzzle(logger, path)
{
    enum Direction : byte { North, West, South, East }
    private const char ROUND = 'O', SOLID = '#';
    private const int CYCLES = 1_000_000_000;

    private readonly Dictionary<int, List<int>> _solidRocksByRow = [], _solidRocksByCol = [];
    private readonly List<CoordRef> _roundRocks = [];
    private readonly HashSet<int> _cache = [];
    private int _rows, _cols;

    public override void Setup()
    {
        var grid = ReadAllLines();
        _rows = grid.Length;
        _cols = grid[0].Length;
        for (int row = 0; row < _rows; row++)
        {
            List<int> solidRocksRow = [];
            _solidRocksByRow.Add(row, solidRocksRow);
            List<int> solidRocksCol = [];
            _solidRocksByCol.Add(row, solidRocksCol); // treat this row as a col

            var line = grid[row];
            for (int col = 0; col < _cols; col++)
            {
                if (line[col] == ROUND)
                    _roundRocks.Add(new(row, col));
                else if (line[col] == SOLID)
                    solidRocksRow.Add(col);

                if (grid[col][row] == SOLID) // swizzled here to traverse column-wise
                    solidRocksCol.Add(col); // treat this col as a row
            }
        }
    }

    public override void SolvePart1()
    {
        ShiftNorth(_roundRocks, _solidRocksByCol);
        _logger.Log(GetScore(_roundRocks));
    }

    public override void SolvePart2()
    {
        for (int i = 0; i < CYCLES; i++)
        {
            DoCycle(_roundRocks);

            if (HasHappenedBefore(_roundRocks)) break;
        }
        _logger.Log(GetScore(_roundRocks));
    }

    private int GetScore(List<CoordRef> roundRocks) => roundRocks.Sum(r => _rows - r.Row);

    private bool HasHappenedBefore(List<CoordRef> roundRocks)
    {
        var hash = GetHash(roundRocks);
        if (_cache.Contains(hash)) return true;

        _cache.Add(hash);
        return false;

        int GetHash(List<CoordRef> roundRocks) => HashCode.Combine(GetScore(roundRocks),
            StructuralComparisons.StructuralEqualityComparer.GetHashCode(roundRocks.Select(r => r.Row).ToArray()),
            StructuralComparisons.StructuralEqualityComparer.GetHashCode(roundRocks.Select(r => r.Col).ToArray()));
    }

    private void DoCycle(List<CoordRef> roundRocks)
    {
        ShiftNorth(roundRocks, _solidRocksByCol);
        ShiftWest(roundRocks, _solidRocksByRow);
        ShiftSouth(roundRocks, _solidRocksByCol);
        ShiftEast(roundRocks, _solidRocksByRow);
    }

    private void ShiftNorth(List<CoordRef> roundRocks, Dictionary<int, List<int>> solidRocks)
    {
        for (int col = 0; col < _cols; col++)
        {
            var roundRocksInColumn = roundRocks.Where(r => r.Col == col).ToList();
            var solidRocksInColumn = solidRocks[col];
            ShiftFromLowToHigh(roundRocksInColumn, solidRocksInColumn, Direction.North);
        }
    }

    private void ShiftWest(List<CoordRef> roundRocks, Dictionary<int, List<int>> solidRocks)
    {
        for (int row = 0; row < _rows; row++)
        {
            var roundRocksInRow = roundRocks.Where(r => r.Row == row).ToList();
            var solidRocksInRow = solidRocks[row];
            ShiftFromLowToHigh(roundRocksInRow, solidRocksInRow, Direction.West);
        }
    }

    private void ShiftSouth(List<CoordRef> roundRocks, Dictionary<int, List<int>> solidRocks)
    {
        for (int col = 0; col < _cols; col++)
        {
            var roundRocksInColumn = roundRocks.Where(r => r.Col == col).ToList();
            var solidRocksInColumn = solidRocks[col].OrderDescending().ToList();
            ShiftFromHighToLow(roundRocksInColumn, solidRocksInColumn, Direction.South);
        }
    }

    private void ShiftEast(List<CoordRef> roundRocks, Dictionary<int, List<int>> solidRocks)
    {
        for (int row = 0; row < _rows; row++)
        {
            var roundRocksInRow = roundRocks.Where(r => r.Row == row).ToList();
            var solidRocksInRow = solidRocks[row].OrderDescending().ToList();
            ShiftFromHighToLow(roundRocksInRow, solidRocksInRow, Direction.East);
        }
    }

    // North, West
    private void ShiftFromLowToHigh(List<CoordRef> roundRocks, List<int> solidRocks, Direction dir)
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
                if (roundRocks.Any(r => r.Col == i))
                    continue; // occupied
            }
            else if (roundRocks.Any(r => r.Row == i))
                continue; // occupied

            switch (dir)
            {
                case Direction.North:
                    var rockToMoveUp = roundRocks.FirstOrDefault(r => r.Row > i && r.Row < nextSolid);
                    if (rockToMoveUp != null)
                        rockToMoveUp.Row = i;
                    break;
                case Direction.West:
                    var rockToMoveLeft = roundRocks.FirstOrDefault(r => r.Col > i && r.Col < nextSolid);
                    if (rockToMoveLeft != null)
                        rockToMoveLeft.Col = i;
                    break;
                default:
                    break;
            }
        }
    }

    // East, South
    private void ShiftFromHighToLow(List<CoordRef> roundRocks, List<int> solidRocks, Direction dir)
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
                if (roundRocks.Any(r => r.Col == i))
                    continue; // occupied
            }
            else if (roundRocks.Any(r => r.Row == i))
                continue; // occupied

            switch (dir)
            {
                case Direction.South:
                    var rockToMoveDown = roundRocks.FirstOrDefault(r => r.Row < i && r.Row > nextSolid);
                    if (rockToMoveDown != null)
                        rockToMoveDown.Row = i;
                    break;
                case Direction.East:
                    var rockToMoveRight = roundRocks.FirstOrDefault(r => r.Col < i && r.Col > nextSolid);
                    if (rockToMoveRight != null)
                        rockToMoveRight.Col = i;
                    break;
                default:
                    break;
            }
        }
    }

    private class CoordRef(int row, int col)
    {
        public int Row { get; set; } = row;
        public int Col { get; set; } = col;
    }
}