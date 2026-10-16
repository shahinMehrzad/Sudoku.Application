# Sudoku.Application

A Sudoku game built with .NET 10. The game logic lives in a UI-independent class library, so it can be reused by a console app today and by a .NET MAUI app on desktop and mobile.

> **Status:** early development. The puzzle generator works; the MAUI user interface is still the default project template.

## Solution layout

| Project | Description |
|---|---|
| `Sudoku.Application` | Core library: puzzle generation and difficulty levels. |
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

## Roadmap

- [ ] Build the game board in `Sudoku.MAUI`
