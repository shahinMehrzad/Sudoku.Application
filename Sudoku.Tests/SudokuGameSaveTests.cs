using System.Text.Json.Nodes;
using Sudoku.Application;

namespace Sudoku.Tests;

public class SudokuGameSaveTests
{
    private static (SudokuGame Game, MainTable Table) NewGame()
    {
        var table = new TableService().StartGame(MatchLevelEnum.Medium);
        return (new SudokuGame(table), table);
    }

    private static int FirstIndex(SudokuGame game, bool isFixed) =>
        Enumerable.Range(0, 81).First(i => game.IsFixed(i) == isFixed);

    // A game with some progress: a player digit, notes on another cell, and an undo history.
    private static (SudokuGame Game, int DigitCell, int NotesCell) PlayedGame()
    {
        var (game, table) = NewGame();
        var empty = Enumerable.Range(0, 81).Where(i => !game.IsFixed(i)).ToList();
        int digitCell = empty[0];
        int notesCell = empty[1];

        game.PlaceDigit(digitCell, table.Solution[digitCell]);
        game.ToggleNote(notesCell, 2);
        game.ToggleNote(notesCell, 7);
        return (game, digitCell, notesCell);
    }

    private static void AssertSameState(SudokuGame expected, SudokuGame actual)
    {
        Assert.Equal(expected.Values, actual.Values);
        for (int i = 0; i < 81; i++)
        {
            Assert.Equal(expected.IsFixed(i), actual.IsFixed(i));
            for (int digit = 1; digit <= 9; digit++)
                Assert.Equal(expected.HasNote(i, digit), actual.HasNote(i, digit));
        }
        Assert.Equal(expected.IsSolved(), actual.IsSolved());
    }

    [Fact]
    public void Snapshot_RestoresValuesFixedCellsAndNotes()
    {
        var (game, digitCell, notesCell) = PlayedGame();

        var restored = SudokuGame.FromSnapshot(game.ToSnapshot());

        AssertSameState(game, restored);
        Assert.True(restored.HasNote(notesCell, 2));
        Assert.True(restored.HasNote(notesCell, 7));
        Assert.NotEqual(0, restored.Values[digitCell]);
    }

    [Fact]
    public void Json_RestoresTheSameGame()
    {
        var (game, _, _) = PlayedGame();

        var restored = SudokuGame.FromJson(game.ToJson());

        AssertSameState(game, restored);
    }

    [Fact]
    public void RestoredGame_KnowsTheSolution()
    {
        var (game, table) = NewGame();
        var restored = SudokuGame.FromJson(game.ToJson());

        for (int i = 0; i < 81; i++)
            if (!restored.IsFixed(i))
                restored.PlaceDigit(i, table.Solution[i]);

        Assert.True(restored.IsSolved());
    }

    [Fact]
    public void RestoredGame_StartsWithAnEmptyHistory()
    {
        var (game, _, _) = PlayedGame();
        Assert.True(game.CanUndo);

        var restored = SudokuGame.FromSnapshot(game.ToSnapshot());

        Assert.False(restored.CanUndo);
        Assert.False(restored.CanRedo);
    }

    [Fact]
    public void RestoredGame_IsIndependentOfTheOriginal()
    {
        var (game, _, notesCell) = PlayedGame();
        var restored = SudokuGame.FromSnapshot(game.ToSnapshot());

        game.ToggleNote(notesCell, 2);
        restored.ToggleNote(notesCell, 9);

        Assert.False(game.HasNote(notesCell, 9));
        Assert.True(restored.HasNote(notesCell, 2));
    }

    [Fact]
    public void Snapshot_LeavesOutCellsWithoutNotes()
    {
        var (game, _) = NewGame();
        int index = FirstIndex(game, isFixed: false);
        game.ToggleNote(index, 5);
        game.ToggleNote(index, 5); // back to no notes

        Assert.Empty(game.ToSnapshot().Notes);
    }

    [Fact]
    public void Snapshot_IsACopyOfTheGameState()
    {
        var (game, _) = NewGame();
        var snapshot = game.ToSnapshot();

        game.PlaceDigit(FirstIndex(game, isFixed: false), 9);

        Assert.NotEqual(snapshot.Values, game.Values);
    }

    // Takes a valid snapshot, lets the test break it, and expects the restore to be refused.
    private static void AssertRejected(Action<GameSnapshot> breakIt)
    {
        var (game, _, _) = PlayedGame();
        var snapshot = game.ToSnapshot();
        breakIt(snapshot);

        Assert.Throws<InvalidDataException>(() => SudokuGame.FromSnapshot(snapshot));
    }

