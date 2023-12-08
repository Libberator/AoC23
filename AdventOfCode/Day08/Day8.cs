using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC;

public class Day8(ILogger logger, string path) : Puzzle(logger, path)
{
    private string _directions = "";
    private const string START = "AAA", END = "ZZZ";
    private readonly Dictionary<string, string[]> _stepsMap = [];

    public override void Setup()
    {
        _directions = ReadAllLines()[0];
        foreach (var line in ReadFromFile(ignoreWhiteSpace: true).Skip(1))
        {
            var match = Regex.Match(line, @"(.+)\s=\s\((.+),\s(.+)\)");
            _stepsMap.Add(match.Groups[1].Value, [match.Groups[2].Value, match.Groups[3].Value]);
        }
    }

    public override void SolvePart1() => _logger.Log(TraverseFrom(START));

    public override void SolvePart2()
    {
        var allStarts = _stepsMap.Keys.Where(k => k[^1] == 'A').ToList();
        var steps = TraverseFromMultiple(allStarts);
        _logger.Log(steps);
    }

    private int TraverseFrom(string current)
    {
        int steps = 0;
        while (current != END)
        {
            int direction = _directions[steps % _directions.Length] == 'L' ? 0 : 1;
            steps++;

            current = _stepsMap[current][direction];
        }
        return steps;
    }

    private long TraverseFromMultiple(List<string> current)
    {
        int steps = 0;
        int[] firstOccurances = new int[current.Count];
        int[] frequencies = new int[current.Count];

        while (frequencies.Any(f => f == 0))
        {
            int direction = _directions[steps % _directions.Length] == 'L' ? 0 : 1;
            steps++;

            for (int i = 0; i < current.Count; i++)
            {
                current[i] = _stepsMap[current[i]][direction];
                if (current[i][^1] == 'Z')
                {
                    if (firstOccurances[i] == 0)
                    {
                        firstOccurances[i] = steps;
                        continue;
                    }
                    if (frequencies[i] == 0)
                        frequencies[i] = steps - firstOccurances[i];
                }
            }
        }

        var primeFactors = frequencies.Select(f => f.FirstPrimeFactor());
        long result = frequencies[0] / frequencies[0].FirstPrimeFactor(); // start with 2nd prime factor
        return primeFactors.Aggregate(result, (result, pf) => result *= pf);
    }
}