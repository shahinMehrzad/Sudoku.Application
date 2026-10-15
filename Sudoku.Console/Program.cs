using Sudoku.Application;

var service = new TableService();
var match = service.StartGame(MatchLevelEnum.Easy);

for (int row = 0; row < 9; row++)
{
	for (int col = 0; col < 9; col++)
	{
		Console.Write(match.Table[row * 9 + col] == 0 ? "_" : match.Table[row * 9 + col].ToString());
		Console.Write(" | ");
	}
	Console.WriteLine();
	Console.WriteLine("_________________________________");
}

Console.ReadLine();
