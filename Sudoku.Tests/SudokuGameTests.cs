using Sudoku.Application;

namespace Sudoku.Tests;

public class SudokuGameTests
{
	private static (SudokuGame Game, MainTable Table) NewGame()
	{
		var table = new TableService().StartGame(MatchLevelEnum.Medium);
		return (new SudokuGame(table), table);
	}

	private static int FirstIndex(SudokuGame game, bool isFixed) =>
		Enumerable.Range(0, 81).First(i => game.IsFixed(i) == isFixed);

	[Fact]
	public void NewGame_StartsWithThePuzzleValues()
	{
		var (game, table) = NewGame();

		Assert.Equal(table.Table, game.Values);
	}

	[Fact]
	public void IsFixed_IsTrueOnlyForCellsGivenByThePuzzle()
	{
		var (game, table) = NewGame();

		for (int i = 0; i < 81; i++)
			Assert.Equal(table.Table[i] != 0, game.IsFixed(i));
	}

	[Fact]
	public void PlaceDigit_SetsTheValueOfAnEmptyCell()
	{
		var (game, _) = NewGame();
		int index = FirstIndex(game, isFixed: false);

		game.PlaceDigit(index, 7);

		Assert.Equal(7, game.Values[index]);
	}

	[Fact]
	public void PlaceDigit_CanReplaceAPlayerDigit()
	{
		var (game, _) = NewGame();
		int index = FirstIndex(game, isFixed: false);

		game.PlaceDigit(index, 3);
		game.PlaceDigit(index, 4);

		Assert.Equal(4, game.Values[index]);
	}

	[Fact]
	public void ClearCell_EmptiesAPlayerCell()
	{
		var (game, _) = NewGame();
		int index = FirstIndex(game, isFixed: false);

		game.PlaceDigit(index, 5);
		game.ClearCell(index);

		Assert.Equal(0, game.Values[index]);
	}

	[Fact]
	public void FixedCells_CannotBeChanged()
	{
		var (game, table) = NewGame();
		int index = FirstIndex(game, isFixed: true);

		Assert.Throws<InvalidOperationException>(() => game.PlaceDigit(index, 1));
		Assert.Throws<InvalidOperationException>(() => game.ClearCell(index));
		Assert.Equal(table.Table[index], game.Values[index]);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(10)]
	[InlineData(-1)]
	public void PlaceDigit_RejectsDigitsOutsideOneToNine(int digit)
	{
		var (game, _) = NewGame();
		int index = FirstIndex(game, isFixed: false);

		Assert.Throws<ArgumentOutOfRangeException>(() => game.PlaceDigit(index, digit));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(81)]
	public void Cells_RejectIndexesOutsideTheBoard(int index)
	{
		var (game, _) = NewGame();

		Assert.Throws<ArgumentOutOfRangeException>(() => game.IsFixed(index));
		Assert.Throws<ArgumentOutOfRangeException>(() => game.PlaceDigit(index, 1));
		Assert.Throws<ArgumentOutOfRangeException>(() => game.ClearCell(index));
	}

	[Fact]
	public void Game_DoesNotModifyTheOriginalTable()
	{
		var (game, table) = NewGame();
		var before = table.Table.ToList();

		game.PlaceDigit(FirstIndex(game, isFixed: false), 9);

		Assert.Equal(before, table.Table);
	}
}
