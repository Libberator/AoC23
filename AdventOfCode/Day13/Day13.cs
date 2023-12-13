using System.Collections.Generic;

namespace AoC;

public class Day13(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<string[]> _patterns = [];

    public override void Setup()
    {
        var data = ReadAllLines();

        int startIndex = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (!string.IsNullOrEmpty(data[i])) continue;
            _patterns.Add(data[startIndex..i]);
            startIndex = i + 1;
        }
        if (!string.IsNullOrEmpty(data[^1]) && data.Length - startIndex > 1)
            _patterns.Add(data[startIndex..]); // in case data ends w/o an empty line
    }

    public override void SolvePart1() => _logger.Log(GetTotalScore(_patterns, withSmudge: false));

    public override void SolvePart2() => _logger.Log(GetTotalScore(_patterns, withSmudge: true));

    private static int GetTotalScore(List<string[]> patterns, bool withSmudge)
    {
        int total = 0;
        foreach (var pattern in patterns)
        {
            if (TryFindHorizontalReflection(pattern, out int row, withSmudge))
                total += 100 * row;
            else if (TryFindVerticalReflection(pattern, out int col, withSmudge))
                total += col;
        }
        return total;
    }

    private static bool TryFindVerticalReflection(string[] pattern, out int foundCol, bool withSmudge = false) =>
        TryFindHorizontalReflection(pattern.Transpose(), out foundCol, withSmudge);

    private static bool TryFindHorizontalReflection(string[] pattern, out int foundRow, bool withSmudge = false)
    {
        foundRow = 0;
        for (int center = 0; center < pattern.Length - 1; center++)
        {
            if (!TryCheckReflection(pattern, center, withSmudge)) continue;
            foundRow = center + 1;
            return true;
        }
        return false;
    }

    private static bool TryCheckReflection(string[] pattern, int center, bool withSmudge)
    {
        int differences = 0;
        for (int i = 0; i <= center; i++)
        {
            var left = center - i;
            var right = center + i + 1;

            if (left < 0 || right >= pattern.Length) break;

            if (pattern[left] != pattern[right])
            {
                if (!withSmudge) return false;

                differences += CountDifferences(pattern[left], pattern[right]);
                if (differences > 1) return false;
            }
        }
        return !withSmudge || differences == 1;
    }

    private static int CountDifferences(string left, string right)
    {
        int differences = 0;
        for (int i = 0; i < left.Length; i++)
            differences += left[i] != right[i] ? 1 : 0;
        return differences;
    }
}