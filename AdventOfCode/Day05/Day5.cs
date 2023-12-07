using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC;

public class Day5(ILogger logger, string path) : Puzzle(logger, path)
{
    private long[] _seeds = [];
    private readonly List<Range> _seedRanges = [];
    private readonly List<Map> _maps = [];

    public override void Setup()
    {
        var firstLine = ReadFromFile().First();
        _seeds = firstLine.Split(' ')[1..].ToLongArray();
        _seedRanges.AddRange(_seeds.Chunk(2).Select(x => new Range(x[0], x[0] + x[1])));

        Map map = [];
        foreach (var line in ReadFromFile(ignoreWhiteSpace: true).Skip(1))
        {
            var match = Regex.Match(line, @"(\d+)\s(\d+)\s(\d+)");
            if (!match.Success)
            {
                _maps.Add(map = []);
                continue;
            }
            var mapping = new Mapping(long.Parse(match.Groups[1].Value),
                long.Parse(match.Groups[2].Value),
                long.Parse(match.Groups[3].Value));

            map.Add(mapping);
        }
    }

    public override void SolvePart1() => _logger.Log(_seeds.Min(GetLocationFor));

    public override void SolvePart2() => _logger.Log(GetLocationRangesFor(_seedRanges).Min(r => r.Start));

    private long GetLocationFor(long seed)
    {
        long location = seed;
        foreach (var map in _maps)
            location = MapTo(location, map);
        return location;
    }

    private static long MapTo(long value, Map map)
    {
        foreach (var mapping in map)
            if (mapping.IsInRange(value))
                return value + mapping.Offset;
        return value;
    }

    private List<Range> GetLocationRangesFor(List<Range> seedRanges)
    {
        List<Range> locations = seedRanges;
        foreach (var map in _maps)
            locations = MapRangesTo(new Stack<Range>(locations), map);
        return locations;
    }

    private static List<Range> MapRangesTo(Stack<Range> ranges, Map map)
    {
        List<Range> mapped = [];
        while (ranges.Count > 0)
            MapRangeTo(ranges.Pop(), map, ranges, mapped);
        return mapped;
    }

    private static void MapRangeTo(Range range, Map map, Stack<Range> ranges, List<Range> mapped)
    {
        foreach (var mapping in map)
        {
            if (mapping.HasRangeOverlap(range, out var overlapStart, out var overlapEnd))
            {
                var mappedRange = new Range(overlapStart + mapping.Offset, overlapEnd + mapping.Offset);
                mapped.Add(mappedRange);

                if (overlapStart > range.Start)
                    ranges.Push(new Range(range.Start, overlapStart));

                if (range.End > overlapEnd)
                    ranges.Push(new Range(overlapEnd, range.End));
                return;
            }
        }
        mapped.Add(range);
    }

    public class Map : List<Mapping> { }
    public record struct Mapping(long Destination, long Source, long Length)
    {
        public readonly long Offset = Destination - Source;
        public readonly bool IsInRange(long value) => value >= Source && value < Source + Length;
        public readonly bool HasRangeOverlap(Range range, out long overlapStart, out long overlapEnd)
        {
            overlapStart = Math.Max(range.Start, Source);
            overlapEnd = Math.Min(range.End, Source + Length);
            return overlapStart < overlapEnd;
        }
    }
    public record struct Range(long Start, long End); // Start is Inclusive, End is Exclusive
}