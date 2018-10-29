using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Noterium.Code.Markdown
{
    public static class RegularExpressions
    {
        /// <summary>
        ///     maximum nested depth of [] and () supported by the transform; implementation detail
        /// </summary>
        private const int NestDepth = 6;

        private static string _nestedBracketsPattern;
        private static string _nestedParensPattern;

        public static Regex ConstantVariables = new Regex(@"^\$\$var[\s]*?(?<varName>[a-zA-Z]*?)[\s]*?=[\s]*?(?<varValue>[1-9]*?)\$\$$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public static Regex CommentBlock = new Regex(@"/\*[^*]*.*?\*/", RegexOptions.Compiled | RegexOptions.Multiline);

        public static Regex HighlightedText = new Regex(@"\[\[\[(.*?)\]\]\]", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex AnchorSource = new Regex(@"^[\s]*\[(?<id>[A-Za-z0-9]+)\]:[\s]*(?<link>.*?)[\s]*(['""](?<title>.*?)['""])?$",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex SimpleAnchorInline = new Regex(@"<((https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|$!:,.;]*[A-Z0-9+&@#/%=~_|$])>", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex ReferenceAnchorInline = new Regex($@"
                (                           # wrap whole match in $1
                    \[
                        ({GetNestedBracketsPattern()})               # link text = $2
                    \]
                    \[
                        ([A-Za-z0-9]+)
                    \]
                )",
            RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex AnchorInline = new Regex($@"
                (                           # wrap whole match in $1
                    \[
                        ({GetNestedBracketsPattern()})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({GetNestedParensPattern()})               # href = $3
                        [ ]*
                        (                   # $4
                        (['""])           # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        [ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )",
            RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex ImageAnchorInline = new Regex($@"
                (                           # wrap whole match in $1
                    !\[
                        ({GetNestedBracketsPattern()})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({GetNestedParensPattern()})               # href = $3
                        [ ]*
                        (                   # $4
                        (.*?)               # title = $6
                        [ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )",
            RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex NewlinesLeadingTrailing = new Regex(@"^\n+|\n+\z", RegexOptions.Compiled);
        public static Regex NewlinesMultiple = new Regex(@"\n{2,}", RegexOptions.Compiled);
        public static Regex LeadingWhitespace = new Regex(@"^[ ]*", RegexOptions.Compiled);

        public static Regex Blockquote = new Regex(@"
            (                           # Wrap whole match in $1
                (
                ^[ ]*>[ ]?              # '>' at the start of a line
                    .+\n                # rest of the first line
                (.+\n)*                 # subsequent consecutive lines
                \n*                     # blanks
                )+
            )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        public static Regex CodeBlock = new Regex(string.Format(@"
                    (?:\n\n|\A\n?)
                    (                        # $1 = the code block -- one or more lines, starting with a space
                    (?:
                        (?:[ ]{{{0}}})       # Lines must start with a tab-width of spaces
                        .*\n+
                    )+
                    )
                    ((?=^[ ]{{0,{0}}}[^ \t\n])|\Z) # Lookahead for non-space at line-start, or end of doc",
                SharedSettings.TabWidth), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex Table = new Regex(@"^\|(.+)\n *\|(.*[-:]+[-:].*)\n((?:.*\|.*(?:\n|$))*)(\n{{0,2}})*",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        public static Regex TableNoAlignRow = new Regex(@"^\|(.+)\n ((?:.*\|.*(?:\n|$))*)(\n{{0,2}})*",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        public static Regex HeaderSetext = new Regex(@"
                ^(.+?)
                [ ]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ ]*
                \n+",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex HeaderAtx = new Regex(@"
                ^(\#{1,6})  # $1 = string of #'s
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \#*         # optional closing #'s (not counted)
                \n+",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly string _listFormat = @"
            (                               # $1 = whole list
              (                             # $2
                [ ]{{0,{1}}}
                ({0})                       # $3 = first list item marker
                [ ]+
              )
              (?s:.+?)
              (                             # $4
                  \z
                |
                  \n{{2,}}
                  (?=\S)
                  (?!                       # Negative lookahead for another list item marker
                    [ ]*
                    {0}[ ]+
                  )
              )
            )";

        private static readonly string WholeList = string.Format(_listFormat, string.Format("(?:{0}|{1})", SharedSettings.MarkerUl, SharedSettings.MarkerOl), SharedSettings.TabWidth - 1);
        private static readonly string TodoList = string.Format(_listFormat, string.Format("(?:{0})", SharedSettings.MarkerToDo), SharedSettings.TabWidth - 1);

        public static Regex ListNested = new Regex(@"^" + WholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex ListTopLevel = new Regex(@"(?:(?<=\n\n)|\A\n?)" + WholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex ToDoList = new Regex(@"(?:(?<=\n\n)|\A\n?)" + TodoList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Regex CodeSpan = new Regex(@"
                    (?<!\\)   # Character before opening ` can't be a backslash
                    (`+)      # $1 = Opening run of `
                    (.+?)     # $2 = The code block
                    (?<!`)
                    \1
                    (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex OutDent = new Regex(@"^[ ]{1," + SharedSettings.TabWidth + @"}", RegexOptions.Multiline | RegexOptions.Compiled);

        public static Regex Math = new Regex(@"(\$\$) (?=\S) (.+?[$]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex Bold = new Regex(@"(\*\*|__) (?=\S) (.+?[*_]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex Strike = new Regex(@"(~~) (?=\S) (.+?[*_]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex StrictBold = new Regex(@"([\W_]|^) (\*\*|__) (?=\S) ([^\r]*?\S[\*_]*) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex Italic = new Regex(@"(\*|_) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex StrictItalic = new Regex(@"([\W_]|^) (\*|_) (?=\S) ([^\r\*_]*?\S) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex HardNewLines = new Regex(@"(\s{2,}|\\)\n",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex HorizontalRules = new Regex(@"
            ^[ ]{0,3}         # Leading space
                ([-*_])       # $1: First marker
                (?>           # Repeated marker group
                    [ ]{0,2}  # Zero, one, or two spaces.
                    \1        # Marker character
                ){2,}         # Group repeated at least twice
                [ ]*          # Trailing spaces
                $             # End of line.
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        ///     Reusable pattern to match balanced [brackets]. See Friedl's
        ///     "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedBracketsPattern()
        {
            // in other words [this] and [this[also]] and [this[also[too]]]
            // up to _nestDepth
            if (_nestedBracketsPattern == null)
                _nestedBracketsPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^\[\]]+      # Anything other than brackets
                     |
                       \[
                           ", NestDepth) + RepeatString(
                        @" \]
                    )*"
                        , NestDepth);
            return _nestedBracketsPattern;
        }

        /// <summary>
        ///     Reusable pattern to match balanced (parens). See Friedl's
        ///     "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPattern()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if (_nestedParensPattern == null)
                _nestedParensPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()\s]+      # Anything other than parens or whitespace
                     |
                       \(
                           ", NestDepth) + RepeatString(
                        @" \)
                    )*"
                        , NestDepth);
            return _nestedParensPattern;
        }

        /// <summary>
        ///     this is to emulate what's evailable in PHP
        /// </summary>
        private static string RepeatString(string text, int count)
        {
            if (text == null) throw new ArgumentNullException("text");

            var sb = new StringBuilder(text.Length * count);
            for (var i = 0; i < count; i++)
                sb.Append(text);
            return sb.ToString();
        }
    }
}