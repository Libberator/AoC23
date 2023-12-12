using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day12(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Report> _reports = [];
    private const char DAMAGED = '#', OPERATIONAL = '.', UNKNOWN = '?';

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            var split = line.Split(' ');
            var condition = split[0].AsMemory();
            var pattern = split[1].Split(',').ToIntArray();
            _reports.Add(new Report(condition, pattern));
        }
    }

    public override void SolvePart1() => _logger.Log(_reports.Sum(r => TotalConfigurations(r)));

    public override void SolvePart2() => _logger.Log(_reports.Sum(r => TotalConfigurations(r, 5)));

    private long TotalConfigurations(Report report, int repeats = 1)
    {
        if (repeats == 1) return Recurse(report, []);

        var repeatedCondition = string.Join(UNKNOWN, Enumerable.Repeat(report.Condition, repeats));
        var repeatedPattern = Enumerable.Repeat(report.Pattern, repeats).SelectMany(arr => arr).ToArray();
        var expandedReport = new Report(repeatedCondition.AsMemory(), repeatedPattern);
        return Recurse(expandedReport, []);
    }

    private static long Recurse(Report report, Dictionary<Report, long> cache, long total = 0)
    {
        if (cache.TryGetValue(report, out var cachedTotal)) return cachedTotal;

        var condition = report.Condition;
        var pattern = report.Pattern;
        if (pattern.Length == 0) return condition.Span.Contains(DAMAGED) ? 0 : 1;

        int chunk = pattern[0];
        var latestIndex = condition.Length - (pattern.Sum() + pattern.Length) + 1;

        for (int i = 0; i <= latestIndex; i++)
        {
            if (condition.Span[..i].Contains(DAMAGED)) break;
            var match = condition[i..(i + chunk)];
            if (match.Span.Contains(OPERATIONAL)) continue;

            var indexAfter = i + chunk;
            if (indexAfter >= condition.Length) return total + 1;
            if (condition.Span[indexAfter] == DAMAGED) continue;

            total += Recurse(report.Next(indexAfter + 1), cache);
        }
        cache.Add(report, total);
        return total;
    }

    private record struct Report(ReadOnlyMemory<char> Condition, int[] Pattern) : IEquatable<Report>
    {
        public readonly Report Next(int startIndex) => new(Condition[startIndex..].TrimStart(OPERATIONAL), Pattern[1..]);
        public readonly bool Equals(Report other) => Condition.Span.SequenceEqual(other.Condition.Span) && StructuralComparisons.StructuralEqualityComparer.Equals(Pattern, other.Pattern);
        public readonly override int GetHashCode() => HashCode.Combine(Condition, StructuralComparisons.StructuralEqualityComparer.GetHashCode(Pattern));
    }
}