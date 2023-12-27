using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day23(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char PATH = '.', FOREST = '#', SLOPE_E = '>', SLOPE_S = 'v', SLOPE_W = '<', SLOPE_N = '^';
    private string[] _grid = [];
    private Vector2Int _start, _end;
    private readonly List<Vector2Int> _nodes = [];
    private record struct Node(Vector2Int Pos, int Steps, int Index);

    public override void Setup()
    {
        _grid = ReadAllLines();
        _start = new(0, _grid[0].IndexOf(PATH));
        _end = new(_grid.Length - 1, _grid[^1].IndexOf(PATH));

        _nodes.Add(_start);
        for (int row = 1; row < _grid.Length - 1; row++)
        {
            var line = _grid[row];
            for (int col = 1; col < line.Length - 1; col++)
            {
                if (line[col] == FOREST) continue;
                var neighbors = Vector2Int.CardinalDirections.Count(dir => _grid[row + dir.X][col + dir.Y] != FOREST);
                if (neighbors >= 3)
                    _nodes.Add(new(row, col));
            }
        }
        _nodes.Add(_end);
    }

    public override void SolvePart1()
    {
        var graph = CreateGraph(_nodes, DirectedGraphMapping);
        var longestPath = FindLongestPath(graph, _start, _end);
        _logger.Log(longestPath);
    }

    public override void SolvePart2()
    {
        var graph = CreateGraph(_nodes, UndirectedGraphMapping);
        var longestPath = FindLongestPath(graph, _start, _end);
        _logger.Log(longestPath);
    }

    private Dictionary<Vector2Int, List<Node>> CreateGraph(List<Vector2Int> nodes, Func<char, Vector2Int[]> nextDir)
    {
        Dictionary<Vector2Int, List<Node>> graph = [];
        foreach (var node in nodes)
        {
            HashSet<Vector2Int> seen = [node];
            List<Node> connectedNodes = graph[node] = [];
            Stack<(Vector2Int, int)> stack = [];
            stack.Push((node, 0));

            while (stack.Count > 0)
            {
                var (pos, steps) = stack.Pop();

                if (nodes.Contains(pos) && steps != 0)
                {
                    var index = nodes.IndexOf(pos);
                    connectedNodes.Add(new Node(pos, steps, index));
                    continue;
                }

                foreach (var dir in nextDir(_grid[pos.X][pos.Y]))
                {
                    var next = pos + dir;
                    if (next.X < 0 || next.Y < 0 || next.X >= _grid.Length || next.Y >= _grid[0].Length) continue;
                    if (_grid[next.X][next.Y] == FOREST) continue;
                    if (!seen.Add(next)) continue;
                    stack.Push((next, steps + 1));
                }
            }
        }
        return graph;
    }

    private static Vector2Int[] DirectedGraphMapping(char c) => c switch
    {
        PATH => Vector2Int.CardinalDirections,
        SLOPE_E => [Vector2Int.N],
        SLOPE_S => [Vector2Int.E],
        SLOPE_W => [Vector2Int.S],
        SLOPE_N => [Vector2Int.W],
        _ => throw new NotImplementedException()
    };

    private static Vector2Int[] UndirectedGraphMapping(char _) => Vector2Int.CardinalDirections;

    // DFS
    private static int FindLongestPath(Dictionary<Vector2Int, List<Node>> graph, Vector2Int current, Vector2Int end, long seenBitMask = 1L)
    {
        if (current == end) return 0;

        int steps = int.MinValue;

        foreach (var next in graph[current])
        {
            var nextBitMask = 1L << next.Index;
            if ((seenBitMask & nextBitMask) > 0) continue;
            steps = Math.Max(steps, next.Steps + FindLongestPath(graph, next.Pos, end, seenBitMask | nextBitMask));
        }

        return steps;
    }
}