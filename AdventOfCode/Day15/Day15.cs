using System.Collections.Generic;

namespace AoC;

public class Day15(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char DASH = '-', EQUALS = '=';
    private string[] _instructions = [];

    public override void Setup() => _instructions = ReadAllLines()[0].Split(',');

    public override void SolvePart1()
    {
        int total = 0;
        foreach (var instruction in _instructions)
            total += HashString(instruction);
        _logger.Log(total);
    }

    public override void SolvePart2()
    {
        var lensBoxes = new List<Lens>[256];
        for (int i = 0; i < 256; i++)
            lensBoxes[i] = [];

        PerformInstructions(_instructions, lensBoxes);

        _logger.Log(GetFocusingPower(lensBoxes));
    }

    private static byte HashString(string label)
    {
        byte total = 0;
        foreach (var c in label)
            total = (byte)((total + c) * 17); // casting to byte does modulus 256 automatically
        return total;
    }

    private static void PerformInstructions(string[] instructions, List<Lens>[] lensBoxes)
    {
        foreach (var instr in instructions)
        {
            if (instr.Contains(EQUALS))
            {
                var label = instr[..instr.IndexOf(EQUALS)];
                var focus = instr[^1] - '0';
                var boxIndex = HashString(label);
                AddOrAdjustLens(label, focus, boxIndex, lensBoxes);
            }
            else // if (instr.Contains(DASH))
            {
                var label = instr[..instr.IndexOf(DASH)];
                var boxIndex = HashString(label);
                RemoveLens(label, boxIndex, lensBoxes);
            }
        }
    }

    private static void AddOrAdjustLens(string label, int focus, int boxIndex, List<Lens>[] lensBoxes)
    {
        // Adjust if we found one
        var box = lensBoxes[boxIndex];
        var lens = box.Find(l => l.Label.Equals(label));
        if (lens != null)
        {
            lens.Focus = focus;
            return;
        }

        // Create a new one if we didn't find one
        var newLens = new Lens(label, focus);
        box.Add(newLens);
    }

    private static void RemoveLens(string label, int boxIndex, List<Lens>[] lensBoxes)
    {
        var box = lensBoxes[boxIndex];
        var lens = box.Find(l => l.Label.Equals(label));
        if (lens != null)
            box.Remove(lens);
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