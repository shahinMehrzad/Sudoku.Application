# Sudoku.Application

A Sudoku game built with .NET 10. The game logic lives in a UI-independent class library, so it can be reused by a console app today and by a .NET MAUI app on desktop and mobile.

> **Status:** early development. The game logic (generation, play, notes, undo/redo, save/load, check, hints) is complete and tested; the MAUI user interface is still the default project template.

## Solution layout

| Project | Description |
|---|---|
| `Sudoku.Application` | Core library: puzzle generation, difficulty levels, and the state of a game in progress. |
| `Sudoku.Console` | Minimal console app that generates and prints a puzzle. |
| `Sudoku.MAUI` | .NET MAUI app (Android, iOS, Mac Catalyst, Windows). UI not implemented yet. |
| `Sudoku.Tests` | xUnit tests for the core library. |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- For `Sudoku.MAUI` only: the MAUI workload

  ```
  dotnet workload install maui
  ```

## Getting started

```
# Run the tests
dotnet test Sudoku.Tests

# Generate and print a puzzle in the console
dotnet run --project Sudoku.Console
```

## How it works

`TableService.StartGame(MatchLevelEnum level)` returns a `MainTable`:

- `Solution`: a complete, valid 9×9 grid (81 values, 1–9), generated with randomized backtracking.
- `Table`: the puzzle, a copy of the solution with some cells set to `0` (empty).

Cells are stored row by row: the cell at row `r`, column `c` (both 0-based) is at index `r * 9 + c`.

### Difficulty levels

| Level | Empty cells |
|---|---|
| Easy | 36 |
| Medium | 45 |
| Hard | 52 |

Cells are removed one at a time in random order. A removal is kept only if the puzzle still has exactly one solution, so every generated puzzle is uniquely solvable. If a grid cannot be emptied enough, a new one is generated.

```csharp
var service = new TableService();
MainTable game = service.StartGame(MatchLevelEnum.Medium);

int value = game.Table[4 * 9 + 2];     // row 4, column 2 (0 = empty)
int answer = game.Solution[4 * 9 + 2]; // the correct value for that cell
```

### Reproducible puzzles

`new TableService(seed)` generates the same puzzles for the same seed (useful for tests or a "puzzle of the day"). The parameterless constructor picks a random seed. Reproducibility is only guaranteed within the same .NET version.

```csharp
var a = new TableService(42).StartGame(MatchLevelEnum.Hard);
var b = new TableService(42).StartGame(MatchLevelEnum.Hard);
// a.Table equals b.Table
```

## Playing a game

`SudokuGame` holds a game in progress. Create one from a generated puzzle:

```csharp
var puzzle = new TableService().StartGame(MatchLevelEnum.Medium);
var game = new SudokuGame(puzzle);
```

Cells given by the puzzle are **fixed** and cannot be changed. All indexes are `0..80` (see the layout above), digits are `1..9`. Invalid arguments throw `ArgumentOutOfRangeException`; changing a fixed cell throws `InvalidOperationException`.

| Feature | Members |
|---|---|
| Board | `Values` (0 = empty), `IsFixed(index)`, `PlaceDigit(index, digit)`, `ClearCell(index)` |
| Rule conflicts | `HasConflict(index)`: the digit also appears in the cell's row, column or box (both cells report it) |
| Finish | `IsSolved()` |
| Notes (pencil marks) | `ToggleNote(index, digit)`, `HasNote(index, digit)`, `ClearNotes(index)`. Placing a digit clears that cell's notes |
| Undo / redo | `Undo()`, `Redo()`, `CanUndo`, `CanRedo`. Keeps the last `SudokuGame.MaxHistory` (5) changes. Each change affects one cell, notes included |
| Check | `Validate()` returns the indexes of wrong digits and counts one use in `ChecksUsed` |
| Hint | `GetHint()` returns a `Hint(Index, Digit)` for the easiest empty cell and counts one use in `HintsUsed` |

`HasConflict` and `Validate` are different: `HasConflict` only sees repeated digits, `Validate` compares with the solution and also finds wrong digits that break no rule yet. `ChecksUsed` and `HintsUsed` are never reduced by undo.

```csharp
game.PlaceDigit(0, 5);
if (game.HasConflict(0)) { /* highlight in red */ }

var wrong = game.Validate();   // indexes of wrong digits
var hint = game.GetHint();     // null when the game is solved
```

### Saving and loading

A game can be saved at any time. The undo/redo history is not saved: a restored game starts with an empty history.

```csharp
string json = game.ToJson();            // store it wherever the app wants
var restored = SudokuGame.FromJson(json);
```

`ToSnapshot()` / `SudokuGame.FromSnapshot(snapshot)` do the same with a plain `GameSnapshot` object. Invalid or tampered data throws `InvalidDataException`. Where the text is stored (file, preferences) is up to the app.

## Roadmap

- [ ] Rate difficulty by the solving techniques a puzzle needs (instead of only the number of empty cells)
- [ ] Build the game board in `Sudoku.MAUI`
