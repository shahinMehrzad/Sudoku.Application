using Sudoku.Application;

namespace Sudoku.Tests;

public class TableServiceTests
{
	private static MainTable NewTable() => new TableService().StartGame(MatchLevelEnum.Easy);

	[Fact]
	public void StartGame_SolutionHasEightyOneCellsWithDigitsOneToNine()
	{
		var cells = NewTable().Solution;

		Assert.Equal(81, cells.Count);
		Assert.All(cells, c => Assert.InRange(c, 1, 9));
	}

	[Fact]
	public void StartGame_EveryRowColumnAndBoxHasDigitsOneToNineOnce()
	{
		var service = new TableService();

		for (int run = 0; run < 1000; run++)
		{
			var cells = service.StartGame(MatchLevelEnum.Easy).Solution;

			for (int unit = 0; unit < 9; unit++)
			{
				AssertComplete(Enumerable.Range(0, 9).Select(i => cells[unit * 9 + i]));            // row
				AssertComplete(Enumerable.Range(0, 9).Select(i => cells[i * 9 + unit]));            // column
				AssertComplete(Enumerable.Range(0, 9).Select(i =>                                   // box
					cells[(unit / 3 * 3 + i / 3) * 9 + unit % 3 * 3 + i % 3]));
			}
		}
	}

	[Fact]
	public void StartGame_CalledTwiceOnSameService_ReturnsIndependentFullTables()
	{
		var service = new TableService();

		var first = service.StartGame(MatchLevelEnum.Easy);
		var second = service.StartGame(MatchLevelEnum.Easy);

		Assert.Equal(81, first.Solution.Count);
		Assert.Equal(81, second.Solution.Count);
		Assert.NotSame(first, second);
	}

	[Fact]
	public void StartGame_ProducesDifferentTables()
	{
		var service = new TableService();

		var distinct = Enumerable.Range(0, 20)
			.Select(_ => string.Join("", service.StartGame(MatchLevelEnum.Easy).Solution))
			.Distinct()
			.Count();

		Assert.True(distinct > 1);
	}

	[Theory]
	[InlineData(MatchLevelEnum.Easy, 36)]
	[InlineData(MatchLevelEnum.Medium, 45)]
	[InlineData(MatchLevelEnum.Hard, 52)]
	public void StartGame_EmptiesTheExpectedNumberOfCells(MatchLevelEnum level, int expectedEmpty)
	{
		var table = new TableService().StartGame(level);

		Assert.Equal(81, table.Table.Count);
		Assert.Equal(expectedEmpty, table.Table.Count(c => c == 0));
	}

	[Fact]
	public void StartGame_RemainingCellsMatchSolution()
	{
		var table = NewTable();

		for (int i = 0; i < 81; i++)
			Assert.True(table.Table[i] == 0 || table.Table[i] == table.Solution[i]);
	}

	[Theory]
	[InlineData(MatchLevelEnum.Easy)]
	[InlineData(MatchLevelEnum.Medium)]
	[InlineData(MatchLevelEnum.Hard)]
	public void StartGame_PuzzleHasExactlyOneSolution(MatchLevelEnum level)
	{
		var service = new TableService();

		for (int run = 0; run < 20; run++)
		{
			var table = service.StartGame(level);

			Assert.Equal(1, CountSolutions(table.Table.ToArray(), 2));
		}
	}

	// Independent brute-force counter (stops at `limit`), so the test does not rely on the code under test.
	private static int CountSolutions(int[] cells, int limit)
	{
		int index = Array.IndexOf(cells, 0);
		if (index == -1)
			return 1;

		int solutions = 0;
		for (int digit = 1; digit <= 9 && solutions < limit; digit++)
		{
			if (!Fits(cells, index, digit))
				continue;

			cells[index] = digit;
			solutions += CountSolutions(cells, limit - solutions);
			cells[index] = 0;
		}

		return solutions;
	}

	private static bool Fits(int[] cells, int index, int digit)
	{
		int row = index / 9, col = index % 9;
		for (int i = 0; i < 9; i++)
		{
			if (cells[row * 9 + i] == digit) return false;
			if (cells[i * 9 + col] == digit) return false;
			if (cells[(row / 3 * 3 + i / 3) * 9 + col / 3 * 3 + i % 3] == digit) return false;
		}
		return true;
	}

	private static void AssertComplete(IEnumerable<int> unit) =>
		Assert.Equal(Enumerable.Range(1, 9), unit.OrderBy(x => x));
}
