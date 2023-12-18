using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC;

public class Day18(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Instruction> _instructions = [];
    private readonly List<Instruction> _hexInstructions = [];

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            var matched = Regex.Match(line, @"([UDRL])\s(\d+)\s\(#(\w+)\)");

            var instruction = new Instruction(Direction(matched.Groups[1].Value[0]), 
                int.Parse(matched.Groups[2].Value));
            _instructions.Add(instruction);

            var hex = matched.Groups[3].Value;
            var hexInstruction = new Instruction(Direction(hex[^1]), Convert.ToInt64(hex[..^1], 16));
            _hexInstructions.Add(hexInstruction);
        }

        static Vector2Int Direction(char input) => input switch
        {
            'R' or '0' => Vector2Int.E,
            'D' or '1' => Vector2Int.S,
            'L' or '2' => Vector2Int.W,
            'U' or '3' => Vector2Int.N,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public override void SolvePart1()
    {
        //long edgeTotal = 0;
        var pos = Vector2Int.Zero;

        HashSet<Vector2Int> edges = [pos];
        HashSet<Vector2Int> inside = [];
        var prevDirection = Vector2Int.Zero;

        foreach (var instruction in _hexInstructions)
        {
            //edgeTotal += instruction.Value;

            bool turnedLeft = prevDirection.RotateLeft() == instruction.Direction;
            var right = instruction.Direction.RotateRight();

            if (turnedLeft)
            {
                // we turned left
                var corner = pos - instruction.Direction + right;
                inside.Add(corner);
            }

            for (var i = 0; i < instruction.Value; i++)
            {
                var prevInnerPos = pos + right;
                inside.Add(prevInnerPos);

                var nextPos = pos + instruction.Direction; // actual edge
                edges.Add(nextPos);

                var innerPos = nextPos + right;
                inside.Add(innerPos);

                //var lastInnerPos = innerPos + instruction.Direction;
                //edges.Add(lastInnerPos);

                pos = nextPos;
            }

            prevDirection = instruction.Direction;
        }

        // Fill the inside up
        inside.RemoveWhere(edges.Contains);

        int insideCount = inside.Count;

        var minY = inside.Min(v => v.Y);
        var maxY = inside.Max(v => v.Y);
        var minX = inside.Min(v => v.X);
        var maxX = inside.Max(v => v.X);
        
        
        for (int y = maxY; y >= minY; y--) // top to bottom
        {
            var insidePointAlongRow = inside.Where(v => v.Y == y).OrderBy(v => v.X).ToArray();
            var edgesAlongRow = edges.Where(v => v.Y == y).ToArray();

            for (int i = 0; i < insidePointAlongRow.Length - 1; i++)
            {
                var left = insidePointAlongRow[i];
                var right = insidePointAlongRow[i + 1];

                if (edgesAlongRow.Any(v => v.X > left.X && v.X < right.X))
                    continue;

                // nothing in-between these two. Fill in the empty space
                insideCount += right.X - left.X - 1;
            }
        }

        int total = edges.Count + insideCount;
        _logger.Log(total);
    }

    private void Floodfill(Vector2Int seed, HashSet<Vector2Int> edges, HashSet<Vector2Int> inside)
    {
        foreach (var neighbor in Vector2Int.CardinalDirections)
        {
            var pos = seed + neighbor;
            if (edges.Contains(pos) || inside.Contains(pos)) continue;
            inside.Add(pos);
            Floodfill(pos, edges, inside);
        }
    }

    public override void SolvePart2()
    {
        _logger.Log("Part 2 Answer");
    }

    private record struct Instruction(Vector2Int Direction, long Value);
}