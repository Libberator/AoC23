using System;
using System.Collections.Generic;
using System.Text;

namespace AoC;

public class Day13(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<string[]> _patterns = [];

    public override void Setup()
    {
        var data = ReadAllLines();

        int startIndex = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (string.IsNullOrEmpty(data[i]))
            {
                _patterns.Add(data[startIndex..i]);
                startIndex = i + 1;
            }

            if (i == data.Length - 1 && i - startIndex > 1)
                _patterns.Add(data[startIndex..]);
        }
    }

    public override void SolvePart1()
    {
        var total = 0; // col count + 100 * row count
        int i = 0;
        foreach (var pattern in _patterns)
        {
            if (TryFindHorizontalReflection(pattern, out int row))
            {
                total += 100 * row;
            }
            else if (TryFindVerticalReflection(pattern, out int col))
            {
                total += col;
            }
            i++;
        }

        _logger.Log(total);
    }

    public override void SolvePart2()
    {
        var total = 0; // col count + 100 * row count
        int i = 0;
        foreach (var pattern in _patterns)
        {
            if (TryFindHorizontalReflection(pattern, out int row, withSmudge: true))
            {
                total += 100 * row;
            }
            else if (TryFindVerticalReflection(pattern, out int col, withSmudge: true))
            {
                total += col;
            }
            i++;
        }

        _logger.Log(total);
    }

    private bool TryFindHorizontalReflection(string[] pattern, out int foundRow, bool withSmudge = false)
    {
        foundRow = -1;

        for (int center = 0; center < pattern.Length - 1; center++)
        {
            bool found = true;
            int differences = 0;
            for (int i = 0; i <= center; i++)
            {
                var left = center - i;
                var right = center + i + 1;

                if (left < 0 || right >= pattern.Length) break;
                if (pattern[left] != pattern[right])
                {
                    if (withSmudge)
                    {
                        for (int j = 0; j < pattern[left].Length; j++)
                        {
                            if (pattern[left][j] != pattern[right][j])
                                differences++;
                        }
                        if (differences > 1)
                            break;
                    }
                    else
                    {
                        found = false;
                        break;
                    }
                }
            }

            if (withSmudge)
            {
                if (differences == 1)
                {
                    foundRow = center + 1;
                    break;
                }
            }
            else if (found)
            {
                foundRow = center + 1;
                break;
            }
        }
        return foundRow != -1;
    }

    private bool TryFindVerticalReflection(string[] pattern, out int foundCol, bool withSmudge = false)
    {
        var transposed = TransposeArray(pattern);
        return TryFindHorizontalReflection(transposed, out foundCol, withSmudge);
    }

    static string[] TransposeArray(string[] original)
    {
        int numRows = original.Length;
        int numCols = original[0].Length;

        // Create a new array to store the transposed result
        string[] transposed = new string[numCols];

        // Iterate over columns
        for (int col = 0; col < numCols; col++)
        {
            // Use StringBuilder for efficient string concatenation
            StringBuilder columnBuilder = new(numRows);

            // Iterate over rows
            for (int row = 0; row < numRows; row++)
            {
                // Append the character at the current column and row to the StringBuilder
                columnBuilder.Append(original[row][col]);
            }

            // Store the concatenated column in the transposed array
            transposed[col] = columnBuilder.ToString();
        }

        return transposed;
    }
}