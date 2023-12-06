using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC;

public class Day5(ILogger logger, string path) : Puzzle(logger, path)
{
    private long[] _seeds = System.Array.Empty<long>();
    private readonly List<Map> _maps = new();

    public override void Setup()
    {
        var firstLine = ReadFromFile().First();
        _seeds = firstLine.Split(' ')[1..].ToLongArray();

        Map map = new();
        foreach (var line in ReadFromFile(ignoreWhiteSpace: true).Skip(1))
        {
            var match = Regex.Match(line, @"(\d+)\s(\d+)\s(\d+)");
            if (!match.Success)
            {
                _maps.Add(map = new());
                continue;
            }
            var range = new Range(long.Parse(match.Groups[1].Value),
                long.Parse(match.Groups[2].Value),
                long.Parse(match.Groups[3].Value));

            map.Add(range);
        }
    }

    public override void SolvePart1() => _logger.Log(_seeds.Min(seed => GetLocationFor(seed)));

    private const int THREADS = 32;
    private readonly object _lock = new();

    public override void SolvePart2()
    {
        long minLocation = long.MaxValue;
        long offset = 1_000_000L;
        long start = 0;

        Parallel.For(0, THREADS, _ =>
        {
            long loc = 0;
            while (loc < minLocation)
            {
                lock (_lock)
                {
                    loc = start;
                    start += offset;
                }
                long end = loc + offset;

                do
                {
                    if (loc > minLocation) return;

                    var seed = GetSeedFor(loc);
                    if (IsValidStartingSeed(seed) && loc < minLocation)
                    {
                        minLocation = loc;
                        return;
                    }
                } while (++loc < end);
            }
        });

        _logger.Log(minLocation);
    }

    private long GetLocationFor(long seed)
    {
        long location = seed;
        foreach (var map in _maps)
            location = MapTo(location, map);
        return location;
    }

    private static long MapTo(long value, Map map)
    {
        long offset = 0;
        foreach (var range in map)
        {
            if (range.IsInRangeSource(value))
                return (value - range.Source) + range.Destination;

            if (value > range.Source + range.Length && value < range.Destination + range.Length)
                offset += range.Length;
        }
        return value - offset;
    }

    private long GetSeedFor(long location)
    {
        long seed = location;
        for (int i = _maps.Count - 1; i >= 0; i--)
            seed = ReverseMapTo(seed, _maps[i]);
        return seed;
    }

    private static long ReverseMapTo(long value, Map map)
    {
        foreach (var range in map)
        {
            if (range.IsInRangeDestination(value))
                return (value - range.Destination) + range.Source;
        }
        return value;
    }

    private bool IsValidStartingSeed(long value)
    {
        for (int i = 0; i < _seeds.Length; i += 2)
        {
            var seedStart = _seeds[i];
            var seedLength = _seeds[i + 1];
            if (value >= seedStart && value < seedStart + seedLength) return true;
        }
        return false;
    }

    public class Map : List<Range> { }
    public record struct Range(long Destination, long Source, long Length)
    {
        public readonly bool IsInRangeSource(long value) => value >= Source && value < Source + Length;
        public readonly bool IsInRangeDestination(long value) => value >= Destination && value < Destination + Length;
    }
}