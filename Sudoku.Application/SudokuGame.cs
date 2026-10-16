using System.Collections.Immutable;
using System.Text.Json;

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
        : this(table.Table.ToArray(), table.Table.Select(v => v != 0).ToArray(), table.Solution.ToArray())
    {
    }

    private SudokuGame(int[] values, bool[] fixedCells, int[] solution)
    {
        _values = values;
        _fixedCells = fixedCells;
        _solution = solution.ToImmutableArray();
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

        Apply(index, () =>
        {
            _values[index] = digit;
            _notes.Remove(index);
        });
    }

    public void ClearCell(int index)
    {
        CheckIndex(index);
        CheckEditable(index);

        Apply(index, () => _values[index] = 0);
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

    /// <summary>How many times <see cref="Validate"/> has been called. Undo does not reduce it.</summary>
    public int ChecksUsed { get; private set; }

    /// <summary>
    /// Checks the digits the player has entered against the solution and returns the indexes of the wrong ones,
    /// in ascending order. Empty and fixed cells are never wrong. Every call counts as one check.
    /// </summary>
    public IReadOnlyList<int> Validate()
    {
        ChecksUsed++;

        return Enumerable.Range(0, 81)
            .Where(i => !_fixedCells[i] && _values[i] != 0 && _values[i] != _solution[i])
            .ToList();
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

        Apply(index, () =>
        {
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
        });
    }

    public void ClearNotes(int index)
    {
        CheckIndex(index);
        CheckEditable(index);

        Apply(index, () => _notes.Remove(index));
    }

    /// <summary>The number of most recent changes that can be undone.</summary>
    public const int MaxHistory = 5;

    private readonly List<CellChange> _undo = new List<CellChange>();
    private readonly Stack<CellChange> _redo = new Stack<CellChange>();

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public void Undo()
    {
        if (!CanUndo)
            throw new InvalidOperationException("There is nothing to undo.");

        var change = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);
        Restore(change.Index, change.Before);
        _redo.Push(change);
    }

    public void Redo()
    {
        if (!CanRedo)
            throw new InvalidOperationException("There is nothing to redo.");

        var change = _redo.Pop();
        Restore(change.Index, change.After);
        _undo.Add(change);
    }

    // Runs a change on one cell and records it in the history. Changes that alter nothing are not recorded.
    private void Apply(int index, Action change)
    {
        var before = Capture(index);
        change();
        var after = Capture(index);

        if (before.Value == after.Value && before.Notes.SequenceEqual(after.Notes))
            return;

        _undo.Add(new CellChange(index, before, after));
        if (_undo.Count > MaxHistory)
            _undo.RemoveAt(0);
        _redo.Clear();
    }

    private CellState Capture(int index) =>
        new CellState(_values[index], _notes.TryGetValue(index, out var notes) ? notes.Order().ToArray() : []);

    private void Restore(int index, CellState state)
    {
        _values[index] = state.Value;
        if (state.Notes.Length == 0)
            _notes.Remove(index);
        else
            _notes[index] = state.Notes.ToList();
    }

    /// <summary>Copies the current state so the game can be saved. The undo/redo history is not included.</summary>
    public GameSnapshot ToSnapshot() => new GameSnapshot
    {
        ChecksUsed = ChecksUsed,
        Values = _values.ToArray(),
        FixedCells = _fixedCells.ToArray(),
        Solution = _solution.ToArray(),
        Notes = _notes
            .Where(n => n.Value.Count > 0)
            .ToDictionary(n => n.Key, n => n.Value.Order().ToArray()),
    };

    /// <summary>Rebuilds a game from a snapshot, with an empty undo/redo history.</summary>
    /// <exception cref="InvalidDataException">The snapshot is not a valid game.</exception>
    public static SudokuGame FromSnapshot(GameSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateSnapshot(snapshot);

        var game = new SudokuGame(snapshot.Values.ToArray(), snapshot.FixedCells.ToArray(), snapshot.Solution);
        game.ChecksUsed = snapshot.ChecksUsed;
        foreach (var (index, digits) in snapshot.Notes)
            game._notes[index] = digits.ToList();
        return game;
    }

    public string ToJson() => JsonSerializer.Serialize(ToSnapshot());

    /// <exception cref="InvalidDataException">The text is not a valid saved game.</exception>
    public static SudokuGame FromJson(string json)
    {
        GameSnapshot? snapshot;
        try
        {
            snapshot = JsonSerializer.Deserialize<GameSnapshot>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The saved game is not valid JSON.", ex);
        }

        if (snapshot == null)
            throw new InvalidDataException("The saved game is empty.");

        return FromSnapshot(snapshot);
    }

    private static void ValidateSnapshot(GameSnapshot s)
    {
        if (s.Version != GameSnapshot.CurrentVersion)
            throw new InvalidDataException($"Unsupported saved game version {s.Version}.");
        if (s.Values == null || s.FixedCells == null || s.Solution == null || s.Notes == null)
            throw new InvalidDataException("The saved game is missing data.");
        if (s.Values.Length != 81 || s.FixedCells.Length != 81 || s.Solution.Length != 81)
            throw new InvalidDataException("The saved game must have 81 cells.");
        if (s.ChecksUsed < 0)
            throw new InvalidDataException("The number of checks cannot be negative.");
        if (s.Values.Any(v => v < 0 || v > 9))
            throw new InvalidDataException("Cell values must be between 0 and 9.");
        if (!IsCompleteGrid(s.Solution))
            throw new InvalidDataException("The saved solution is not a valid Sudoku grid.");

        for (int i = 0; i < 81; i++)
        {
            if (s.FixedCells[i] && s.Values[i] != s.Solution[i])
                throw new InvalidDataException($"Fixed cell {i} does not match the solution.");
        }

        foreach (var (index, digits) in s.Notes)
        {
            if (index < 0 || index > 80 || s.FixedCells[index] || s.Values[index] != 0)
                throw new InvalidDataException($"Cell {index} cannot have notes.");
            if (digits == null || digits.Any(d => d < 1 || d > 9) || digits.Distinct().Count() != digits.Length)
                throw new InvalidDataException($"Cell {index} has invalid notes.");
        }
    }

    // True if every row, column and box holds each of 1..9 exactly once.
    private static bool IsCompleteGrid(int[] cells)
    {
        for (int unit = 0; unit < 9; unit++)
        {
            var row = Enumerable.Range(0, 9).Select(i => cells[unit * 9 + i]);
            var column = Enumerable.Range(0, 9).Select(i => cells[i * 9 + unit]);
            var box = Enumerable.Range(0, 9).Select(i => cells[(unit / 3 * 3 + i / 3) * 9 + unit % 3 * 3 + i % 3]);

            foreach (var cellsOfUnit in new[] { row, column, box })
                if (!cellsOfUnit.Order().SequenceEqual(Enumerable.Range(1, 9)))
                    return false;
        }

        return true;
    }

    private record CellState(int Value, int[] Notes);

    private record CellChange(int Index, CellState Before, CellState After);
}
