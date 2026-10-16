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

    [Fact]
    public void HasNote_IsFalseForACellWithoutNotes()
    {
        var (game, _) = NewGame();
        int index = FirstIndex(game, isFixed: false);

        for (int digit = 1; digit <= 9; digit++)
            Assert.False(game.HasNote(index, digit));
    }

    [Fact]
    public void ToggleNote_AddsTheNoteAndTogglingAgainRemovesIt()
    {
        var (game, _) = NewGame();
        int index = FirstIndex(game, isFixed: false);

        game.ToggleNote(index, 4);
        Assert.True(game.HasNote(index, 4));

        game.ToggleNote(index, 4);
        Assert.False(game.HasNote(index, 4));
    }

    [Fact]
    public void ToggleNote_KeepsDifferentDigitsIndependent()
    {
        var (game, _) = NewGame();
        int index = FirstIndex(game, isFixed: false);

        game.ToggleNote(index, 2);
        game.ToggleNote(index, 7);
        game.ToggleNote(index, 2);

        Assert.False(game.HasNote(index, 2));
        Assert.True(game.HasNote(index, 7));
    }

    [Fact]
    public void ToggleNote_OnlyAffectsTheGivenCell()
    {
        var game = GameWithFixed();

        game.ToggleNote(0, 5);

        Assert.True(game.HasNote(0, 5));
        Assert.False(game.HasNote(1, 5));
    }

    [Fact]
    public void ToggleNote_ThrowsOnAFixedCell()
    {
        var (game, _) = NewGame();
        int index = FirstIndex(game, isFixed: true);

        Assert.Throws<InvalidOperationException>(() => game.ToggleNote(index, 1));
    }

    [Fact]
    public void ToggleNote_ThrowsOnACellThatHasADigit()
    {
        var (game, _) = NewGame();
        int index = FirstIndex(game, isFixed: false);
        game.PlaceDigit(index, 3);

        Assert.Throws<InvalidOperationException>(() => game.ToggleNote(index, 1));
    }

    [Fact]
    public void HasNote_IsFalseInsteadOfThrowingForFixedAndFilledCells()
    {
        var (game, _) = NewGame();
        int fixedIndex = FirstIndex(game, isFixed: true);
        int filledIndex = FirstIndex(game, isFixed: false);
        game.PlaceDigit(filledIndex, 3);

        Assert.False(game.HasNote(fixedIndex, 1));
        Assert.False(game.HasNote(filledIndex, 1));
    }

    [Fact]
    public void PlaceDigit_ClearsTheNotesOfThatCellOnly()
    {
        var game = GameWithFixed();
        game.ToggleNote(0, 1);
        game.ToggleNote(0, 2);
        game.ToggleNote(1, 1);

        game.PlaceDigit(0, 9);
        game.ClearCell(0);

        Assert.False(game.HasNote(0, 1));
        Assert.False(game.HasNote(0, 2));
        Assert.True(game.HasNote(1, 1));
    }

    [Fact]
    public void ClearNotes_RemovesAllNotesOfTheCell()
    {
        var game = GameWithFixed();
        game.ToggleNote(0, 1);
        game.ToggleNote(0, 9);
        game.ToggleNote(1, 1);

        game.ClearNotes(0);

        Assert.False(game.HasNote(0, 1));
        Assert.False(game.HasNote(0, 9));
        Assert.True(game.HasNote(1, 1));
    }

    [Fact]
    public void ClearNotes_DoesNothingOnACellWithoutNotes()
    {
        var game = GameWithFixed();

        game.ClearNotes(0);

        Assert.False(game.HasNote(0, 1));
    }

    [Fact]
    public void ClearNotes_ThrowsOnAFixedCell()
    {
        var (game, _) = NewGame();
        int index = FirstIndex(game, isFixed: true);

        Assert.Throws<InvalidOperationException>(() => game.ClearNotes(index));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(81, 1)]
    [InlineData(0, 0)]
    [InlineData(0, 10)]
    public void Notes_RejectInvalidIndexOrDigit(int index, int digit)
    {
        var game = GameWithFixed();

        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => game.HasNote(index, digit));
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => game.ToggleNote(index, digit));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(81)]
    public void ClearNotes_RejectsIndexesOutsideTheBoard(int index)
    {
        var game = GameWithFixed();

        Assert.Throws<ArgumentOutOfRangeException>(() => game.ClearNotes(index));
    }

    [Fact]
    public void NewGame_HasNothingToUndoOrRedo()
    {
        var game = GameWithFixed();

        Assert.False(game.CanUndo);
        Assert.False(game.CanRedo);
        Assert.Throws<InvalidOperationException>(() => game.Undo());
        Assert.Throws<InvalidOperationException>(() => game.Redo());
    }

    [Fact]
    public void Undo_RevertsPlaceDigitAndRedoPlacesItAgain()
    {
        var game = GameWithFixed();
        game.PlaceDigit(0, 5);

        game.Undo();
        Assert.Equal(0, game.Values[0]);
        Assert.False(game.CanUndo);
        Assert.True(game.CanRedo);

        game.Redo();
        Assert.Equal(5, game.Values[0]);
        Assert.True(game.CanUndo);
        Assert.False(game.CanRedo);
    }

    [Fact]
    public void Undo_RevertsDigitAndNotesOfACellInOneStep()
    {
        var game = GameWithFixed();
        game.ToggleNote(0, 1);
        game.ToggleNote(0, 2);
        game.PlaceDigit(0, 9); // also wipes the notes

        game.Undo();

        Assert.Equal(0, game.Values[0]);
        Assert.True(game.HasNote(0, 1));
        Assert.True(game.HasNote(0, 2));

        game.Redo();

        Assert.Equal(9, game.Values[0]);
        Assert.False(game.HasNote(0, 1));
        Assert.False(game.HasNote(0, 2));
    }

    [Fact]
    public void Undo_RevertsReplacingADigit()
    {
        var game = GameWithFixed();
        game.PlaceDigit(0, 3);
        game.PlaceDigit(0, 4);

        game.Undo();

        Assert.Equal(3, game.Values[0]);
    }

    [Fact]
    public void Undo_RevertsClearCell()
    {
        var game = GameWithFixed();
        game.PlaceDigit(0, 7);
        game.ClearCell(0);

        game.Undo();

        Assert.Equal(7, game.Values[0]);
    }

    [Fact]
    public void Undo_RevertsToggleNote()
    {
        var game = GameWithFixed();
        game.ToggleNote(0, 6);

        game.Undo();
        Assert.False(game.HasNote(0, 6));

        game.Redo();
        Assert.True(game.HasNote(0, 6));
    }

    [Fact]
    public void Undo_RevertsClearNotes()
    {
        var game = GameWithFixed();
        game.ToggleNote(0, 1);
        game.ToggleNote(0, 8);
        game.ClearNotes(0);

        game.Undo();

        Assert.True(game.HasNote(0, 1));
        Assert.True(game.HasNote(0, 8));
    }

    [Fact]
    public void Undo_GoesBackOneChangeAtATimeInReverseOrder()
    {
        var game = GameWithFixed();
        game.PlaceDigit(0, 1);
        game.PlaceDigit(1, 2);
        game.PlaceDigit(2, 3);

        game.Undo();
        Assert.Equal(new[] { 1, 2, 0 }, game.Values.Take(3));

        game.Undo();
        Assert.Equal(new[] { 1, 0, 0 }, game.Values.Take(3));

        game.Undo();
        Assert.Equal(new[] { 0, 0, 0 }, game.Values.Take(3));
    }

    [Fact]
    public void NewChangeAfterUndo_DiscardsTheRedoHistory()
    {
        var game = GameWithFixed();
        game.PlaceDigit(0, 1);
        game.Undo();
        Assert.True(game.CanRedo);

        game.PlaceDigit(1, 2);

        Assert.False(game.CanRedo);
    }

    [Fact]
    public void History_KeepsOnlyTheLastFiveChanges()
    {
        var game = GameWithFixed();
        for (int i = 0; i < 7; i++)
            game.PlaceDigit(i, 1);

        for (int i = 0; i < SudokuGame.MaxHistory; i++)
            game.Undo();

        Assert.False(game.CanUndo);
        Assert.Equal(new[] { 1, 1, 0, 0, 0, 0, 0 }, game.Values.Take(7)); // the two oldest stay
    }

    [Fact]
    public void ChangesThatAlterNothing_AreNotRecorded()
    {
        var game = GameWithFixed();

        game.ClearCell(0);          // already empty
        game.ClearNotes(0);         // no notes
        Assert.False(game.CanUndo);

        game.PlaceDigit(1, 4);
        game.Undo();
        game.PlaceDigit(1, 4);
        game.PlaceDigit(1, 4);      // same digit again
        game.Undo();                // undoes the single real change

        Assert.Equal(0, game.Values[1]);
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void FailedChanges_AreNotRecorded()
    {
        var (game, _) = NewGame();
        int fixedIndex = FirstIndex(game, isFixed: true);

        Assert.Throws<InvalidOperationException>(() => game.PlaceDigit(fixedIndex, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.PlaceDigit(0, 10));

        Assert.False(game.CanUndo);
    }

    [Fact]
    public void UndoAndRedo_AreNotRecordedAsChanges()
    {
        var game = GameWithFixed();
        game.PlaceDigit(0, 1);

        game.Undo();
        game.Redo();
        game.Undo();
        game.Redo();

        Assert.True(game.CanUndo);
        game.Undo();
        Assert.False(game.CanUndo);
    }

    private static int WrongDigitFor(MainTable table, int index) => table.Solution[index] % 9 + 1;

    [Fact]
    public void Validate_ReturnsNothingForANewGame()
    {
        var (game, _) = NewGame();

        Assert.Empty(game.Validate());
    }

    [Fact]
    public void Validate_DoesNotFlagCorrectDigits()
    {
        var (game, table) = NewGame();
        int index = FirstIndex(game, isFixed: false);
        game.PlaceDigit(index, table.Solution[index]);

        Assert.Empty(game.Validate());
    }

    [Fact]
    public void Validate_ReturnsTheCellsWithWrongDigitsInAscendingOrder()
    {
        var (game, table) = NewGame();
        var empty = Enumerable.Range(0, 81).Where(i => !game.IsFixed(i)).ToList();
        int first = empty[0], second = empty[5], correct = empty[3];

        game.PlaceDigit(second, WrongDigitFor(table, second));
        game.PlaceDigit(correct, table.Solution[correct]);
        game.PlaceDigit(first, WrongDigitFor(table, first));

        Assert.Equal(new[] { first, second }, game.Validate());
    }

    [Fact]
    public void Validate_NeverFlagsEmptyOrFixedCells()
    {
        var (game, table) = NewGame();
        game.PlaceDigit(FirstIndex(game, isFixed: false), WrongDigitFor(table, FirstIndex(game, isFixed: false)));

        var mistakes = game.Validate();

        Assert.All(mistakes, i => Assert.False(game.IsFixed(i)));
        Assert.All(mistakes, i => Assert.NotEqual(0, game.Values[i]));
    }

    [Fact]
    public void Validate_NoLongerFlagsACellOnceItIsCorrected()
    {
        var (game, table) = NewGame();
        int index = FirstIndex(game, isFixed: false);
        game.PlaceDigit(index, WrongDigitFor(table, index));
        Assert.Single(game.Validate());

        game.PlaceDigit(index, table.Solution[index]);

        Assert.Empty(game.Validate());
    }

    [Fact]
    public void Validate_FlagsAWrongDigitEvenWhenNothingConflicts()
    {
        var (game, table) = NewGame();
        // any empty cell and digit that breaks no row/column/box rule yet is not the solution
        var (index, digit) = Enumerable.Range(0, 81)
            .Where(i => !game.IsFixed(i))
            .SelectMany(i => Enumerable.Range(1, 9).Select(d => (Index: i, Digit: d)))
            .First(c => c.Digit != table.Solution[c.Index] && !Conflicts(game, c.Index, c.Digit));
        game.PlaceDigit(index, digit);

        Assert.False(game.HasConflict(index));
        Assert.Equal(new[] { index }, game.Validate());
    }

    private static bool Conflicts(SudokuGame game, int index, int digit)
    {
        game.PlaceDigit(index, digit);
        bool conflict = game.HasConflict(index);
        game.ClearCell(index);
        return conflict;
    }

    [Fact]
    public void ChecksUsed_StartsAtZeroAndCountsEveryValidate()
    {
        var (game, _) = NewGame();
        Assert.Equal(0, game.ChecksUsed);

        game.Validate(); // nothing wrong, still counts
        game.Validate();

        Assert.Equal(2, game.ChecksUsed);
    }

    [Fact]
    public void ChecksUsed_IsNotReducedByUndo()
    {
        var (game, _) = NewGame();
        game.PlaceDigit(FirstIndex(game, isFixed: false), 1);
        game.Validate();

        game.Undo();

        Assert.Equal(1, game.ChecksUsed);
    }

    [Fact]
    public void Validate_DoesNotChangeTheBoardOrTheHistory()
    {
        var (game, table) = NewGame();
        int index = FirstIndex(game, isFixed: false);
        game.PlaceDigit(index, WrongDigitFor(table, index));
        var before = game.Values.ToList();

        game.Validate();

        Assert.Equal(before, game.Values);
        game.Undo();
        Assert.False(game.CanUndo); // Validate added nothing to the history
    }
}
