namespace Sudoku.Application;

/// <summary>
/// A suggestion for the next move: the cell to look at and the digit that belongs there.
/// A UI can show just the cell first and the digit later.
/// </summary>
public record Hint(int Index, int Digit);
