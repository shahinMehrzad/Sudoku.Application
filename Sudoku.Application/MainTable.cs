using System.Net.Http.Headers;

namespace Sudoku.Application
{
	public class MainTable
	{
        //public Row R1 { get; set; } = new Row();
        //public Row R2 { get; set; } = new Row();
        //public Row R3 { get; set; } = new Row();
        //public Row R4 { get; set; } = new Row();
        //public Row R5 { get; set; } = new Row();
        //public Row R6 { get; set; } = new Row();
        //public Row R7 { get; set; } = new Row();
        //public Row R8 { get; set; } = new Row();
        //public Row R9 { get; set; } = new Row();
        public List<int> Table { get; set; } = new List<int>();
        //public List<int> Row1 { get; set; } = new List<int>();
        //public List<int> Row2 { get; set; } = new List<int>();
        //public List<int> Row3 { get; set; } = new List<int>();
        //public List<int> Row4 { get; set; } = new List<int>();
        //public List<int> Row5 { get; set; } = new List<int>();
        //public List<int> Row6 { get; set; } = new List<int>();
        //public List<int> Row7 { get; set; } = new List<int>();
        //public List<int> Row8 { get; set; } = new List<int>();
        //public List<int> Row9 { get; set; } = new List<int>();
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
}
