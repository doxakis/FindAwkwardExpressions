using Ganss.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FindAwkwardExpressions
{
	public class Expression
	{
		public int Id { get; set; }
		public Regex Regexp { get; set; }
		public bool IsExactExpression { get; set; }
		public string Expr { get; set; }
		public string Pattern { get; set; }
	}

	public class Runner
	{
		Dictionary<string, List<Expression>> GroupByLonguestCommonExpression;
		AhoCorasick TreeAhoCorasick;

		public Runner(List<string> rules)
		{
			GroupByLonguestCommonExpression = new Dictionary<string, List<Expression>>();
			TreeAhoCorasick = TextUtils.ComputeRules(
				rules,
				GroupByLonguestCommonExpression);
		}

		public List<Expression> FindAwkwardExpressions(string text)
		{
			return TextUtils.FindAwkwardExpressions(
					GroupByLonguestCommonExpression,
					TreeAhoCorasick,
					text);
		}
	}

	public class TextUtils
    {
		// Used to rewrite the sentence. (only keep words)
		private static Regex regexKeepWordOnly = new Regex("[\\p{L}\\d]+",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		// Used to prepare the Regex. (replace * for words or characters)
		private static Regex regexKeepWordOnlyAndSpecialCharacters =
			new Regex("[\\p{L}\\d\\*]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		public static List<Expression> FindAwkwardExpressions(
			Dictionary<string, List<Expression>> groupByLonguestCommonExpression,
			AhoCorasick treeAhoCorasick,
			string text)
		{
			List<Expression> expressions = new List<Expression>();

			text = RemoveAccents(text);
			text = text.ToLower();

			var matches = regexKeepWordOnly.Matches(text);
			var words = matches.Select(m => m.Value);
			text = " " + string.Join(' ', words) + " ";

			var results = treeAhoCorasick.Search(text).ToList();
			if (results.Any())
			{
				foreach (var item in results)
				{
					var curRules = groupByLonguestCommonExpression[item.Word];
					var exactExpression = curRules.FirstOrDefault(m => m.IsExactExpression);
					if (exactExpression != null)
					{
						expressions.Add(exactExpression);
					}
					else
					{
						foreach (var rule in curRules.Where(m => !m.IsExactExpression))
						{
							if (rule.Regexp == null)
							{
								// Lazy load Regex.
								rule.Regexp = new Regex(rule.Pattern,
									RegexOptions.Compiled | RegexOptions.IgnoreCase);
							}
							var hasMatch = rule.Regexp.IsMatch(text);
							if (hasMatch)
							{
								expressions.Add(rule);
							}
						}
					}
				}
			}

			return expressions;
		}

		public static AhoCorasick ComputeRules(
			List<string> rules,
			Dictionary<string, List<Expression>> groupByLonguestCommonExpression)
		{
			int nextId = 0;
			for (int i = 0; i < rules.Count; i++)
			{
				var pattern = rules[i];
				pattern = RemoveAccents(pattern);
				pattern = pattern.ToLower();

				// We will take the longuest fixed expression.
				var longuestCommonExpression = pattern
					.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries)
					.OrderByDescending(m => m.Length)
					.First();

				var matches = regexKeepWordOnly.Matches(longuestCommonExpression);
				var words = matches.Select(m => m.Value);
				longuestCommonExpression = string.Join(' ', words);

				// Exact expression is faster.
				var exactExpression = !pattern.Contains("*");

				if (exactExpression)
				{
					// Exact word or group of words. (add extra space to prevent matching)
					longuestCommonExpression = " " + longuestCommonExpression + " ";
				}
				else
				{
					// We are using only a part of the expression.
					// We will not append extra space.
				}

				// Prepare the future Regex.
				var matches2 = regexKeepWordOnlyAndSpecialCharacters.Matches(pattern);
				var words2 = matches2.Select(m => m.Value.Replace("*", "[\\p{L}]+"));
				pattern = string.Join(' ', words2);

				if (!groupByLonguestCommonExpression.ContainsKey(longuestCommonExpression))
				{
					groupByLonguestCommonExpression[longuestCommonExpression] =
						new List<Expression>();
				}

				var expr = new Expression
				{
					Id = nextId++,
					Expr = rules[i],
					IsExactExpression = exactExpression
				};

				if (!exactExpression)
				{
					// The pattern will be use if it is not an exact expression
					// to resolve the wildcard.
					expr.Pattern = pattern;
				}

				groupByLonguestCommonExpression[longuestCommonExpression].Add(expr);
			}
			var keywords = groupByLonguestCommonExpression.Select(m => m.Key).ToList();

			// We are using the Aho–Corasick algorithm.
			// (https://en.wikipedia.org/wiki/Aho%E2%80%93Corasick_algorithm)
			
			// Unfortunately, it doesn't support wildcard.
			// But, we will use the longuest fixed expression.
			// So, we reduce complexities and the number of regex check.

			// The algorithm will tell us which Regex to evaluate.
			// But, if it is an exact expression, no need to do extra processing.

			AhoCorasick treeAhoCorasick = new AhoCorasick();
			treeAhoCorasick.Add(keywords);
			treeAhoCorasick.BuildFail();
			return treeAhoCorasick;
		}

		public static string RemoveAccents(string input)
		{
			string normalized = input.Normalize(NormalizationForm.FormKD);
			Encoding removal = Encoding.GetEncoding(
				Encoding.ASCII.CodePage,
				new EncoderReplacementFallback(""),
				new DecoderReplacementFallback(""));
			byte[] bytes = removal.GetBytes(normalized);
			return Encoding.ASCII.GetString(bytes);
		}
	}
}
