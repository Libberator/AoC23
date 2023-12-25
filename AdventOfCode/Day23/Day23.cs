using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AoC;

public class Day23(ILogger logger, string path) : Puzzle(logger, path)
{
    private string[] _grid = [];
    private const char PATH = '.', FOREST = '#', SLOPE_R = '>', SLOPE_D = 'v'; //, SLOPE_L = '<', SLOPE_U = '^', ;
    private Vector2Int _start, _end;

    private readonly List<Node> _nodes = [];

    public override void Setup()
    {
        _grid = ReadAllLines();
        _start = new Vector2Int(0, _grid[0].IndexOf(PATH));
        _end = new Vector2Int(_grid.Length - 1, _grid[^1].IndexOf(PATH));

        InitializeNodes(_start, _end);
    }

    private void InitializeNodes(Vector2Int start, Vector2Int end)
    {
        throw new NotImplementedException();
    }

    public override void SolvePart1()
    {
        var path = FindLongestPath(_start, _end);
        //LogPath(path);
        _logger.Log(path.Count);
    }

    private void LogPath(List<Vector2Int> path)
    {
        for (int x = path.Min(p => p.X); x <= path.Max(p => p.X); x++)
        {
            var sb = new StringBuilder();
            for (int y = path.Min(p => p.Y); y <= path.Max(p => p.Y); y++)
            {
                var pos = new Vector2Int(x, y);
                sb.Append(path.Contains(pos) ? 'O' : '.');
            }
            Console.WriteLine(sb.ToString());
        }
    }

    public override void SolvePart2()
    {
        _logger.Log("Part 2 Answer");
    }

    private List<Vector2Int> FindLongestPath(Vector2Int start, Vector2Int end)
    {
        Queue<Node> leadingNodes = new(); 
        leadingNodes.Enqueue(new Node(start, 0));
        Node longestNode = new(start, 0);
        return [];
        //while (leadingNodes.Count > 0)
        //{
        //    var current = leadingNodes.Dequeue();

        //    if (current.Pos == end)
        //    {
        //        if (current.PathTotal > longestNode.PathTotal)
        //        {
        //            longestNode = current;
        //            Console.WriteLine($"New longest path: {current.PathTotal} ({leadingNodes.Count} remaining)");
        //        }
        //        continue;
        //    }

        //    foreach (var nextPos in current.GetNeighbors(_grid))
        //    {
        //        var neighbor = new Node(nextPos, current.PathTotal + 1);
        //        neighbor.Path.AddRange(current.Path);
        //        neighbor.Path.Add(current.Pos);
        //        leadingNodes.Enqueue(neighbor);
        //    }
        //}

        //return longestNode.Path;
    }

    private record class Node(Vector2Int Pos, int CostToNode)
    {
        public List<Node> Connected = [];
    }

}