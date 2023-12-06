using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC;

public class Day6(ILogger logger, string path) : Puzzle(logger, path)
{
    private int[] _times = Array.Empty<int>();
    private long[] _distances = Array.Empty<long>();

    public override void Setup()
    {
        var data = ReadAllLines();
        var times = Regex.Matches(data[0], @"(\d+)");
        var dists = Regex.Matches(data[1], @"(\d+)");
        _times = times.Select(m => int.Parse(m.Value)).ToArray();
        _distances = dists.Select(m => long.Parse(m.Value)).ToArray();
    }

    public override void SolvePart1() => _logger.Log(Enumerable.Range(0, _times.Length).Select(i => WaysToWin(_times[i], _distances[i])).Product());

    public override void SolvePart2()
    {
        var time = _times.Aggregate((a, b) => a * (int)NearestPowerOf10(b) + b);
        var distance = _distances.Aggregate((a, b) => a * NearestPowerOf10(b) + b);
        _logger.Log(WaysToWin(time, distance));
    }

    private static long WaysToWin(int time, long distance)
    {
        for (long i = 1; i < time; i++)
            if (i * (time - i) > distance) return time - 2 * i + 1;
        return 0;
    }

    // 0-9 -> 10, 10-99 -> 100, 100-999 -> 1000, 1000-9999 -> 10000, etc.
    private static long NearestPowerOf10(long number) => 10 * (long)Math.Pow(10, Math.Floor(Math.Log10(number)));
}