using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day12(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Report> _reports = [];
    private readonly List<Report> _unfoldedReports = [];
    private const char DAMAGED = '#', WORKING = '.', UNKNOWN = '?';

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            var split = line.Split(' ');
            var condition = split[0].AsMemory();
            var groups = split[1].Split(',').ToIntArray();
            _reports.Add(new Report(condition, groups));

            var unfoldedCondition = string.Join(UNKNOWN, Enumerable.Repeat(condition, 5)).AsMemory();
            var unfoldedGroups = Enumerable.Repeat(groups, 5).SelectMany(arr => arr).ToArray();
            _unfoldedReports.Add(new Report(unfoldedCondition, unfoldedGroups));
        }
    }

    public override void SolvePart1() => _logger.Log(_reports.Sum(r => Recurse(r, [])));

    public override void SolvePart2() => _logger.Log(_unfoldedReports.Sum(r => Recurse(r, [])));

    private static long Recurse(Report report, Dictionary<Report, long> cache)
    {
        if (cache.TryGetValue(report, out var cachedTotal)) return cachedTotal;

        var condition = report.Condition;
        var groups = report.Groups;
        if (groups.Length == 0) return condition.Span.Contains(DAMAGED) ? 0 : 1; // fail vs success

        int group = groups[0];
        var latestIndex = condition.Length - (groups.Sum() + groups.Length) + 1; // furthest we can slide window

        long total = 0;
        for (int i = 0; i <= latestIndex; i++)
        {
            // check for all the early outs to prune branches
            if (condition.Span[..i].Contains(DAMAGED)) break; // can't skip over a known damaged spring, '#'
            var endIndex = i + group;
            if (condition[i..endIndex].Span.Contains(WORKING)) continue; // can't slide this window on a '.'
            if (endIndex >= condition.Length) return total + 1; // this successful group reached end. yay!
            if (condition.Span[endIndex] == DAMAGED) continue; // next char is not a '.' or '?'. that's a no-no

            total += Recurse(report.Next(endIndex + 1), cache); // +1 because we need a '.' spacing between groups
        }
        cache.Add(report, total);
        return total;
    }

    private readonly record struct Report(ReadOnlyMemory<char> Condition, int[] Groups) : IEquatable<Report>
    {
        public readonly Report Next(int startIndex) => new(Condition[startIndex..].TrimStart(WORKING), Groups[1..]);
        public readonly bool Equals(Report other) => Condition.Span.SequenceEqual(other.Condition.Span) && StructuralComparisons.StructuralEqualityComparer.Equals(Groups, other.Groups);
        public readonly override int GetHashCode() => HashCode.Combine(Condition, StructuralComparisons.StructuralEqualityComparer.GetHashCode(Groups));
    }
}