    [Fact]
    public void FromSnapshot_RejectsAnUnknownVersion() =>
        AssertRejected(s => s.Version = 99);

    [Fact]
    public void FromSnapshot_RejectsWrongArrayLengths()
    {
        AssertRejected(s => s.Values = s.Values.Take(80).ToArray());
        AssertRejected(s => s.FixedCells = s.FixedCells.Take(80).ToArray());
        AssertRejected(s => s.Solution = s.Solution.Take(80).ToArray());
    }

    [Fact]
    public void FromSnapshot_RejectsDigitsOutsideZeroToNine()
    {
        AssertRejected(s => s.Values[0] = 10);
        AssertRejected(s => s.Values[0] = -1);
    }

    [Fact]
    public void FromSnapshot_RejectsASolutionThatIsNotAValidGrid() =>
        AssertRejected(s => s.Solution[0] = s.Solution[1]);

    [Fact]
    public void FromSnapshot_RejectsAFixedCellThatDiffersFromTheSolution()
    {
        AssertRejected(s =>
        {
            int fixedIndex = Array.IndexOf(s.FixedCells, true);
            s.Values[fixedIndex] = s.Solution[fixedIndex] % 9 + 1;
        });
    }

    [Fact]
    public void FromSnapshot_RejectsNotesOnFixedFilledOrMissingCells()
    {
        AssertRejected(s => s.Notes[Array.IndexOf(s.FixedCells, true)] = [1]);
        AssertRejected(s =>
        {
            // a cell the player has filled in (not fixed, but not empty)
            int filledIndex = Enumerable.Range(0, 81).First(i => !s.FixedCells[i] && s.Values[i] != 0);
            s.Notes[filledIndex] = [1];
        });
        AssertRejected(s => s.Notes[81] = [1]);
        AssertRejected(s => s.Notes[-1] = [1]);
    }

    [Fact]
    public void FromSnapshot_RejectsInvalidNoteDigits()
    {
        AssertRejected(s => s.Notes[s.Notes.Keys.First()] = [0]);
        AssertRejected(s => s.Notes[s.Notes.Keys.First()] = [10]);
        AssertRejected(s => s.Notes[s.Notes.Keys.First()] = [3, 3]);
    }

    [Fact]
    public void FromSnapshot_RejectsNull() =>
        Assert.Throws<ArgumentNullException>(() => SudokuGame.FromSnapshot(null!));

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"Values\": ")]
    [InlineData("null")]
    [InlineData("{}")]
    public void FromJson_RejectsTextThatIsNotAValidSavedGame(string json) =>
        Assert.Throws<InvalidDataException>(() => SudokuGame.FromJson(json));

    [Fact]
    public void Snapshot_KeepsTheNumberOfChecks()
    {
        var (game, _, _) = PlayedGame();
        game.Validate();
        game.Validate();

        var restored = SudokuGame.FromJson(game.ToJson());

        Assert.Equal(2, restored.ChecksUsed);
    }

    [Fact]
    public void FromJson_LoadsASaveMadeWithoutTheCheckCounter()
    {
        var (game, _, _) = PlayedGame();
        var json = JsonNode.Parse(game.ToJson())!.AsObject();
        json.Remove("ChecksUsed");

        var restored = SudokuGame.FromJson(json.ToJsonString());

        Assert.Equal(0, restored.ChecksUsed);
        AssertSameState(game, restored);
    }

    [Fact]
    public void FromSnapshot_RejectsANegativeNumberOfChecks() =>
        AssertRejected(s => s.ChecksUsed = -1);

    [Fact]
    public void Snapshot_KeepsTheNumberOfHints()
    {
        var (game, _, _) = PlayedGame();
        game.GetHint();
        game.GetHint();
        game.GetHint();

        var restored = SudokuGame.FromJson(game.ToJson());

        Assert.Equal(3, restored.HintsUsed);
    }

    [Fact]
    public void FromJson_LoadsASaveMadeWithoutTheHintCounter()
    {
        var (game, _, _) = PlayedGame();
        var json = JsonNode.Parse(game.ToJson())!.AsObject();
        json.Remove("HintsUsed");

        var restored = SudokuGame.FromJson(json.ToJsonString());

        Assert.Equal(0, restored.HintsUsed);
        AssertSameState(game, restored);
    }

    [Fact]
    public void FromSnapshot_RejectsANegativeNumberOfHints() =>
        AssertRejected(s => s.HintsUsed = -1);
}
