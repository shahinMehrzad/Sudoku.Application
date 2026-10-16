namespace Sudoku.Application;

/// <summary>
/// A plain-data copy of a game in progress, used to save and restore it.
/// The undo/redo history is not part of it.
/// </summary>
public class GameSnapshot
{
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;

    /// <summary>How many times the player has checked their digits. Saves made without it load as 0.</summary>
    public int ChecksUsed { get; set; }

    /// <summary>How many hints the player has used. Saves made without it load as 0.</summary>
    public int HintsUsed { get; set; }

    /// <summary>Current digit of every cell (0 = empty), row by row.</summary>
    public int[] Values { get; set; } = [];

    /// <summary>True for the cells given by the puzzle.</summary>
    public bool[] FixedCells { get; set; } = [];

    /// <summary>The complete solution, row by row.</summary>
    public int[] Solution { get; set; } = [];

    /// <summary>Pencil marks by cell index. Cells without notes are left out.</summary>
    public Dictionary<int, int[]> Notes { get; set; } = new();
}
