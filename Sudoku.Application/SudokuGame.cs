namespace Sudoku.Application;

/// <summary>
/// The state of a game in progress: the puzzle's fixed cells plus the digits the player has placed.
/// </summary>
public class SudokuGame
{
	private readonly int[] values;
	private readonly bool[] fixedCells;

	public SudokuGame(MainTable table)
	{
		values = table.Table.ToArray();
		fixedCells = values.Select(v => v != 0).ToArray();
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
}
