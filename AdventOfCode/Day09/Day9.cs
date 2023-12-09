using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day9(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<int[]> _data = [];

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
            _data.Add(line.Split(' ').ToIntArray());
    }

    public override void SolvePart1() => _logger.Log(_data.Sum(NextValueInPattern));

    public override void SolvePart2() => _logger.Log(_data.Sum(PrevValueInPattern));

    private static int NextValueInPattern(int[] pattern)
    {
        var diffStack = GetDiffStack(pattern);
        int nextValue = 0;

        while (diffStack.Count > 0)
            nextValue += diffStack.Pop()[^1];

        return nextValue;
    }

    private static int PrevValueInPattern(int[] pattern)
    {
        var diffStack = GetDiffStack(pattern);
        int prevValue = 0;

        while (diffStack.Count > 0)
            prevValue = diffStack.Pop()[0] - prevValue;

        return prevValue;
    }

    private static Stack<int[]> GetDiffStack(int[] pattern)
    {
        Stack<int[]> diffStack = [];
        int[] diff = pattern;

        while (!diff.All(d => d == 0))
        {
            diffStack.Push(diff);
            diff = GetDiff(diff);
        }

        return diffStack;
    }

    private static int[] GetDiff(int[] pattern)
    {
        int[] diff = new int[pattern.Length - 1];

        for (int i = 0; i < diff.Length; i++)
            diff[i] = pattern[i + 1] - pattern[i];

        return diff;
    }
}