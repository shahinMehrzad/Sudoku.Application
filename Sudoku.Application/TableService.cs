using System;
using System.Data;
using System.Reflection;

namespace Sudoku.Application
{
	public class TableService
	{
		//Column
		List<int> c1 = [0, 9, 18, 27, 36, 45, 54, 63, 72];
		List<int> c2 = [1, 10, 19, 28, 37, 46, 55, 64, 73];
		List<int> c3 = [2, 11, 20, 29, 38, 47, 56, 65, 74];
		List<int> c4 = [3, 12, 21, 30, 39, 48, 57, 66, 75];
		List<int> c5 = [4, 13, 22, 31, 40, 49, 58, 67, 76];
		List<int> c6 = [5, 14, 23, 32, 41, 50, 59, 68, 77];
		List<int> c7 = [6, 15, 24, 33, 42, 51, 60, 69, 78];
		List<int> c8 = [7, 16, 25, 34, 43, 52, 61, 70, 79];
		List<int> c9 = [8, 17, 26, 35, 44, 53, 62, 71, 80];

		//Row
		List<int> r1 = [0, 1, 2, 3, 4, 5, 6, 7, 8];
		List<int> r2 = [9, 10, 11, 12, 13, 14, 15, 16, 17];
		List<int> r3 = [18, 19, 20, 21, 22, 23, 24, 25, 26];
		List<int> r4 = [27, 28, 29, 30, 31, 32, 33, 34, 35];
		List<int> r5 = [36, 37, 38, 39, 40, 41, 42, 43, 44];
		List<int> r6 = [45, 46, 47, 48, 49, 50, 51, 52, 53];
		List<int> r7 = [54, 55, 56, 57, 58, 59, 60, 61, 62];
		List<int> r8 = [63, 64, 65, 66, 67, 68, 69, 70, 71];
		List<int> r9 = [72, 73, 74, 75, 76, 77, 78, 79, 80];

		//Box
		List<int> b1 = [0, 1, 2, 9, 10, 11, 18, 19, 20];
		List<int> b2 = [3, 4, 5, 12, 13, 14, 21, 22, 23];
		List<int> b3 = [6, 7, 8, 15, 16, 17, 24, 25, 26];
		List<int> b4 = [27, 28, 29, 36, 37, 38, 45, 46, 47];
		List<int> b5 = [30, 31, 32, 39, 40, 41, 48, 49, 50];
		List<int> b6 = [33, 34, 35, 42, 43, 44, 51, 52, 53];
		List<int> b7 = [54, 55, 56, 63, 64, 65, 72, 73, 74];
		List<int> b8 = [57, 58, 59, 66, 67, 68, 75, 76, 77];
		List<int> b9 = [60, 61, 62, 69, 70, 71, 78, 79, 80];

		MainTable table = new MainTable();

