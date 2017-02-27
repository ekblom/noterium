using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Noterium.Code.Helpers
{
	public static class NoteMathHelper
	{
		public static Regex Math = new Regex(@"(\$\$) (?=\S) (.+?[$]*) (?<=\S) \1",
	RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

		public static Regex ConstantVariables = new Regex(@"^\$\$var[\s]*?(?<varName>[a-zA-Z]*?)[\s]*?=[\s]*?(?<varValue>[1-9]*?)\$\$$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		public static string ReplaceMathTokens(string text)
		{
			var vars = InitMathVariables(ref text);
			text = ReplaceMathTokensPrivate(text, vars);
			return text;
		}

		private static string ReplaceMathTokensPrivate(string text, Dictionary<string, double> vars)
		{
			var match = Math.Match(text);
			using (DataTable dt = new DataTable())
			{
				while (match.Success)
				{
					text = text.Remove(match.Index, match.Length);
					string result = "**Invalid math expression**";

					try
					{
						//TODO: Keep variables?

						//Expression e = new Expression(match.Value.Trim('$'), vars.ToArray());
						//double expressionResult = e.calculate();

						object o = dt.Compute(match.Value.Trim('$'), null);
						double expressionResult = double.NaN;
						if (o is int)
							expressionResult = ((int)o);
						else if (o is double)
							expressionResult = (double)o;
						else if (o is decimal)
							expressionResult = Convert.ToDouble((decimal)o);

						if (!double.IsNaN(expressionResult))
							result = expressionResult.ToString("N", CultureInfo.InvariantCulture);
					}
					catch (Exception)
					{

					}

					text = text.Insert(match.Index, result);

					match = Math.Match(text);
				}
			}

			return text;
		}

		private static Dictionary<string, double> InitMathVariables(ref string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			Dictionary<string, double> mathParams = new Dictionary<string, double>();

			var matches = ConstantVariables.Matches(text);
			foreach (Match m in matches)
			{
				string name = m.Groups[1].Value;
				string value = m.Groups[2].Value;

				double doubleValue;
				if (double.TryParse(value, out doubleValue) && !mathParams.ContainsKey(name))
					mathParams.Add(name, doubleValue);

				text = text.Replace(m.Value, string.Empty);
			}

			return mathParams;
		}
	}
}