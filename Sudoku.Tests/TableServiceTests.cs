using Sudoku.Application;

namespace Sudoku.Tests;

public class TableServiceTests
{
	private static MainTable NewTable() => new TableService().StartGame(MatchLevelEnum.Easy);

	[Fact]
	public void StartGame_ReturnsEightyOneCellsWithDigitsOneToNine()
	{
		var cells = NewTable().Table;

		Assert.Equal(81, cells.Count);
		Assert.All(cells, c => Assert.InRange(c, 1, 9));
	}

	[Fact]
	public void StartGame_EveryRowColumnAndBoxHasDigitsOneToNineOnce()
	{
		var service = new TableService();

		for (int run = 0; run < 1000; run++)
		{
			var cells = service.StartGame(MatchLevelEnum.Easy).Table;

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

		Assert.Equal(81, first.Table.Count);
		Assert.Equal(81, second.Table.Count);
		Assert.NotSame(first, second);
	}

	[Fact]
	public void StartGame_ProducesDifferentTables()
	{
		var service = new TableService();

		var distinct = Enumerable.Range(0, 20)
			.Select(_ => string.Join("", service.StartGame(MatchLevelEnum.Easy).Table))
			.Distinct()
			.Count();

		Assert.True(distinct > 1);
	}

	private static void AssertComplete(IEnumerable<int> unit) =>
		Assert.Equal(Enumerable.Range(1, 9), unit.OrderBy(x => x));
}
