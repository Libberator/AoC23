﻿using System.Collections.Generic;
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
            _stepsMap[match.Groups[1].Value] = [match.Groups[2].Value, match.Groups[3].Value];
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
            int direction = _directions[steps++ % _directions.Length] == 'L' ? 0 : 1;
            current = _stepsMap[current][direction];
        }
        return steps;
    }

    private long TraverseFromMultiple(List<string> current)
    {
        int steps = 0;
        int[] firstOccurances = new int[current.Count];
        long[] periodicity = new long[current.Count]; // steps between each cyclical END-occurance

        while (periodicity.Any(p => p == 0))
        {
            int direction = _directions[steps++ % _directions.Length] == 'L' ? 0 : 1;

            for (int i = 0; i < current.Count; i++)
            {
                current[i] = _stepsMap[current[i]][direction];
                if (current[i][^1] == 'Z') // at an END node
                {
                    if (firstOccurances[i] == 0)
                        firstOccurances[i] = steps;
                    else if (periodicity[i] == 0)
                        periodicity[i] = steps - firstOccurances[i];
                }
            }
        }

        return periodicity.Aggregate(1L, (a, b) => a = Utils.LeastCommonMultiple(a, b));
    }
}