﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using org.mariuszgromada.math.mxparser;

namespace Noterium.Code.Helpers
{
    public static class NoteMathHelper
    {
        public static Regex Math = new Regex(@"(\$\$) (?=\S) (.+?[$]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex ConstantVariables = new Regex(@"\$\$var[\s]*?(?<varName>[a-zA-Z]*?)[\s]*?=[\s]*?(?<varValue>[1-9\.\,]*?)\$\$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        public static string ReplaceMathTokens(string text)
        {
            var vars = InitMathConstants(text);
            RemoveMathConstants(ref text);
            text = ReplaceMathTokensPrivate(text, vars);
            return text;
        }

        private static void RemoveMathConstants(ref string text)
        {
            var lines = text.Split('\n');
            var result = new List<string>();
            foreach (var l in lines)
                if (!l.StartsWith("$$var"))
                    result.Add(l);

            text = string.Join("\n", result);
        }

        private static string ReplaceMathTokensPrivate(string text, Constant[] constants)
        {
            var match = Math.Match(text);
            while (match.Success)
            {
                text = text.Remove(match.Index, match.Length);
                var result = "**Invalid math expression**";

                try
                {
                    var e = new Expression(match.Value.Trim('$'), constants);
                    var expressionResult = e.calculate();

                    if (!double.IsNaN(expressionResult))
                        result = expressionResult.ToString("N", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }

                text = text.Insert(match.Index, result);

                match = Math.Match(text);
            }

            return text;
        }

        private static Constant[] InitMathConstants(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var mathParams = new List<Constant>();

            var matches = ConstantVariables.Matches(text);
            foreach (Match m in matches)
            {
                var name = m.Groups[1].Value;
                var value = m.Groups[2].Value;

                double doubleValue;
                if (double.TryParse(value, out doubleValue))
                    mathParams.Add(new Constant(name, doubleValue));
            }

            return mathParams.ToArray();
        }
    }
}