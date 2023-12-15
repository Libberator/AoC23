using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day14(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char ROUND = 'O', SOLID = '#';

    private readonly List<List<int>> _roundRocksByRow = [], _solidRocksByRow = [];
    private readonly List<List<int>> _roundRocksByCol = [], _solidRocksByCol = [];
    private readonly List<int> _cache = [];
    private int _rows, _cols;

    public override void Setup()
    {
        var grid = ReadAllLines();
        _rows = grid.Length;
        _cols = grid[0].Length;
        for (int row = 0; row < _rows; row++)
        {
            List<int> roundRocksRow = [];
            _roundRocksByRow.Add(roundRocksRow);
            List<int> solidRocksRow = [];
            _solidRocksByRow.Add(solidRocksRow);

            List<int> roundRocksCol = [];
            _roundRocksByCol.Add(roundRocksCol);
            List<int> solidRocksCol = [];
            _solidRocksByCol.Add(solidRocksCol);

            var line = grid[row];
            for (int col = 0; col < _cols; col++)
            {
                // row-wise
                if (line[col] == ROUND) roundRocksRow.Add(col);
                else if (line[col] == SOLID) solidRocksRow.Add(col);

                // column-wise. swizzled
                if (grid[col][row] == ROUND) roundRocksCol.Add(col);
                else if (grid[col][row] == SOLID) solidRocksCol.Add(col);
            }
        }
    }

    public override void SolvePart1()
    {
        ShiftNorth(_roundRocksByCol, _solidRocksByCol);
        _logger.Log(GetScore(_roundRocksByRow));
    }

    public override void SolvePart2()
    {
        var cycles = 1_000_000_000;
        for (int i = 0; i < cycles; i++)
        {
            DoCycle();

            if (HasHappenedBefore(_roundRocksByRow, out var prevCycle))
            {
                var period = i - prevCycle;
                var remaining = (cycles - i) % period;
                cycles = i + remaining;
                // or just break; instead - same end result, but not logically sound
            }
        }
        _logger.Log(GetScore(_roundRocksByRow));
    }

    private int GetScore(List<List<int>> roundRocks) => roundRocks.Select((row, index) => (_rows - index) * row.Count).Sum();

    private bool HasHappenedBefore(List<List<int>> roundRocksRow, out int prevCycle)
    {
        var hash = GetHash(GetScore(roundRocksRow), roundRocksRow);
        prevCycle = _cache.IndexOf(hash);
        if (prevCycle != -1) return true;

        _cache.Add(hash);
        return false;

        static int GetHash(int score, List<List<int>> roundRocksRow) =>
            HashCode.Combine(score, StructuralComparisons.StructuralEqualityComparer.GetHashCode(roundRocksRow.SelectMany(row => row).ToArray()));
    }

    private void DoCycle()
    {
        ShiftNorth(_roundRocksByCol, _solidRocksByCol);
        ShiftWest(_roundRocksByRow, _solidRocksByRow);
        ShiftSouth(_roundRocksByCol, _solidRocksByCol);
        ShiftEast(_roundRocksByRow, _solidRocksByRow);
    }

    private void Move(int fromRow, int fromCol, int toRow, int toCol)
    {
        _roundRocksByRow[fromRow].Remove(fromCol);
        _roundRocksByRow[toRow].Add(toCol);
        _roundRocksByCol[fromCol].Remove(fromRow);
        _roundRocksByCol[toCol].Add(toRow);
    }

    private void ShiftNorth(List<List<int>> roundRocks, List<List<int>> solidRocks)
    {
        for (int col = 0; col < _cols; col++)
        {
            var roundRocksInColumn = roundRocks[col];
            var solidRocksInColumn = solidRocks[col];

            int solidIndex = 0;
            var nextSolid = solidRocksInColumn.Count > solidIndex ? solidRocksInColumn[solidIndex++] : _cols;

            for (int row = 0; row < _rows; row++)
            {
                if (row == nextSolid)
                {
                    nextSolid = solidRocksInColumn.Count > solidIndex ? solidRocksInColumn[solidIndex++] : _cols;
                    continue;
                }

                if (roundRocksInColumn.Any(r => r == row))
                    continue; // occupied

                // TODO: look for optimizations here - leapfrog from the back instead of doing all 1-at-a-time?

                var rocksToMoveUp = roundRocksInColumn.Where(r => r > row && r < nextSolid).ToList();
                for (int i = rocksToMoveUp.Count - 1; i >= 0; i--)
                    Move(rocksToMoveUp[i], col, row++, col);
            }
        }
    }

    private void ShiftWest(List<List<int>> roundRocks, List<List<int>> solidRocks)
    {
        for (int row = 0; row < _rows; row++)
        {
            var roundRocksInRow = roundRocks[row];
            var solidRocksInRow = solidRocks[row];

            int solidIndex = 0;
            var nextSolid = solidRocksInRow.Count > solidIndex ? solidRocksInRow[solidIndex++] : _rows;

            for (int col = 0; col < _cols; col++)
            {
                if (col == nextSolid)
                {
                    nextSolid = solidRocksInRow.Count > solidIndex ? solidRocksInRow[solidIndex++] : _rows;
                    continue;
                }

                if (roundRocksInRow.Any(r => r == col))
                    continue; // occupied

                var rocksToMoveLeft = roundRocksInRow.Where(r => r > col && r < nextSolid).ToList();
                for (int i = rocksToMoveLeft.Count - 1; i >= 0; i--)
                    Move(row, rocksToMoveLeft[i], row, col++);
            }
        }
    }

    private void ShiftSouth(List<List<int>> roundRocks, List<List<int>> solidRocks)
    {
        for (int col = 0; col < _cols; col++)
        {
            var roundRocksInColumn = roundRocks[col];
            var solidRocksInColumn = solidRocks[col].OrderDescending().ToList();

            int solidIndex = 0;
            var nextSolid = solidRocksInColumn.Count > solidIndex ? solidRocksInColumn[solidIndex++] : -1;

            for (int row = _rows - 1; row >= 0; row--)
            {
                if (row == nextSolid)
                {
                    nextSolid = solidRocksInColumn.Count > solidIndex ? solidRocksInColumn[solidIndex++] : -1;
                    continue;
                }

                if (roundRocksInColumn.Any(r => r == row))
                    continue; // occupied

                var rocksToMoveDown = roundRocksInColumn.Where(r => r < row && r > nextSolid).ToList();
                for (int i = rocksToMoveDown.Count - 1; i >= 0; i--)
                    Move(rocksToMoveDown[i], col, row--, col);
            }
        }
    }

    private void ShiftEast(List<List<int>> roundRocks, List<List<int>> solidRocks)
    {
        for (int row = 0; row < _rows; row++)
        {
            var roundRocksInRow = roundRocks[row];
            var solidRocksInRow = solidRocks[row].OrderDescending().ToList();

            int solidIndex = 0;
            var nextSolid = solidRocksInRow.Count > solidIndex ? solidRocksInRow[solidIndex++] : -1;

            for (int col = _cols - 1; col >= 0; col--)
            {
                if (col == nextSolid)
                {
                    nextSolid = solidRocksInRow.Count > solidIndex ? solidRocksInRow[solidIndex++] : -1;
                    continue;
                }

                if (roundRocksInRow.Any(r => r == col))
                    continue; // occupied

                var rocksToMoveRight = roundRocksInRow.Where(r => r < col && r > nextSolid).ToList();
                for (int i = rocksToMoveRight.Count - 1; i >= 0; i--)
                    Move(row, rocksToMoveRight[i], row, col--);
            }
        }
    }
}