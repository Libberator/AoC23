using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day12(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Report> _reports = [];
    private const char DAMAGED = '#', WORKING = '.', UNKNOWN = '?';

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

    private static long Recurse(Report report, Dictionary<Report, long> cache)
    {
        if (cache.TryGetValue(report, out var cachedTotal)) return cachedTotal;

        var condition = report.Condition;
        var pattern = report.Pattern;
        if (pattern.Length == 0) return condition.Span.Contains(DAMAGED) ? 0 : 1; // fail vs success

        int chunk = pattern[0];
        var latestIndex = condition.Length - (pattern.Sum() + pattern.Length) + 1;

        long total = 0;
        for (int i = 0; i <= latestIndex; i++)
        {
            // check for all the early outs to prune branches
            if (condition.Span[..i].Contains(DAMAGED)) break; // can't skip over a known damaged spring, '#'
            var endIndex = i + chunk;
            if (condition[i..endIndex].Span.Contains(WORKING)) continue; // can't slide this window on a '.'
            if (endIndex >= condition.Length) return total + 1; // reached end. yay!
            if (condition.Span[endIndex] == DAMAGED) continue; // next char is not a '.' or '?'. that's a no-no

            total += Recurse(report.Next(endIndex + 1), cache); // +1 because we need a '.' spacing between chunks
        }
        cache.Add(report, total);
        return total;
    }

    private record struct Report(ReadOnlyMemory<char> Condition, int[] Pattern) : IEquatable<Report>
    {
        public readonly Report Next(int startIndex) => new(Condition[startIndex..].TrimStart(WORKING), Pattern[1..]);
        public readonly bool Equals(Report other) => Condition.Span.SequenceEqual(other.Condition.Span) && StructuralComparisons.StructuralEqualityComparer.Equals(Pattern, other.Pattern);
        public readonly override int GetHashCode() => HashCode.Combine(Condition, StructuralComparisons.StructuralEqualityComparer.GetHashCode(Pattern));
    }
}