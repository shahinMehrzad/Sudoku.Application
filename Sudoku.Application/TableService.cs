namespace Sudoku.Application;

public class TableService
{
	private readonly Random rnd;

	/// <summary>Creates a service that generates different puzzles every time.</summary>
	public TableService()
	{
		rnd = new Random();
	}

	/// <summary>
	/// Creates a service whose puzzles are reproducible: two services created with the same seed
	/// produce the same puzzles, in the same order, for the same levels.
	/// </summary>
	public TableService(int seed)
	{
		rnd = new Random(seed);
	}

	public MainTable StartGame(MatchLevelEnum level)
	{
		int target = EmptyCells(level);

		while (true)
		{
			var solution = new int[81];
			Solve(solution, 0);

			var cells = (int[])solution.Clone();
			int removed = 0;

			// Remove cells one by one, keeping a removal only if the puzzle still has a single solution.
			foreach (var index in RandomIndexes())
			{
				if (removed == target)
					break;

				int digit = cells[index];
				cells[index] = 0;

				if (CountSolutions(cells, 2) == 1)
					removed++;
				else
					cells[index] = digit;
			}

			// This grid could not be emptied enough; try again with a new one.
			if (removed < target)
				continue;

			var table = new MainTable();
			table.Solution.AddRange(solution);
			table.Table.AddRange(cells);
			return table;
		}
	}

	// Counts solutions of the puzzle, stopping early once `limit` is reached.
	private static int CountSolutions(int[] cells, int limit)
	{
		int bestIndex = -1;
		int bestCount = 10;

		// Pick the empty cell with the fewest candidates to keep the search small.
		for (int i = 0; i < 81; i++)
		{
			if (cells[i] != 0)
				continue;

			int count = 0;
			for (int digit = 1; digit <= 9; digit++)
				if (IsAllowed(cells, i, digit))
					count++;

			if (count < bestCount)
			{
				bestCount = count;
				bestIndex = i;
			}
		}

		if (bestIndex == -1)
			return 1;

		int solutions = 0;
		for (int digit = 1; digit <= 9 && solutions < limit; digit++)
		{
			if (!IsAllowed(cells, bestIndex, digit))
				continue;

			cells[bestIndex] = digit;
			solutions += CountSolutions(cells, limit - solutions);
		}

		cells[bestIndex] = 0;
		return solutions;
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

