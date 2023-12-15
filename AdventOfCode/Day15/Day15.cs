using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day15(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char DASH = '-', EQUALS = '=';
    private string[] _instructions = [];
    private readonly List<Lens>[] _lensBoxes = new List<Lens>[256];

    public override void Setup()
    {
        _instructions = ReadAllLines()[0].Split(',');
        for (int i = 0; i < 256; i++)
            _lensBoxes[i] = [];
    }

    public override void SolvePart1() => _logger.Log(_instructions.Sum(HashString));

    public override void SolvePart2()
    {
        PerformInstructions(_instructions, _lensBoxes);
        _logger.Log(GetFocusingPower(_lensBoxes));
    }

    private static int HashString(string label)
    {
        var total = 0;
        foreach (var c in label)
            total = HashWithChar(total, c);
        return total;
    }

    private static int HashWithChar(int value, char c) => (value + c) * 17 % 256;

    private static void PerformInstructions(string[] instructions, List<Lens>[] lensBoxes)
    {
        foreach (var instr in instructions)
        {
            if (instr.Contains(EQUALS))
            {
                var label = instr[..instr.IndexOf(EQUALS)];
                var focus = instr[^1] - '0';
                AddOrAdjustLens(label, focus, lensBoxes);
            }
            else // if (instr.Contains(DASH))
            {
                var label = instr[..instr.IndexOf(DASH)];
                RemoveLens(label, lensBoxes);
            }
        }
    }

    private static void AddOrAdjustLens(string label, int focus, List<Lens>[] lensBoxes)
    {
        // Adjust if we found one
        foreach (var box in lensBoxes)
        {
            foreach (var lens in box)
            {
                if (lens.Label != label) continue;
                lens.Focus = focus;
                return;
            }
        }
        // Create a new one if we didn't find one
        var newLens = new Lens(label, focus);
        var boxIndex = HashString(label);
        lensBoxes[boxIndex].Add(newLens);
    }

    private static void RemoveLens(string label, List<Lens>[] lensBoxes)
    {
        foreach (var box in lensBoxes)
        {
            foreach (var lens in box)
            {
                if (lens.Label != label) continue;
                box.Remove(lens);
                return;
            }
        }
    }

    private static int GetFocusingPower(List<Lens>[] lensBoxes)
    {
        int total = 0;
        for (int boxNumber = 0; boxNumber < lensBoxes.Length; boxNumber++)
        {
            var box = lensBoxes[boxNumber];
            for (int slotNumber = 0; slotNumber < box.Count; slotNumber++)
            {
                var lens = box[slotNumber];
                total += (boxNumber + 1) * (slotNumber + 1) * lens.Focus;
            }
        }
        return total;
    }

    private class Lens(string label, int focus)
    {
        public readonly string Label = label;
        public int Focus = focus;
    }
}