using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC;

public class Day19(ILogger logger, string path) : Puzzle(logger, path)
{
    private const string ACCEPTED = "A", REJECTED = "R", START = "in";

    private readonly Dictionary<string, Workflow> _workflows = [];
    private readonly List<Part> _parts = [];

    public override void Setup()
    {
        bool firstPart = true;
        foreach (var line in ReadFromFile())
        {
            if (string.IsNullOrEmpty(line))
            {
                firstPart = false;
                continue;
            }

            if (firstPart)
                ParseWorkflows(line);
            else
                ParseParts(line);
        }
    }

    private void ParseWorkflows(string line)
    {
        var split = line.Split('{');
        var id = split[0];
        var rules = split[1][..^1].Split(',');

        var workflow = new Workflow(failResult: rules[^1]);
        _workflows.Add(id, workflow);

        for (int i = 0; i < rules.Length - 1; i++)
        {
            var rule = rules[i];
            char letter = rule[0];
            bool isGreaterThan = rule[1] == '>';

            var match = Regex.Match(rule, @"(\d+):(\w+)");
            int value = int.Parse(match.Groups[1].Value);
            string passResult = match.Groups[2].Value;

            workflow.Add(new Condition(letter, isGreaterThan, value, passResult));
        }
    }

    private void ParseParts(string line)
    {
        var matches = Regex.Matches(line, @"\d+");
        var x = int.Parse(matches[0].Value);
        var m = int.Parse(matches[1].Value);
        var a = int.Parse(matches[2].Value);
        var s = int.Parse(matches[3].Value);
        _parts.Add(new Part(x, m, a, s));
    }

    public override void SolvePart1() =>
        _logger.Log(_parts.Sum(p => IsAcceptedPart(p, _workflows[START]) ? p.Total : 0));

    public override void SolvePart2()
    {
        var range = new Range(1, 4000);
        var partRange = new PartRange(x: range, m: range, a: range, s: range);
        List<PartRange> validRanges = [];

        ProcessRange(partRange, _workflows[START], validRanges);

        _logger.Log(validRanges.Sum(r => r.Total));
    }

    private bool IsAcceptedPart(Part part, Workflow workflow)
    {
        foreach (var condition in workflow)
        {
            if (condition.Passes(part))
            {
                return condition.PassResult switch
                {
                    ACCEPTED => true,
                    REJECTED => false,
                    _ => IsAcceptedPart(part, _workflows[condition.PassResult])
                };
            }
        }

        return workflow.FailResult switch
        {
            ACCEPTED => true,
            REJECTED => false,
            _ => IsAcceptedPart(part, _workflows[workflow.FailResult])
        };
    }

    private void ProcessRange(PartRange fail, Workflow workflow, List<PartRange> validRanges)
    {
        foreach (var condition in workflow)
        {
            var letter = condition.Letter; // used as an indexer

            Range failRange, passRange; // For splitting into two ranges
            if (condition.IsGreaterThan)
                fail[letter].Split(condition.Value, out failRange, out passRange);
            else
                fail[letter].Split(condition.Value - 1, out passRange, out failRange);

            var pass = fail; // copy current failing version
            pass[letter] = passRange;
            fail[letter] = failRange;

            if (condition.PassResult == ACCEPTED)
                validRanges.Add(pass);
            else if (condition.PassResult != REJECTED)
                ProcessRange(pass, _workflows[condition.PassResult], validRanges);
        }

        if (workflow.FailResult == ACCEPTED)
            validRanges.Add(fail);
        else if (workflow.FailResult != REJECTED)
            ProcessRange(fail, _workflows[workflow.FailResult], validRanges);
    }

    private class Workflow(string failResult) : List<Condition>
    {
        public readonly string FailResult = failResult;
    }

    private record struct Condition(char Letter, bool IsGreaterThan, int Value, string PassResult)
    {
        public readonly bool Passes(Part part) =>
            IsGreaterThan ? part[Letter] > Value : part[Letter] < Value;
    }

    private record struct Part(int X, int M, int A, int S)
    {
        public readonly int Total = X + M + A + S;
        public readonly int this[char letter] => letter switch
        {
            'x' => X,
            'm' => M,
            'a' => A,
            's' => S,
            _ => throw new IndexOutOfRangeException()
        };
    }

    private record struct Range(int Start, int End) // End is inclusive
    {
        public readonly long Length => End - Start + 1;
        public readonly void Split(int value, out Range left, out Range right) =>
            (left, right) = (new Range(Start, value), new Range(value + 1, End));
    }

    private struct PartRange(Range x, Range m, Range a, Range s)
    {
        public Range X = x, M = m, A = a, S = s;
        public readonly long Total => X.Length * M.Length * A.Length * S.Length;

        public Range this[char letter]
        {
            readonly get => letter switch
            {
                'x' => X,
                'm' => M,
                'a' => A,
                's' => S,
                _ => throw new IndexOutOfRangeException()
            };
            set
            {
                switch (letter)
                {
                    case 'x': X = value; break;
                    case 'm': M = value; break;
                    case 'a': A = value; break;
                    case 's': S = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
    }
}