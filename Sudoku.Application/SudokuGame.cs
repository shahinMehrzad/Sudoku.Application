using System.Collections.Immutable;

namespace Sudoku.Application;

/// <summary>
/// The state of a game in progress: the puzzle's fixed cells plus the digits the player has placed.
/// </summary>
public class SudokuGame
{
    private readonly int[] _values;
    private readonly bool[] _fixedCells;
    private readonly ImmutableArray<int> _solution;
    private readonly Dictionary<int, List<int>> _notes = new Dictionary<int, List<int>>();

    public SudokuGame(MainTable table)
    {
        _values = table.Table.ToArray();
        _fixedCells = _values.Select(v => v != 0).ToArray();
        _solution = table.Solution.ToImmutableArray();
    }

    /// <summary>Current digit of every cell (0 = empty), row by row.</summary>
    public IReadOnlyList<int> Values => _values;

    /// <summary>True if the cell was given by the puzzle and cannot be changed.</summary>
    public bool IsFixed(int index)
    {
        CheckIndex(index);
        return _fixedCells[index];
    }

    public void PlaceDigit(int index, int digit)
    {
        CheckIndex(index);
        CheckDigit(digit);
        CheckEditable(index);

        _values[index] = digit;
        ClearNotes(index);
    }

    public void ClearCell(int index)
    {
        CheckIndex(index);
        CheckEditable(index);

        _values[index] = 0;
    }

    private void CheckEditable(int index)
    {
        if (_fixedCells[index])
            throw new InvalidOperationException($"Cell {index} is part of the puzzle and cannot be changed.");
    }

    private static void CheckIndex(int index)
    {
        if (index < 0 || index > 80)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 80.");
    }

    private static void CheckDigit(int digit)
    {
        if (digit < 1 || digit > 9)
            throw new ArgumentOutOfRangeException(nameof(digit), "Digit must be between 1 and 9.");
    }

    private void CheckNotable(int index)
    {
        if (_values[index] > 0)
            throw new InvalidOperationException("Cannot add notes to a cell that already has a digit.");
    }

    public bool HasConflict(int index)
    {
        CheckIndex(index);

        var digit = _values[index];
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
            if (rowIndex != index && _values[rowIndex] == digit) return true;
            if (colIndex != index && _values[colIndex] == digit) return true;
            if (boxIndex != index && _values[boxIndex] == digit) return true;
        }

        return false;
    }

    public bool IsSolved()
    {
        for (int i = 0; i < 81; i++)
        {
            if (_values[i] != _solution[i])
                return false;
        }
        return true;
    }

    public bool HasNote(int index, int digit)
    {
        CheckIndex(index);
        CheckDigit(digit);

        return _notes.TryGetValue(index, out var notes) && notes.Contains(digit);
    }

    public void ToggleNote(int index, int digit)
    {
        CheckIndex(index);
        CheckEditable(index);
        CheckDigit(digit);
        CheckNotable(index);

        _notes.TryGetValue(index, out var notes);
        if (notes == null)
        {
            notes = new List<int>();
            _notes[index] = notes;
        }

        if (notes.Contains(digit))
            notes.Remove(digit);
        else
            notes.Add(digit);
    }

    public void ClearNotes(int index)
    {
        CheckIndex(index);
        CheckEditable(index);

        _notes.TryGetValue(index, out var notes);
        if (notes != null)
            notes.Clear();
    }
}
