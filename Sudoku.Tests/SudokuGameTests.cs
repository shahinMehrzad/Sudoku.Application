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

    // A board with only the given cells filled in; every other cell is empty and editable.
    private static SudokuGame GameWithFixed(params (int Index, int Digit)[] given)
    {
        var table = new MainTable();
        table.Table.AddRange(new int[81]);
        foreach (var (index, digit) in given)
            table.Table[index] = digit;
        return new SudokuGame(table);
    }

    // Cell 0 is row 0, column 0, box 0.
    [Theory]
    [InlineData(8)]   // same row only
    [InlineData(72)]  // same column only
    [InlineData(20)]  // same box only
    public void HasConflict_IsTrueForBothCellsWhenTheyShareAUnitAndADigit(int otherIndex)
    {
        var game = GameWithFixed((otherIndex, 5));

        game.PlaceDigit(0, 5);

        Assert.True(game.HasConflict(0));
        Assert.True(game.HasConflict(otherIndex)); // the locked cell is part of the conflict too
    }

    [Fact]
    public void HasConflict_IsTrueBetweenTwoPlayerCells()
    {
        var game = GameWithFixed();

        game.PlaceDigit(0, 5);
        game.PlaceDigit(1, 5);

        Assert.True(game.HasConflict(0));
        Assert.True(game.HasConflict(1));
    }

    [Fact]
    public void HasConflict_IsFalseForTheSameDigitInUnrelatedCells()
    {
        var game = GameWithFixed((40, 5)); // row 4, column 4, box 4: shares nothing with cell 0

        game.PlaceDigit(0, 5);

        Assert.False(game.HasConflict(0));
        Assert.False(game.HasConflict(40));
    }

    [Fact]
    public void HasConflict_IsFalseForDifferentDigitsInTheSameUnit()
    {
        var game = GameWithFixed((8, 5));

        game.PlaceDigit(0, 6);

        Assert.False(game.HasConflict(0));
    }

    [Fact]
    public void HasConflict_IsFalseForEmptyCells()
    {
        var game = GameWithFixed((8, 5));

        Assert.False(game.HasConflict(0));
        Assert.False(game.HasConflict(1)); // empty, and its row contains other empty cells
    }

    [Fact]
    public void HasConflict_IsFalseForALoneDigit()
    {
        var game = GameWithFixed();

        game.PlaceDigit(0, 5);

        Assert.False(game.HasConflict(0)); // a cell does not conflict with itself
    }

    [Fact]
    public void HasConflict_DisappearsWhenTheConflictingCellIsCleared()
    {
        var game = GameWithFixed((8, 5));
        game.PlaceDigit(0, 5);

        game.ClearCell(0);

        Assert.False(game.HasConflict(8));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(81)]
    public void HasConflict_RejectsIndexesOutsideTheBoard(int index)
    {
        var game = GameWithFixed();

        Assert.Throws<ArgumentOutOfRangeException>(() => game.HasConflict(index));
    }

    private static void FillCorrectly(SudokuGame game, MainTable table)
    {
        for (int i = 0; i < 81; i++)
            if (!game.IsFixed(i))
                game.PlaceDigit(i, table.Solution[i]);
    }

    [Fact]
    public void IsSolved_IsFalseForANewGame()
    {
        var (game, _) = NewGame();

        Assert.False(game.IsSolved());
    }

    [Theory]
    [InlineData(MatchLevelEnum.Easy)]
    [InlineData(MatchLevelEnum.Medium)]
    [InlineData(MatchLevelEnum.Hard)]
    public void IsSolved_IsTrueWhenEveryCellMatchesTheSolution(MatchLevelEnum level)
    {
        var table = new TableService().StartGame(level);
        var game = new SudokuGame(table);

        FillCorrectly(game, table);

        Assert.True(game.IsSolved());
    }

    [Fact]
    public void IsSolved_IsFalseWhenOneCellIsStillEmpty()
    {
        var (game, table) = NewGame();
        FillCorrectly(game, table);
        int index = FirstIndex(game, isFixed: false);

        game.ClearCell(index);

        Assert.False(game.IsSolved());
    }

    [Fact]
    public void IsSolved_IsFalseWhenOneCellHasAWrongDigit()
    {
        var (game, table) = NewGame();
        FillCorrectly(game, table);
        int index = FirstIndex(game, isFixed: false);

        game.PlaceDigit(index, table.Solution[index] % 9 + 1);

        Assert.False(game.IsSolved());
    }

    [Fact]
    public void IsSolved_BecomesTrueAfterAWrongDigitIsCorrected()
    {
        var (game, table) = NewGame();
        FillCorrectly(game, table);
        int index = FirstIndex(game, isFixed: false);
        game.PlaceDigit(index, table.Solution[index] % 9 + 1);

        game.PlaceDigit(index, table.Solution[index]);

        Assert.True(game.IsSolved());
    }
}
