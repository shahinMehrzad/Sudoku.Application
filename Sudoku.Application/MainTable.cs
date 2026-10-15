namespace Sudoku.Application;

public class MainTable
{
    public List<int> Table { get; set; } = new List<int>();
    public List<int> Solution { get; set; } = new List<int>();
}

public class Row
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
    public int E { get; set; }
    public int F { get; set; }
    public int G { get; set; }
    public int H { get; set; }
    public int I { get; set; }
}

public class IndexProbabilities
{
    public int Index { get; set; }
    public List<int> Probabilities { get; set; } = new List<int>();
}

