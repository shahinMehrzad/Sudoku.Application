using System.Collections.Immutable;

namespace Sudoku.Application;

/// <summary>
/// The state of a game in progress: the puzzle's fixed cells plus the digits the player has placed.
/// </summary>
public class SudokuGame
{
    private readonly int[] values;
    private readonly bool[] fixedCells;
    private readonly ImmutableArray<int> solution;

    public SudokuGame(MainTable table)
    {
        values = table.Table.ToArray();
        fixedCells = values.Select(v => v != 0).ToArray();
        solution = table.Solution.ToImmutableArray();
    }

    /// <summary>Current digit of every cell (0 = empty), row by row.</summary>
    public IReadOnlyList<int> Values => values;

    /// <summary>True if the cell was given by the puzzle and cannot be changed.</summary>
    public bool IsFixed(int index)
    {
        CheckIndex(index);
        return fixedCells[index];
    }

    public void PlaceDigit(int index, int digit)
    {
        CheckIndex(index);
        if (digit < 1 || digit > 9)
            throw new ArgumentOutOfRangeException(nameof(digit), "Digit must be between 1 and 9.");
        CheckEditable(index);

        values[index] = digit;
    }

    public void ClearCell(int index)
    {
        CheckIndex(index);
        CheckEditable(index);

        values[index] = 0;
    }

    private void CheckEditable(int index)
    {
        if (fixedCells[index])
            throw new InvalidOperationException($"Cell {index} is part of the puzzle and cannot be changed.");
    }

    private static void CheckIndex(int index)
    {
        if (index < 0 || index > 80)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 80.");
    }

    public bool HasConflict(int index)
    {
        CheckIndex(index);

        var digit = values[index];
        if (digit == 0)
            return false;

        int row = index / 9;
        int col = index % 9;
        int boxRow = row / 3 * 3;
        int boxCol = col / 3 * 3;

        for (int i = 0; i < 9; i++)
        {
            int rowIndex = row * 9 + i;
            int colIndex = i * 9 + col;
            int boxIndex = (boxRow + i / 3) * 9 + boxCol + i % 3;

            // A cell never conflicts with itself.
            if (rowIndex != index && values[rowIndex] == digit) return true;
            if (colIndex != index && values[colIndex] == digit) return true;
            if (boxIndex != index && values[boxIndex] == digit) return true;
        }

        return false;
    }

    public bool IsSolved()
    {
        for (int i = 0; i < 81; i++)
        {
            if (values[i] != solution[i])
                return false;
        }
        return true;
    }
}
