namespace Sudoku.Application;

public class TableService
{
	private readonly Random rnd = new Random();

	public MainTable StartGame(MatchLevelEnum level)
	{
		var cells = new int[81];
		Solve(cells, 0);

		var table = new MainTable();
		table.Solution.AddRange(cells);

		foreach (var index in RandomIndexes().Take(EmptyCells(level)))
			cells[index] = 0;
		table.Table.AddRange(cells);

		return table;
	}

	private static int EmptyCells(MatchLevelEnum level) => level switch
	{
		MatchLevelEnum.Easy => 36,
		MatchLevelEnum.Medium => 45,
		MatchLevelEnum.Hard => 52,
		_ => throw new ArgumentOutOfRangeException(nameof(level)),
	};

	private IEnumerable<int> RandomIndexes()
	{
		int[] indexes = Enumerable.Range(0, 81).ToArray();
		rnd.Shuffle(indexes);
		return indexes;
	}

	// Backtracking: fills cells[index..80] with random valid digits.
	private bool Solve(int[] cells, int index)
	{
		if (index == 81)
			return true;

		foreach (var digit in Shuffled())
		{
			if (!IsAllowed(cells, index, digit))
				continue;

			cells[index] = digit;
			if (Solve(cells, index + 1))
				return true;
		}

		cells[index] = 0;
		return false;
	}

	private static bool IsAllowed(int[] cells, int index, int digit)
	{
		int row = index / 9;
		int col = index % 9;
		int boxRow = row / 3 * 3;
		int boxCol = col / 3 * 3;

		for (int i = 0; i < 9; i++)
		{
			if (cells[row * 9 + i] == digit) return false;
			if (cells[i * 9 + col] == digit) return false;
			if (cells[(boxRow + i / 3) * 9 + boxCol + i % 3] == digit) return false;
		}

		return true;
	}

	private int[] Shuffled()
	{
		int[] digits = [1, 2, 3, 4, 5, 6, 7, 8, 9];
		rnd.Shuffle(digits);
		return digits;
	}
}

