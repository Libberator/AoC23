using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AoC;

public class Day14(ILogger logger, string path) : Puzzle(logger, path)
{
    enum Direction { North, West, South, East }
    enum Tile : byte { Empty, Round, Cube }
    private const char ROUND = 'O', CUBE = '#';

    private readonly List<List<Tile>> _grid = [];
    private readonly List<int> _cache = [];
    private int _size = 0;

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
            _grid.Add(line.Select(MapCharToTile).ToList());

        _size = _grid.Count;

        static Tile MapCharToTile(char c) => c switch
        {
            ROUND => Tile.Round,
            CUBE => Tile.Cube,
            _ => Tile.Empty,
        };
    }

    public override void SolvePart1()
    {
        Shift(_grid, Direction.North);
        _logger.Log(GetScore(_grid));
    }

    public override void SolvePart2()
    {
        var cycles = 1_000_000_000;
        for (int i = 0; i < cycles; i++)
        {
            DoCycle();

            if (HasHappenedBefore(_grid, out var prevCycle))
            {
                var period = i - prevCycle;
                var remaining = (cycles - i) % period;
                cycles = i + remaining;
                // or just break; instead - same end result, but not logically sound
            }
        }
        _logger.Log(GetScore(_grid));
    }

    private int GetScore(List<List<Tile>> grid) => grid.Select((row, index) => (_size - index) * row.Count(t => t == Tile.Round)).Sum();

    private void DoCycle()
    {
        Shift(_grid, Direction.North);
        Shift(_grid, Direction.West);
        Shift(_grid, Direction.South);
        Shift(_grid, Direction.East);
    }

    private bool HasHappenedBefore(List<List<Tile>> grid, out int prevCycle)
    {
        var hash = GetHash(grid);
        prevCycle = _cache.IndexOf(hash);
        if (prevCycle != -1) return true;

        _cache.Add(hash);
        return false;

        static int GetHash(List<List<Tile>> grid)
        {
            int hash = 0, rows = grid.Count;
            for (int row = 0; row < rows; row++)
            {
                var rowTiles = grid[row];
                for (int col = 0; col < rowTiles.Count; col++)
                {
                    if (rowTiles[col] != Tile.Round) continue;
                    hash ^= 1 << ((row * rows + col) & 31); // XOR hash approach
                }
            }
            return hash;
        }
    }

    private void MoveTile(int fromRow, int fromCol, int toRow, int toCol)
    {
        _grid[fromRow][fromCol] = Tile.Empty;
        _grid[toRow][toCol] = Tile.Round;
    }

    private void MoveTileNorth(int i, int from, int to) => MoveTile(from, i, to, i);
    private void MoveTileWest(int i, int from, int to) => MoveTile(i, from, i, to);
    private void MoveTileSouth(int i, int from, int to) => MoveTile(_size - 1 - from, i, _size - 1 - to, i);
    private void MoveTileEast(int i, int from, int to) => MoveTile(i, _size - 1 - from, i, _size - 1 - to);

    private void Shift(List<List<Tile>> grid, Direction dir)
    {
        Action<int, int, int> moveAction = dir switch
        {
            Direction.North => MoveTileNorth,
            Direction.West => MoveTileWest,
            Direction.South => MoveTileSouth,
            Direction.East => MoveTileEast,
            _ => throw new IndexOutOfRangeException()
        };

        Parallel.For(0, _size, i =>
        {
            var tiles = dir switch
            {
                Direction.North => grid.Select(row => row[i]).ToList(),
                Direction.West => grid[i],
                Direction.South => grid.Select(row => row[i]).Reverse().ToList(),
                Direction.East => grid[i].AsEnumerable().Reverse().ToList(),
                _ => throw new IndexOutOfRangeException(),
            };

            var nextCubeIndex = -1;
            GetNextCubeIndex(tiles, ref nextCubeIndex);

            for (int j = 0; j < _size; j++)
            {
                if (j == nextCubeIndex)
                {
                    GetNextCubeIndex(tiles, ref nextCubeIndex);
                    continue;
                }

                for (int fromIndex = j; fromIndex < nextCubeIndex; fromIndex++)
                {
                    if (tiles[fromIndex] != Tile.Round) continue;
                    moveAction(i, fromIndex, j);
                    j++;
                }

                j = nextCubeIndex - 1;
            }
        });

        static void GetNextCubeIndex(List<Tile> tiles, ref int nextCubeIndex)
        {
            nextCubeIndex = tiles.FindIndex(nextCubeIndex + 1, t => t == Tile.Cube);
            if (nextCubeIndex == -1) nextCubeIndex = tiles.Count;
        }
    }
}