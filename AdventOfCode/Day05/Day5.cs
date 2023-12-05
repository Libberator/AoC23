using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC;

public class Day5(ILogger logger, string path) : Puzzle(logger, path)
{
    private long[] _seeds = Array.Empty<long>();
    private readonly List<List<Map>> _almanac = new();

    public override void Setup()
    {
        var firstLine = ReadFromFile().First();
        _seeds = firstLine.Split(' ')[1..].ToLongArray();

        List<Map> maps = new();
        foreach (var line in ReadFromFile(ignoreWhiteSpace: true).Skip(1))
        {
            var match = Regex.Match(line, @"(\d+)\s(\d+)\s(\d+)");
            if (!match.Success)
            {
                _almanac.Add(maps = new());
                continue;
            }
            var map = new Map(long.Parse(match.Groups[1].Value),
                long.Parse(match.Groups[2].Value),
                long.Parse(match.Groups[3].Value));

            maps.Add(map);
        }
    }

    public override void SolvePart1()
    {
        long minLocation = long.MaxValue;

        foreach (var seed in _seeds)
        {
            long location = GetLocationFor(seed);
            minLocation = Math.Min(location, minLocation);
        }

        _logger.Log(minLocation);
    }

    public override void SolvePart2()
    {
        long minLocation = 0;

        while (true)
        {
            var seed = GetSeedFor(minLocation);
            if (IsValidStartingSeed(seed)) break;
            minLocation++;
        }

        _logger.Log(minLocation);
    }

    private long GetLocationFor(long seed)
    {
        long location = seed;
        foreach (var maps in _almanac)
            location = MapTo(location, maps);
        return location;
    }

    private static long MapTo(long input, List<Map> maps)
    {
        long offset = 0;

        foreach (var map in maps)
        {
            if (input >= map.Source && input <= map.Source + map.Length)
            {
                return (input - map.Source) + map.Destination;
            }
            if (input > map.Source + map.Length && input < map.Destination + map.Length)
            {
                offset += map.Length;
            }
        }
        return input - offset;
    }

    private long GetSeedFor(long location)
    {
        long seed = location;
        for (int i = _almanac.Count - 1; i >= 0; i--)
        {
            var maps = _almanac[i];
            seed = ReverseMapTo(seed, maps);
        }
        return seed;
    }

    private static long ReverseMapTo(long input, List<Map> maps)
    {
        long offset = 0;

        foreach (var map in maps)
        {
            if (input >= map.Destination && input <= map.Destination + map.Length)
            {
                return (input - map.Destination) + map.Source;
            }
            if (input > map.Destination + map.Length && input < map.Source + map.Length)
            {
                offset += map.Length;
            }
        }
        return input - offset;
    }

    private bool IsValidStartingSeed(long seed)
    {
        for (int i = 0; i < _seeds.Length; i += 2)
        {
            var seedStart = _seeds[i];
            var range = _seeds[i + 1];
            if (seed >= seedStart && seed < seedStart + range) return true;
        }

        return false;
    }

    public struct Map(long dest, long source, long length)
    {
        public long Destination = dest;
        public long Source = source;
        public long Length = length;
    }
}