using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Noterium.Core.Services
{
    internal class ClipboardHtmlHelper
    {
        public static ClipboardHtmlOutput ParseString(string s)
        {
            var html = new ClipboardHtmlOutput();

            var pattern = @"Version:(?<version>[0-9]+(?:\.[0-9]*)?).+StartHTML:(?<startH>\d*).+EndHTML:(?<endH>\d*).+StartFragment:(?<startF>\d+).+EndFragment:(?<endF>\d*).+SourceURL:(?<source>f|ht{1}tps?://[-a-zA-Z0-9@:%_\+.~#?&//=]+)";
            var match = Regex.Match(s, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                try
                {
                    html.Input = s;
                    html.Version = double.Parse(match.Groups["version"].Value, CultureInfo.InvariantCulture);
                    html.Source = match.Groups["source"].Value;
                    html.startHTML = int.Parse(match.Groups["startH"].Value);
                    html.endHTML = int.Parse(match.Groups["endH"].Value);
                    html.startFragment = int.Parse(match.Groups["startF"].Value);
                    html.endFragment = int.Parse(match.Groups["endF"].Value);
                }
                catch (Exception)
                {
                    return null;
                }

                return html;
            }

            return null;
        }
    }

    internal class ClipboardHtmlOutput
    {
        internal int endFragment;
        internal int endHTML;
        internal int startFragment;

        internal int startHTML;
        public double Version { get; internal set; }
        public string Source { get; internal set; }

        public string Input { get; internal set; }

        //public String Html { get { return Input.Substring(startHTML, (endHTML - startHTML)); } }
        public string Html => Input.Substring(startHTML, Math.Min(endHTML - startHTML, Input.Length - startHTML));

        public string Fragment => Input.Substring(startFragment, endFragment - startFragment);
    }
}