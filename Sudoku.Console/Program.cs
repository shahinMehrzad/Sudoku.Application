// See https://aka.ms/new-console-template for more information
using Sudoku.Application;



var service = new TableService();
var match = service.StartGame(MatchLevelEnum.Easy);

for(int i = 0; i < 9;  i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 9; i < 18; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 18; i < 27; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 27; i < 36; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 36; i < 45; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 45; i < 54; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 54; i < 63; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 63; i < 72; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

for (int i = 72; i < 81; i++)
{
	Console.Write(match.Table[i]);
	Console.Write(" | ");
}
Console.WriteLine();
Console.WriteLine("_________________________________");

Console.ReadLine();

