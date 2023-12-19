using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AoC;

public class Day19(ILogger logger, string path) : Puzzle(logger, path)
{
    private record struct Part(int X, int M, int A, int S)
    {
        public readonly int Total = X + M + A + S;
    }

    private class Workflow()
    {
        public readonly List<Rule> Rules = [];
        public string IfNonePass = "";
    }

    private record struct Rule(Condition Condition, string ResultIfTrue);
    private record struct Condition(string Letter, bool IsGreaterThan, int Value)
    {
        public readonly bool PassesCondition(Part part)
        {
            var partValue = Letter switch
            {
                "x" => part.X,
                "m" => part.M,
                "a" => part.A,
                "s" => part.S,
                _ => throw new IndexOutOfRangeException()
            };
            return IsGreaterThan ? partValue > Value : partValue < Value;
        }
    }

    private readonly List<Part> _parts = [];
    private readonly Dictionary<string, Workflow> _workflows = [];
    private Workflow? _startingWorkflow;

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

            if (firstPart) // workflows
            {
                var split = line.Split('{');
                var id = split[0];
                var rules = split[1][..^1].Split(',');

                var workflow = new Workflow();

                for (int i = 0; i < rules.Length - 1; i++)
                {
                    var match = Regex.Match(rules[i], @"(\w)([><])(\d+):(\w+)");
                    var letter = match.Groups[1].Value;
                    var isGreaterSymbol = match.Groups[2].Value;
                    var number = int.Parse(match.Groups[3].Value);
                    var ifTrue = match.Groups[4].Value;

                    var condition = new Condition(letter, isGreaterSymbol == ">", number);
                    var rule = new Rule(condition, ifTrue);
                    workflow.Rules.Add(rule);
                }
                var ifAllFalse = rules[^1];
                workflow.IfNonePass = ifAllFalse;
                _workflows.Add(id, workflow);
                if (id == "in") _startingWorkflow = workflow;
            }
            else // parts
            {
                var matches = Regex.Matches(line, @"\d+");
                var x = int.Parse(matches[0].Value);
                var m = int.Parse(matches[1].Value);
                var a = int.Parse(matches[2].Value);
                var s = int.Parse(matches[3].Value);

                var workflow = new Part(x, m, a, s);
                _parts.Add(workflow);
            }
        }
    }

    public override void SolvePart1()
    {
        int total = 0;
        foreach (var part in _parts)
        {
            if (IsAcceptedPart(part, _startingWorkflow!))
                total += part.Total;
        }

        _logger.Log(total);
    }

    private bool IsAcceptedPart(Part part, Workflow workflow)
    {
        foreach (var rule in workflow.Rules)
        {
            if (rule.Condition.PassesCondition(part))
            {
                var result = rule.ResultIfTrue;
                if (result == "A") return true;
                if (result == "R") return false;

                return IsAcceptedPart(part, _workflows[result]);
            }
        }
        // none of the rules passed
        var ifNonePass = workflow.IfNonePass;
        if (ifNonePass == "A") return true;
        if (ifNonePass == "R") return false;
        return IsAcceptedPart(part, _workflows[ifNonePass]);
    }

    public override void SolvePart2()
    {
        long total = 0;

        var range = new LongRange(1, 4000);
        var partRange = new PartRange(range, range, range, range);
        var workflow = _workflows["in"];

        List<PartRange> validRanges = [];

        ProcessRange(partRange, workflow, validRanges);

        foreach (var validRange in validRanges)
        {
            total += validRange.Total;
        }

        _logger.Log(total);
    }

    private void ProcessRange(PartRange range, Workflow workflow, List<PartRange> validRanges)
    {
        if (range.Total == 0) return;
        // "failing" remainder values
        var fail = range;

        foreach (var rule in workflow.Rules)
        {
            var condition = rule.Condition;
            var resultIfTrue = rule.ResultIfTrue;

            var pass = fail; // copy current failing version

            switch (condition.Letter)
            {
                case "x":
                    if (condition.IsGreaterThan)
                        fail.X.Split(condition.Value, out fail.X, out pass.X);
                    else
                        fail.X.Split(condition.Value - 1, out pass.X, out fail.X);
                    break;
                case "m":
                    if (condition.IsGreaterThan)
                        fail.M.Split(condition.Value, out fail.M, out pass.M);
                    else
                        fail.M.Split(condition.Value - 1, out pass.M, out fail.M);
                    break;
                case "a":
                    if (condition.IsGreaterThan)
                        fail.A.Split(condition.Value, out fail.A, out pass.A);
                    else
                        fail.A.Split(condition.Value - 1, out pass.A, out fail.A);
                    break;
                case "s":
                    if (condition.IsGreaterThan)
                        fail.S.Split(condition.Value, out fail.S, out pass.S);
                    else
                        fail.S.Split(condition.Value - 1, out pass.S, out fail.S);
                    break;
            }

            if (resultIfTrue == "A")
            {
                if (pass.Total != 0)
                    validRanges.Add(pass); // yay
                continue;
            }
            if (resultIfTrue == "R") continue; // end branch
            ProcessRange(pass, _workflows[resultIfTrue], validRanges); // split into valid path
        }

        // none of the rules passed
        var ifNonePass = workflow.IfNonePass;
        if (ifNonePass == "A")
        {
            if (fail.Total != 0)
                validRanges.Add(fail);
            return;
        }
        if (ifNonePass == "R") return;
        ProcessRange(fail, _workflows[ifNonePass], validRanges);
    }


    private struct PartRange(LongRange x, LongRange m, LongRange a, LongRange s)
    {
        public LongRange X = x;
        public LongRange M = m;
        public LongRange A = a;
        public LongRange S = s;

        public readonly long Total => X.Length * M.Length * A.Length * S.Length;
    }

    private record struct LongRange(long Start, long End)
    {
        public readonly long Length => End - Start + 1;

        public readonly void Split(long value, out LongRange left, out LongRange right)
        {
            var rightResult = new LongRange(value + 1, End);
            var leftResult = new LongRange(Start, value);
            right = rightResult;
            left = leftResult;
        }
    }
}