		public MainTable StartGame(MatchLevelEnum level)
		{
			Random rnd = new Random();

			for (int i = 0; i < 81; i++)
			{
				table.Table.Add(Fill(table, i));
			}

			if (table.Table.Contains(0))
			{
				var firstZeroIndex = table.Table.IndexOf(0);
				int index = 0;

				if (r1.Contains(firstZeroIndex))
				{
					index = r1.First();
					ClearRowValues(index);
					ReCalculate(r1);
					ReCalculate(r2);
					ReCalculate(r3);
					ReCalculate(r4);
					ReCalculate(r5);
					ReCalculate(r6);
					ReCalculate(r7);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else if (r2.Contains(firstZeroIndex))
				{
					index = r2.First();
					ClearRowValues(index);
					ReCalculate(r2);
					ReCalculate(r3);
					ReCalculate(r4);
					ReCalculate(r5);
					ReCalculate(r6);
					ReCalculate(r7);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else if (r3.Contains(firstZeroIndex))
				{
					index = r3.First();
					ClearRowValues(index);
					ReCalculate(r3);
					ReCalculate(r4);
					ReCalculate(r5);
					ReCalculate(r6);
					ReCalculate(r7);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else if (r4.Contains(firstZeroIndex))
				{
					index = r4.First();
					ClearRowValues(index);
					ReCalculate(r4);
					ReCalculate(r5);
					ReCalculate(r6);
					ReCalculate(r7);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else if (r5.Contains(firstZeroIndex))
				{
					index = r5.First();
					ClearRowValues(index);
					ReCalculate(r5);
					ReCalculate(r6);
					ReCalculate(r7);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else if (r6.Contains(firstZeroIndex))
				{
					index = r6.First();
					ClearRowValues(index);
					ReCalculate(r6);
					ReCalculate(r7);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else if (r7.Contains(firstZeroIndex))
				{
					index = r7.First();
					ClearRowValues(index);
					ReCalculate(r7);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else if (r8.Contains(firstZeroIndex))
				{
					index = r8.First();
					ClearRowValues(index);
					ReCalculate(r8);
					ReCalculate(r9);
				}
				else
				{
					index = r9.First();
					ClearRowValues(index);
					ReCalculate(r9);
				}

			}

			return table;
		}

		private void ReCalculate(List<int> items)
		{
			Random rnd = new Random();
			List<int> finished = new List<int>();

			//Detect zero row and clear data			
			List<IndexProbabilities> indexProbabilities = new List<IndexProbabilities>();

			for (int i = 1; i < 10; i++)
			{
				foreach (var x in items.Except(finished))
				{

					List<int> probabilities = [1, 2, 3, 4, 5, 6, 7, 8, 9];
					var boxProbs = GetBoxProbability(x, probabilities);
					var rowProbs = GetRowProbability(x, boxProbs);
					var columnProbs = GetColumnProbability(x, rowProbs);
					if (!columnProbs.Any())
					{
						table.Table.Clear();
						for (int ii = 0; ii < 81; ii++)
						{
							table.Table.Add(Fill(table, ii));
						}
					}
						
					indexProbabilities.Add(new IndexProbabilities
					{
						Index = x,
						Probabilities = columnProbs
					});

				}

				var lessProbabilityIndex = indexProbabilities.OrderBy(x => x.Probabilities.Count).FirstOrDefault();
				var rand = rnd.Next(lessProbabilityIndex.Probabilities.Count);
				try
				{
					table.Table[lessProbabilityIndex.Index] = lessProbabilityIndex.Probabilities[rand];
				}
				catch (Exception ex)
				{
					table.Table[lessProbabilityIndex.Index] = 0;
				}

				indexProbabilities.Clear();
				finished.Add(lessProbabilityIndex.Index);
			}
		}

		

		private void ClearRowValues(int index)
		{
			for (int i = index; i < 81; i++)
			{
				table.Table[i] = 0;
			}
		}

		private List<int> GetBoxProbability(int index, List<int> Probabilities)
		{
			List<int> rawNumbers = Probabilities;
			//Get the numbers in smallTable
			if (b1.Contains(index))
				RemoveProbabilties(rawNumbers, b1);
			else if (b2.Contains(index))
				RemoveProbabilties(rawNumbers, b2);
			else if (b3.Contains(index))
				RemoveProbabilties(rawNumbers, b3);
			else if (b4.Contains(index))
				RemoveProbabilties(rawNumbers, b4);
			else if (b5.Contains(index))
				RemoveProbabilties(rawNumbers, b5);
			else if (b6.Contains(index))
				RemoveProbabilties(rawNumbers, b6);
			else if (b7.Contains(index))
				RemoveProbabilties(rawNumbers, b7);
			else if (b8.Contains(index))
				RemoveProbabilties(rawNumbers, b8);
			else
				RemoveProbabilties(rawNumbers, b9);

			return rawNumbers;
		}

		private List<int> GetRowProbability(int index, List<int> Probabilities)
		{
			List<int> rawNumbers = Probabilities;
			//Get the numbers in smallTable
			if (r1.Contains(index))
				RemoveProbabilties(rawNumbers, r1);
			else if (r2.Contains(index))
				RemoveProbabilties(rawNumbers, r2);
			else if (r3.Contains(index))
				RemoveProbabilties(rawNumbers, r3);
			else if (r4.Contains(index))
				RemoveProbabilties(rawNumbers, r4);
			else if (r5.Contains(index))
				RemoveProbabilties(rawNumbers, r5);
			else if (r6.Contains(index))
				RemoveProbabilties(rawNumbers, r6);
			else if (r7.Contains(index))
				RemoveProbabilties(rawNumbers, r7);
			else if (r8.Contains(index))
				RemoveProbabilties(rawNumbers, r8);
			else
				RemoveProbabilties(rawNumbers, r9);

			return rawNumbers;
		}

		private List<int> GetColumnProbability(int index, List<int> Probabilities)
		{
			List<int> rawNumbers = Probabilities;
			//Get the numbers in column
			if (c1.Contains(index))
				RemoveProbabilties(rawNumbers, c1);
			else if (c2.Contains(index))
				RemoveProbabilties(rawNumbers, c2);
			else if (c3.Contains(index))
				RemoveProbabilties(rawNumbers, c3);
			else if (c4.Contains(index))
				RemoveProbabilties(rawNumbers, c4);
			else if (c5.Contains(index))
				RemoveProbabilties(rawNumbers, c5);
			else if (c6.Contains(index))
				RemoveProbabilties(rawNumbers, c6);
			else if (c7.Contains(index))
				RemoveProbabilties(rawNumbers, c7);
			else if (c8.Contains(index))
				RemoveProbabilties(rawNumbers, c8);
			else
				RemoveProbabilties(rawNumbers, c9);

			return rawNumbers;
		}
				

		private int Fill(MainTable table, int index)
		{
			Random rnd = new Random();
			List<int> probability = [1, 2, 3, 4, 5, 6, 7, 8, 9];

			probability = GetBoxProbability(index, probability);
			probability = GetRowProbability(index, probability);
			probability = GetColumnProbability(index, probability);


			index = rnd.Next(probability.Count);
			return probability.Any() ? (probability[index]) : 0;
		}

		private List<int> RemoveProbabilties(List<int> probabilities, List<int> items)
		{
			foreach (var i in items)
			{
				if (table.Table.Count > i)
					probabilities.Remove(table.Table[i]);
			}
			return probabilities;
		}
	}
}
