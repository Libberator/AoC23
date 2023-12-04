using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day3(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Number> _numbers = new();
    private readonly List<Gear> _gears = new();

    public override void Setup()
    {
        var data = ReadAllLines();
        var rows = data.Length;
        var cols = data[0].Length;

        for (int row = 0; row < rows; row++)
        {
            var line = data[row];
            for (int col = 0; col < cols; col++)
            {
                if (!char.IsDigit(line[col])) continue;
                var number = GetNumber(data, line, row, ref col);
                _numbers.Add(number);
            }
        }
    }

    public override void SolvePart1() => _logger.Log(_numbers.Sum(n => n.IsAdjacentToSymbol ? n.Value : 0));

    public override void SolvePart2() => _logger.Log(_gears.Sum(g => g.Ratio));

    private Number GetNumber(string[] data, string line, int row, ref int col)
    {
        var c = line[col];
        var number = new Number(c - '0');
        CheckSymbolAdjacency(data, number, row, col);

        while (++col < line.Length)
        {
            c = line[col];
            if (!char.IsDigit(c)) break;

            number.AppendDigit(c - '0');
            if (!number.IsAdjacentToSymbol)
                CheckSymbolAdjacency(data, number, row, col);
        }

        return number;
    }

    private void CheckSymbolAdjacency(string[] data, Number number, int row, int col)
    {
        for (int y = -1; y <= 1; y++)
        {
            if (row + y < 0 || row + y >= data.Length) continue;
            var line = data[row + y];
            for (int x = -1; x <= 1; x++)
            {
                if ((col == 0 && y == 0) || col + x < 0 || col + x >= line.Length) continue;
                var c = line[col + x];
                if (char.IsDigit(c) || c == '.') continue;

                number.IsAdjacentToSymbol = true;
                if (c == '*')
                    AddNumberToGear(number, new Vector2Int(row + y, col + x));

                return;
            }
        }
    }

    private void AddNumberToGear(Number number, Vector2Int pos)
    {
        var gear = _gears.Find(g => g.Pos == pos);
        if (gear == null)
            _gears.Add(gear = new Gear(pos));
        gear.AdjacentNumbers.Add(number);
    }

    public class Number(int value)
    {
        public int Value = value;
        public bool IsAdjacentToSymbol = false;

        public void AppendDigit(int digit) => Value = (10 * Value) + digit;
    }

    public class Gear(Vector2Int pos)
    {
        public Vector2Int Pos = pos;
        public readonly List<Number> AdjacentNumbers = new();

        public int Ratio => AdjacentNumbers.Count == 2 ? AdjacentNumbers.Select(n => n.Value).Product() : 0;
    }
}