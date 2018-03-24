using Ganss.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FindAwkwardExpressions
{
    public class Program
    {
		static void Main(string[] args)
		{
			// Load rules.
			// * is considered as a special characters which can replace characters or any word.
			List<string> rules = LoadRules();
			var runner = new Runner(rules);

			// Analyze some reddit comments.
			foreach (var path in Directory.EnumerateFiles("Samples", "*.txt"))
			{
				var text = File.ReadAllText(path);
				var results = runner.FindAwkwardExpressions(text);
				if (results.Any())
				{
					Console.WriteLine("\n\nSubreddit: " + Path.GetFileNameWithoutExtension(path).Split('-')[0].Trim());
					Console.WriteLine("\ncomment:");
					Console.WriteLine(text);
					Console.WriteLine("\nRules:");
					foreach (var rule in results)
					{
						Console.WriteLine(" - " + rule.Expr);
					}
					Console.ReadLine();
				}
			}

			Console.WriteLine("\nPress any key to continue...");
			Console.ReadLine();
		}

		private static List<string> LoadRules()
		{
			List<string> rules = new List<string>();
			foreach (var path in Directory.EnumerateFiles("Rules", "*.txt"))
			{
				rules.AddRange(File.ReadAllLines(path)
					.Where(m => !string.IsNullOrWhiteSpace(m))  // Remove empty lines.
					.Where(m => !m.StartsWith("#"))             // Remove comments.
					.ToList());
			}
			return rules;
		}
	}
}
