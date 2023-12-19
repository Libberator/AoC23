using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AoC;

public partial class Day18(ILogger logger, string path) : Puzzle(logger, path)
{
    private record struct Instruction(Vector2Long Direction, long Value);

    private readonly List<Instruction> _instructions = [];
    private readonly List<Instruction> _hexInstructions = [];

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            var match = Regex.Match(line, @"([DLRU])\s(\d+)\s\(#(\w+)\)");
            var letter = match.Groups[1].Value[0]; // as char
            var number = match.Groups[2].Value;
            var hex = match.Groups[3].Value;

            var instruction = new Instruction(Direction(letter), long.Parse(number));
            _instructions.Add(instruction);

            var hexInstruction = new Instruction(Direction(hex[^1]), Convert.ToInt64(hex[..^1], 16));
            _hexInstructions.Add(hexInstruction);
        }

        static Vector2Long Direction(char input) => input switch
        {
            'R' or '0' => Vector2Long.E,
            'D' or '1' => Vector2Long.S,
            'L' or '2' => Vector2Long.W,
            'U' or '3' => Vector2Long.N,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public override void SolvePart1() => _logger.Log(CalculatePolygonArea(_instructions));

    public override void SolvePart2() => _logger.Log(CalculatePolygonArea(_hexInstructions));

    // this uses a slightly adjusted shoelace formula: https://en.wikipedia.org/wiki/Shoelace_formula
    private static long CalculatePolygonArea(List<Instruction> instructions)
    {
        long shoelaceArea = 0;
        long perimeter = 0;
        var prev = Vector2Long.Zero;

        foreach (var instruction in instructions)
        {
            var curr = prev + instruction.Direction * instruction.Value; // next vertex

            shoelaceArea += prev.X * curr.Y - curr.X * prev.Y;
            perimeter += instruction.Value;
            
            prev = curr;
        }

        shoelaceArea = Math.Abs(shoelaceArea) / 2;
        // we add half the perimiter and 1 to adjusted for formula not including the lower bounds
        return shoelaceArea + perimeter / 2 + 1;
    }
}