using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day14(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char ROUND = 'O', SOLID = '#';

    private readonly Dictionary<int, List<int>> _solidRocksByRow = [], _solidRocksByCol = [];
    private readonly Dictionary<int, List<int>> _roundRocksByRow = [], _roundRocksByCol = [];
    private readonly Dictionary<int, int> _cache = [];
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

            List<int> roundRocksRow = [];
            _roundRocksByRow[row] = roundRocksRow;
            List<int> roundRocksCol = [];
            _roundRocksByCol[row] = roundRocksCol; // treat this row as a col

            var line = grid[row];
            for (int col = 0; col < _cols; col++)
            {
                if (line[col] == ROUND)
                    roundRocksRow.Add(col);
                else if (line[col] == SOLID)
                    solidRocksRow.Add(col);

                if (grid[col][row] == ROUND) // swizzled here to traverse column-wise
                    roundRocksCol.Add(col); // treat this col as a row
                else if (grid[col][row] == SOLID) // swizzled here to traverse column-wise
                    solidRocksCol.Add(col); // treat this col as a row
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

            if (HasHappenedBefore(_roundRocksByRow, _roundRocksByCol, i, out var prevCycle))
            {
                var period = i - prevCycle;
                var remaining = (cycles - i) % period;
                cycles = i + remaining;
                // or just break; instead - same end result, but not logically sound
            }
        }
        _logger.Log(GetScore(_roundRocksByRow));
    }

    private int GetScore(Dictionary<int, List<int>> roundRocks) => roundRocks.Sum(kvp => (_rows - kvp.Key) * kvp.Value.Count);

    private bool HasHappenedBefore(Dictionary<int, List<int>> roundRocksRow, Dictionary<int, List<int>> roundRocksCol, int cycle, out int prevCycle)
    {
        var hash = GetHash(roundRocksRow, roundRocksCol);
        if (_cache.TryGetValue(hash, out prevCycle)) return true;

        _cache.Add(hash, cycle);
        return false;

        int GetHash(Dictionary<int, List<int>> roundRocksRow, Dictionary<int, List<int>> roundRockCol) => HashCode.Combine(GetScore(roundRocksRow),
            StructuralComparisons.StructuralEqualityComparer.GetHashCode(roundRocksRow.SelectMany(kvp => kvp.Value).ToArray()),
            StructuralComparisons.StructuralEqualityComparer.GetHashCode(roundRockCol.SelectMany(kvp => kvp.Value).ToArray()));
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

    private void ShiftNorth(Dictionary<int, List<int>> roundRocks, Dictionary<int, List<int>> solidRocks)
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

                var rocksToMoveUp = roundRocksInColumn.Where(r => r > row && r < nextSolid);
                if (rocksToMoveUp.Any())
                {
                    var rockToMoveUp = rocksToMoveUp.Min();
                    Move(rockToMoveUp, col, row, col);
                }
            }
        }
    }

    private void ShiftWest(Dictionary<int, List<int>> roundRocks, Dictionary<int, List<int>> solidRocks)
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

                var rocksToMoveLeft = roundRocksInRow.Where(r => r > col && r < nextSolid);
                if (rocksToMoveLeft.Any())
                {
                    var rockToMoveLeft = rocksToMoveLeft.Min();
                    Move(row, rockToMoveLeft, row, col);
                }
            }
        }
    }

    private void ShiftSouth(Dictionary<int, List<int>> roundRocks, Dictionary<int, List<int>> solidRocks)
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

                var rocksToMoveDown = roundRocksInColumn.Where(r => r < row && r > nextSolid);
                if (rocksToMoveDown.Any())
                {
                    var rockToMoveDown = rocksToMoveDown.Max();
                    Move(rockToMoveDown, col, row, col);
                }
            }
        }
    }

    private void ShiftEast(Dictionary<int, List<int>> roundRocks, Dictionary<int, List<int>> solidRocks)
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

                var rocksToMoveRight = roundRocksInRow.Where(r => r < col && r > nextSolid);
                if (rocksToMoveRight.Any())
                {
                    var rockToMoveRight = rocksToMoveRight.Max();
                    Move(row, rockToMoveRight, row, col);
                }
            }
        }
    }
}