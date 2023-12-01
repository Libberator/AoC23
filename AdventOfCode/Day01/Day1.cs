using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day1(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char ZERO = '0';
    private static readonly Dictionary<string, int> _numbers = new()
    {
        ["one"] = 1, ["two"] = 2, ["three"] =  3,
        ["four"] = 4, ["five"] = 5, ["six"] = 6,
        ["seven"] = 7, ["eight"] = 8, ["nine"] = 9
    };

    public override void SolvePart1()
    {
        int total = 0;
        foreach (var line in ReadFromFile())
            total += 10 * FirstDigit(line) + LastDigit(line);

        _logger.Log(total);
    }

    private static int FirstDigit(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            if (!char.IsDigit(line[i])) continue;
            return line[i] - ZERO;
        }
        return 0;
    }

    private static int LastDigit(string line)
    {
        for (int i = line.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(line[i])) continue;
            return line[i] - ZERO;
        }
        return 0;
    }

    public override void SolvePart2()
    {
        int total = 0;
        foreach (var line in ReadFromFile())
            total += 10 * FirstNumber(line) + LastNumber(line);

        _logger.Log(total);
    }

    private static int FirstNumber(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            if (!char.IsDigit(line[i])) continue;

            if (TryMatchFirstWord(line[..i], out int value))
                return value;

            return line[i] - ZERO;
        }
        return TryMatchFirstWord(line, out int val) ? val : 0;
    }

    private static int LastNumber(string line)
    {
        for (int i = line.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(line[i])) continue;

            if (TryMatchLastWord(line[i..], out int value))
                return value;

            return line[i] - ZERO;
        }
        return TryMatchLastWord(line, out int val) ? val : 0;
    }

    private static bool TryMatchFirstWord(string line, out int value)
    {
        value = _numbers.Where(kvp => line.Contains(kvp.Key))
            .OrderBy(kvp => line.IndexOf(kvp.Key))
            .FirstOrDefault().Value;
        return value > 0;
    }

    private static bool TryMatchLastWord(string line, out int value)
    {
        value = _numbers.Where(kvp => line.Contains(kvp.Key))
            .OrderBy(kvp => line.LastIndexOf(kvp.Key))
            .LastOrDefault().Value;
        return value > 0;
    }
}