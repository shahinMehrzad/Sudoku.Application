namespace Sudoku.Application;

public class MainTable
{
    /// <summary>The puzzle, row by row: 0 marks an empty cell.</summary>
    public List<int> Table { get; set; } = new List<int>();

    /// <summary>The complete solution of the puzzle, row by row.</summary>
    public List<int> Solution { get; set; } = new List<int>();
}
