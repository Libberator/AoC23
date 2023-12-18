using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day17(ILogger logger, string path) : Puzzle(logger, path)
{
    private int[][] _grid = [];

    public override void Setup() => _grid = ReadAllLines().
        Select(l => Array.ConvertAll(l.ToCharArray(), c => c - '0')).ToArray();

    public override void SolvePart1() => _logger.Log(GetMinHeatLoss(_grid, 3));

    public override void SolvePart2() => _logger.Log(GetMinHeatLoss(_grid, 10, 4));

    private static int GetMinHeatLoss(int[][] grid, int maxConsective, int minConsecutive = 0)
    {
        HashSet<Snapshot> seen = [];
        PriorityQueue<Node, int> toSearch = new();
        toSearch.Enqueue(new Node(Vector2Int.Zero, Vector2Int.Zero, 0, 0), 0); // start
        var endPos = new Vector2Int(grid.Length - 1, grid[0].Length - 1);

        while (toSearch.Count > 0)
        {
            var current = toSearch.Dequeue();

            if (current.Pos == endPos)
            {
                if (current.Consecutive < minConsecutive) continue;
                return current.HeatLoss;
            }

            if (!seen.Add(current.Snapshot())) continue; // avoid endless loops

            foreach (var dir in Vector2Int.CardinalDirections)
            {
                if (dir == -current.Dir) continue; // don't turn around

                var nextPos = current.Pos + dir;
                if (!IsInGrid(grid, nextPos)) continue;

                var heatLoss = current.HeatLoss + grid[nextPos.X][nextPos.Y];
                if (dir == current.Dir || current.Dir == Vector2Int.Zero) // going in same direction
                {
                    if (current.Consecutive >= maxConsective) continue;
                    toSearch.Enqueue(new Node(nextPos, dir, current.Consecutive + 1, heatLoss), heatLoss);
                }
                else // turning left or right
                {
                    if (current.Consecutive < minConsecutive) continue;
                    toSearch.Enqueue(new Node(nextPos, dir, 1, heatLoss), heatLoss);
                }
            }
        }
        return 0;
    }

    private static bool IsInGrid(int[][] grid, Vector2Int pos) =>
        pos.X >= 0 && pos.Y >= 0 && pos.X < grid.Length && pos.Y < grid[0].Length;

    private record struct Snapshot(Vector2Int Pos, Vector2Int Dir, int Consecutive);
    private record struct Node(Vector2Int Pos, Vector2Int Dir, int Consecutive, int HeatLoss)
    {
        public readonly Snapshot Snapshot() => new(Pos, Dir, Consecutive); // don't capture HeatLoss to prevent endless loops
    }
}