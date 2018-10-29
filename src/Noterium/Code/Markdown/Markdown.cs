using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Noterium.Core.DataCarriers;

namespace Noterium.Code.Markdown
{
    public class Markdown : DependencyObject
    {
        private static readonly Regex EolnRegex = new Regex("\\s+");
        private Dictionary<string, LinkReference> _anchorSources;
        private bool _highlight;
        private Regex _highlightReg;
        private List<Run> _highlights;
        private string _highlightText;
        private int _listLevel;

        public Markdown()
        {
            HyperlinkCommand = NavigationCommands.GoToPage;
        }

        public ReadOnlyCollection<Run> Highlights => _highlights.AsReadOnly();

        public string Transform(FlowDocument fd)
        {
            return string.Empty;
        }

        public FlowDocument Transform(string text, string highlightText = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            _checkBoxNumber = 0;
            text = Normalize(text);

            //text = InitMathVariables(text);
            text = RegularExpressions.CommentBlock.Replace(text, string.Empty);
            text = text.Trim() + Environment.NewLine;

            _highlights = new List<Run>();
            _highlightText = highlightText;
            _highlight = !string.IsNullOrWhiteSpace(_highlightText);

            _highlightReg = _highlight ? new Regex($"({_highlightText})?", RegexOptions.IgnoreCase | RegexOptions.Multiline) : null;

            _anchorSources = GetAnchorSources(text);
            text = CleanAnchorSources(text);

            var document = Create<FlowDocument, Block>(RunBlockGamut(text));

            document.PagePadding = new Thickness(0);
            if (DocumentStyle != null) document.Style = DocumentStyle;

            return document;
        }

        //private string InitMathVariables(string text)
        //{
        //	if (text == null)
        //	{
        //		throw new ArgumentNullException(nameof(text));
        //	}

        //	_mathParams.Clear();

        //	var matches = RegularExpressions.ConstantVariables.Matches(text);
        //	foreach (Match m in matches)
        //	{
        //		string name = m.Groups[1].Value;
        //		string value = m.Groups[2].Value;

        //		double doubleValue;
        //		if (double.TryParse(value, out doubleValue))
        //			_mathParams.Add(new Constant(name, doubleValue));

        //		text = text.Replace(m.Value, string.Empty);
        //	}

        //	return text;
        //}

        public string StripMarkdown(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            text = Normalize(text);

            text = CleanAnchorSources(text);
            text = text.Replace("\'", string.Empty)
                .Replace("\"", string.Empty)
                .Replace("*", string.Empty)
                .Replace("=", string.Empty)
                .Replace("-", string.Empty);

            return text;
        }

        private string CleanAnchorSources(string text)
        {
            return RegularExpressions.AnchorSource.Replace(text, string.Empty);
        }

        /// <summary>
        ///     Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Block> RunBlockGamut(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return DoHeaders(text,
                s0 => DoImageAnchors(s0,
                    s1 => DoHorizontalRules(s1,
                        s2 => DoTodoLists(s2,
                            s3 => DoLists(s3,
                                s4 => DoBlockQuotes(s4,
                                    s5 => DoTables(s5,
                                        s6 => DoTablesNoAlignRow(s6,
                                            s7 => DoCodeBlocks(s7,
                                                FormParagraphs)))))))));

            //text = DoCodeBlocks(text);
            //text = DoBlockQuotes(text);

            //// We already ran HashHTMLBlocks() before, in Markdown(), but that
            //// was to escape raw HTML in the original Markdown source. This time,
            //// we're escaping the markup we've just created, so that we don't wrap
            //// <p> tags around block-level tags.
            //text = HashHTMLBlocks(text);

            //text = FormParagraphs(text);

            //return text;
        }

        /// <summary>
        ///     Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Inline> RunSpanGamut(string text, bool highlight = true)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return DoCodeSpans(text, highlight,
                s0 => DoHardBreaks(s0,
                    //s10 => DoHighlightText(s10,
                    s1 => DoSimpleAnchors(s1,
                        s2 => DoAnchors(s2, highlight,
                            s3 => DoReferenceAnchors(s3,
                                s4 => DoMath(s4,
                                    s5 => DoItalicsAndBold(s5, highlight,
                                        s6 => DoText(s6, highlight))))))));

            //text = EscapeSpecialCharsWithinTagAttributes(text);
            //text = EscapeBackslashes(text);

            //// Images must come first, because ![foo][f] looks like an anchor.
            //text = DoImages(text);
            //text = DoAnchors(text);

            //// Must come after DoAnchors(), because you can use < and >
            //// delimiters in inline links like [this](<url>).
            //text = DoAutoLinks(text);

            //text = EncodeAmpsAndAngles(text);
            //text = DoItalicsAndBold(text);
            //text = DoHardBreaks(text);

            //return text;
        }

        private IEnumerable<Inline> DoHardBreaks(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.HardNewLines, LineBreakEvaluator, defaultHandler);
        }

        private Inline LineBreakEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            return new LineBreak();
        }

        private IEnumerable<Block> DoBlockQuotes(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.Blockquote, BlockQuoteEvaluator, defaultHandler);
        }

        private Block BlockQuoteEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var quote = match.Groups[0].Value;
            quote = quote.TrimStart('>').Replace("\n>", "\n");
            var result = new Run(quote.Trim());

            var pg = new Paragraph(result);
            if (QuoteStyle != null) pg.Style = QuoteStyle;

            return pg;
        }

        private IEnumerable<Block> DoCodeBlocks(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.CodeBlock, CodeBlocksEvaluator, defaultHandler);
        }

        private Block CodeBlocksEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var codeBlock = match.Groups[0].Value;
            codeBlock = codeBlock.Trim('\n', '\r');
            codeBlock = Environment.NewLine + codeBlock + Environment.NewLine;
            var result = new Run(codeBlock);

            var pg = new Paragraph(result);
            if (CodeBlockStyle != null) pg.Style = CodeBlockStyle;

            return pg;
        }

        private IEnumerable<Block> DoTables(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.Table, TableEvaluator, defaultHandler);
        }

        private IEnumerable<Block> DoTablesNoAlignRow(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.TableNoAlignRow, TableNoAlignEvaluator, defaultHandler);
        }

        private Block TableNoAlignEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var headString = match.Groups[1].Value;
            var rowsString = match.Groups[2].Value;

            return GenerateTable(headString, null, rowsString);
        }

        private Block TableEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var headString = match.Groups[1].Value;
            var textAlign = match.Groups[2].Value;
            var rowsString = match.Groups[3].Value;

            return GenerateTable(headString, textAlign, rowsString);
        }

        private Block GenerateTable(string headString, string textAlign, string rowsString)
        {
            var headColumns = headString.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            var rows = rowsString.Split('\n');

            TextAlignment[] alignments = null;

            if (!string.IsNullOrEmpty(textAlign))
            {
                var textAlignColumns = textAlign.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                alignments = GetTextAlignmentsArray(textAlignColumns);
            }

            var t = new Table();

            if (TableStyle != null)
                t.Style = TableStyle;

            for (var i = 0; i < headColumns.Length; i++)
            {
                var c = new TableColumn();
                if (TableColumnStyle != null)
                    c.Style = TableColumnStyle;
                t.Columns.Add(c);
            }

            var tableHeadRowGroup = new TableRowGroup();
            t.RowGroups.Add(tableHeadRowGroup);
            var headRow = new TableRow();
            if (TableHeadStyle != null)
                headRow.Style = TableHeadStyle;

            for (var i = 0; i < headColumns.Length; i++)
            {
                var rowColumn = headColumns[i];
                var p = new Paragraph(new Run(rowColumn.Trim()));

                if (alignments != null)
                    p.TextAlignment = alignments[i];

                var cell = new TableCell(p);
                if (TableCellStyle != null)
                    cell.Style = TableCellStyle;

                headRow.Cells.Add(cell);
            }

            tableHeadRowGroup.Rows.Add(headRow);

            var tableContentRowGroup = new TableRowGroup();
            t.RowGroups.Add(tableContentRowGroup);
            foreach (var rowString in rows)
            {
                var trimmed = rowString.Trim();
                var rowColumns = trimmed.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                var row = new TableRow();
                if (TableRowStyle != null)
                    row.Style = TableRowStyle;

                for (var i = 0; i < rowColumns.Length; i++)
                {
                    var rowColumn = rowColumns[i];

                    var text = rowColumn.Trim();

                    var textElements = RunSpanGamut(text);

                    //Run cellContent;
                    //if (text.Contains("$$"))
                    //{
                    //    text = text.Trim('$');
                    //    cellContent = EvaluateMathExpression(text);
                    //}
                    //else
                    //{
                    //    cellContent = new Run(text);
                    //}

                    var p = new Paragraph();
                    p.Inlines.AddRange(textElements);

                    if (alignments != null)
                        if (i < alignments.Length)
                            p.TextAlignment = alignments[i];

                    var cell = new TableCell(p);
                    if (TableCellStyle != null)
                        cell.Style = TableCellStyle;

                    row.Cells.Add(cell);
                }

                tableContentRowGroup.Rows.Add(row);
            }

            return t;
        }

        private static TextAlignment[] GetTextAlignmentsArray(string[] textAlignColumns)
        {
            var alignments = new TextAlignment[textAlignColumns.Length];
            for (var i = 0; i < textAlignColumns.Length; i++)
            {
                var textAlignColumn = textAlignColumns[i];
                if (textAlignColumn.StartsWith(":") && textAlignColumn.EndsWith(":"))
                    alignments[i] = TextAlignment.Center;
                else if (textAlignColumn.StartsWith(":"))
                    alignments[i] = TextAlignment.Left;
                else if (textAlignColumn.EndsWith(":"))
                    alignments[i] = TextAlignment.Right;
                else
                    alignments[i] = TextAlignment.Left;
            }

            return alignments;
        }

        /// <summary>
        ///     splits on two or more newlines, to form "paragraphs";
        /// </summary>
        private IEnumerable<Block> FormParagraphs(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            // split on two or more newlines
            var grafs = RegularExpressions.NewlinesMultiple.Split(RegularExpressions.NewlinesLeadingTrailing.Replace(text, ""));

            return grafs.Select(g =>
            {
                var p = Create<Paragraph, Inline>(RunSpanGamut(g));
                if (ParagraphStyle != null)
                    p.Style = ParagraphStyle;
                return p;
            });
        }

        private Dictionary<string, LinkReference> GetAnchorSources(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var result = new Dictionary<string, LinkReference>();


            var matches = RegularExpressions.AnchorSource.Matches(text);
            foreach (Match m in matches)
            {
                var key = m.Groups["id"].Value;
                var url = m.Groups["link"].Value;
                var title = m.Groups["title"].Value;

                if (!result.ContainsKey(key))
                    result.Add(key, new LinkReference(key, url, title));

                //TODO: Remove anchor sources in text...
            }


            return result;
        }

        /// <summary>
        ///     Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        ///     [link text](url "title")
        /// </remarks>
        private IEnumerable<Inline> DoMath(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.Math, MathEvaluator, defaultHandler);
        }

        /// <summary>
        ///     Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        ///     [link text](url "title")
        /// </remarks>
        private IEnumerable<Inline> DoAnchors(string text, bool highlight, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            // Next, inline-style links: [link text](url "optional title") or [link text](url "optional title")
            if (highlight)
                return Evaluate(text, RegularExpressions.AnchorInline, AnchorInlineEvaluatorHighlight, defaultHandler);

            return Evaluate(text, RegularExpressions.AnchorInline, AnchorInlineEvaluatorNoHighlight, defaultHandler);
        }

        private IEnumerable<Inline> DoReferenceAnchors(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.ReferenceAnchorInline, AnchorInlineEvaluatorNoHighlight, defaultHandler);
        }

        /// <summary>
        ///     Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        ///     [link text](url "title")
        /// </remarks>
        private IEnumerable<Inline> DoSimpleAnchors(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            // Next, inline-style links: <url>
            return Evaluate(text, RegularExpressions.SimpleAnchorInline, SimpleAnchorInlineEvaluator, defaultHandler);
        }

        /// <summary>
        ///     Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        ///     [link text](url "title")
        /// </remarks>
        private IEnumerable<Block> DoImageAnchors(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            // Next, inline-style links: [link text](url "optional title") or [link text](url "optional title")
            return Evaluate(text, RegularExpressions.ImageAnchorInline, ImageInlineEvaluator, defaultHandler);
        }

        private Inline SimpleAnchorInlineEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var url = match.Groups[1].Value;

            var result = Create<Hyperlink, Inline>(DoHighlightText(url));
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;

            if (LinkStyle != null)
                result.Style = LinkStyle;

            return result;
        }

        private Inline AnchorInlineEvaluatorHighlight(Match match)
        {
            return AnchorInlineEvaluator(match, true);
        }

        private Inline AnchorInlineEvaluatorNoHighlight(Match match)
        {
            return AnchorInlineEvaluator(match, false);
        }

        private Inline AnchorInlineEvaluator(Match match, bool highlight)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var linkText = match.Groups[2].Value;
            var url = match.Groups[3].Value;
            var title = match.Groups[6].Value;

            LinkReference reference = null;
            if (_anchorSources.ContainsKey(url)) reference = _anchorSources[url];

            if (reference != null)
            {
                url = reference.Link;
                title = reference.Title;
            }

            if (string.IsNullOrWhiteSpace(linkText))
                linkText = url;

            var result = Create<Hyperlink, Inline>(RunSpanGamut(linkText, highlight));
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;
            if (!string.IsNullOrWhiteSpace(title))
                result.ToolTip = title;

            if (LinkStyle != null)
                result.Style = LinkStyle;

            return result;
        }

        private Inline MathEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var mathExpression = match.Groups[0].Value;
            if (mathExpression.Contains("$"))
                mathExpression = mathExpression.Trim('$');

            return EvaluateMathExpression(mathExpression);
        }

        private Run EvaluateMathExpression(string math)
        {
            Run result = null;
            var message = string.Empty;

            try
            {
                //org.mariuszgromada.math.mxparser.Expression e = new org.mariuszgromada.math.mxparser.Expression(math, _mathParams.ToArray());
                //double expressionResult = e.calculate();
                //message = e.getErrorMessage();

                //if (expressionResult == double.NaN)
                //	throw new Exception(message);

                //using (DataTable dt = new DataTable())
                //{
                //    object o = dt.Compute(math, null);
                //    if (o is int)
                //        result = new Run(((int)o).ToString(CultureInfo.CurrentCulture));
                //    else if (o is double)
                //        result = new Run(((double)o).ToString(CultureInfo.CurrentCulture));
                //    else if (o is decimal)
                //        result = new Run(((decimal)o).ToString(CultureInfo.CurrentCulture));
                //    else
                //        message = o.ToString();
                //}

                //result = new Run(expressionResult.ToString(CultureInfo.CurrentCulture));
                //if (!string.IsNullOrWhiteSpace(message))
                //	result.ToolTip = message;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                message = e.Message;
            }

            return result ?? new Run("Invalid math expression") {ToolTip = message, Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0))};
        }

        private Block ImageInlineEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            //string linkText = match.Groups[2].Value;
            var url = match.Groups[3].Value;
            var options = match.Groups[4].Value;
            var title = match.Groups[6].Value;

            var nf = Note.Files.FirstOrDefault(n => n.FileName == url);
            var result = new Paragraph();

            if (nf != null && File.Exists(nf.FullName))
            {
                if (!string.IsNullOrWhiteSpace(title))
                    result.ToolTip = title;

                var imageUri = new Uri(nf.FullName, UriKind.Absolute);

                var imgTemp = new BitmapImage();
                imgTemp.BeginInit();
                imgTemp.CacheOption = BitmapCacheOption.OnLoad;
                imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                imgTemp.UriSource = imageUri;
                imgTemp.EndInit();

                var image = new Image
                {
                    Source = imgTemp,
                    Stretch = Stretch.None,
                    StretchDirection = StretchDirection.Both,
                    MaxWidth = imgTemp.Width,
                    MaxHeight = imgTemp.Height
                };

                string floatDirection = null;
                if (!string.IsNullOrWhiteSpace(options))
                {
                    //options = options.Trim('=');

                    var reg = new Regex(@"=(?:(?<width>\d{1,})x?(?<height>\d{1,})?)?;?(?<float>\w{1,})?", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

                    var m = reg.Match(options);
                    //string[] chunks = options.Split('x');
                    var widthGroup = m.Groups["width"];
                    if (widthGroup != null && widthGroup.Success)
                    {
                        int width;
                        if (int.TryParse(widthGroup.Value, out width))
                            image.Width = width;

                        var heightGroup = m.Groups["height"];
                        if (heightGroup != null && heightGroup.Success)
                        {
                            int height;
                            if (int.TryParse(heightGroup.Value, out height))
                                image.Height = height;
                        }
                        else
                        {
                            image.Stretch = Stretch.Uniform;
                        }
                    }

                    var floatGroup = m.Groups["float"];
                    if (floatGroup != null && floatGroup.Success) floatDirection = floatGroup.Value;
                }

                if (ImageStyle != null)
                    image.Style = ImageStyle;

                if (!string.IsNullOrWhiteSpace(floatDirection))
                {
                    var imgContainer = new BlockUIContainer {Child = image};

                    var fl = new Floater(imgContainer);
                    if (image.Width > -1)
                        fl.Width = image.Width;

                    if (floatDirection.Equals("left"))
                        fl.HorizontalAlignment = HorizontalAlignment.Left;
                    else if (floatDirection.Equals("right")) fl.HorizontalAlignment = HorizontalAlignment.Right;

                    result.Inlines.Add(fl);
                }
                else
                {
                    result.Inlines.Add(image);
                }
            }
            else if (nf != null)
            {
                result.Inlines.Add(new Run("Missing file: " + nf.FullName));
            }

            return result;
        }

        /// <summary>
        ///     Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        ///     Header 1
        ///     ========
        ///     Header 2
        ///     --------
        ///     # Header 1
        ///     ## Header 2
        ///     ## Header 2 with closing hashes ##
        ///     ...
        ///     ###### Header 6
        /// </remarks>
        private IEnumerable<Block> DoHeaders(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.HeaderSetext, SetextHeaderEvaluator,
                s => Evaluate(s, RegularExpressions.HeaderAtx, AtxHeaderEvaluator, defaultHandler));
        }

        private Block SetextHeaderEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var header = match.Groups[1].Value;
            var level = match.Groups[2].Value.StartsWith("=") ? 1 : 2;

            return CreateHeader(level, RunSpanGamut(header.Trim()));
        }

        private Block AtxHeaderEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var header = match.Groups[2].Value;
            var level = match.Groups[1].Value.Length;
            return CreateHeader(level, RunSpanGamut(header));
        }

        public Block CreateHeader(int level, IEnumerable<Inline> content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var block = Create<Paragraph, Inline>(content);

            switch (level)
            {
                case 1:
                    if (Heading1Style != null) block.Style = Heading1Style;
                    break;

                case 2:
                    if (Heading2Style != null) block.Style = Heading2Style;
                    break;

                case 3:
                    if (Heading3Style != null) block.Style = Heading3Style;
                    break;

                case 4:
                    if (Heading4Style != null) block.Style = Heading4Style;
                    break;
            }

            return block;
        }

        /// <summary>
        ///     Turn Markdown horizontal rules into HTML hr tags
        /// </summary>
        /// <remarks>
        ///     ***
        ///     * * *
        ///     ---
        ///     - - -
        /// </remarks>
        private IEnumerable<Block> DoHorizontalRules(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.HorizontalRules, RuleEvaluator, defaultHandler);
        }

        private Block RuleEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var line = new Line {X2 = 1, StrokeThickness = 1.0};
            var container = new BlockUIContainer(line);
            return container;
        }

        /// <summary>
        ///     Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        private IEnumerable<Block> DoTodoLists(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            return Evaluate(text, RegularExpressions.ToDoList, TodoListEvaluator, defaultHandler);
        }

        /// <summary>
        ///     Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        private IEnumerable<Block> DoLists(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            // We use a different prefix before nested lists than top-level lists.
            // See extended comment in _ProcessListItems().
            if (_listLevel > 0)
                return Evaluate(text, RegularExpressions.ListNested, ListEvaluator, defaultHandler);
            return Evaluate(text, RegularExpressions.ListTopLevel, ListEvaluator, defaultHandler);
        }

        private Block ListEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var list = match.Groups[1].Value;
            var listType = Regex.IsMatch(match.Groups[3].Value, SharedSettings.MarkerUl) ? "ul" : "ol";

            // Turn double returns into triple returns, so that we can make a
            // paragraph for the last item in a list, if necessary:
            list = Regex.Replace(list, @"\n{2,}", "\n\n\n");

            var resultList = Create<List, ListItem>(ProcessListItems(list, listType == "ul" ? SharedSettings.MarkerUl : SharedSettings.MarkerOl));

            resultList.MarkerStyle = listType == "ul" ? TextMarkerStyle.Disc : TextMarkerStyle.Decimal;

            return resultList;
        }

        private Block TodoListEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var content = match.Groups[1].Value;
            var list = content.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            var sp = new StackPanel();
            var ctnr = new BlockUIContainer(sp);

            foreach (var s in list)
            {
                var text = s.Substring(s.IndexOf("] ", StringComparison.Ordinal) + 2).Trim();

                var spcb = new TextBlock();
                spcb.Style = TodoTextStyle;
                var spans = RunSpanGamut(text);
                spcb.Inlines.AddRange(spans);

                var bt = new CheckBox
                {
                    IsChecked = s.Contains("[x]"),
                    Content = spcb,
                    Tag = _checkBoxNumber
                };
                bt.CommandParameter = bt;
                bt.Command = CheckBoxCheckedCommand;
                bt.Style = TodoCheckBoxStyle;
                _checkBoxNumber++;
                sp.Children.Add(bt);
            }

            //sp.Children.Add(new Button()
            //{
            //	Content = "Clear finished",
            //	Command = CheckBoxCheckedCommand,
            //	CommandParameter = "CLEAR"
            //});

            return ctnr;
        }

        /// <summary>
        ///     Process the contents of a single ordered or unordered list, splitting it
        ///     into individual list items.
        /// </summary>
        private IEnumerable<ListItem> ProcessListItems(string list, string marker)
        {
            // The listLevel global keeps track of when we're inside a list.
            // Each time we enter a list, we increment it; when we leave a list,
            // we decrement. If it's zero, we're not in a list anymore.

            // We do this because when we're not inside a list, we want to treat
            // something like this:

            //    I recommend upgrading to version
            //    8. Oops, now this line is treated
            //    as a sub-list.

            // As a single paragraph, despite the fact that the second line starts
            // with a digit-period-space sequence.

            // Whereas when we're inside a list (or sub-list), that line will be
            // treated as the start of a sub-list. What a kludge, huh? This is
            // an aspect of Markdown's syntax that's hard to parse perfectly
            // without resorting to mind-reading. Perhaps the solution is to
            // change the syntax rules such that sub-lists must start with a
            // starting cardinal number; e.g. "1." or "a.".

            _listLevel++;
            try
            {
                // Trim trailing blank lines:
                list = Regex.Replace(list, @"\n{2,}\z", "\n");

                var pattern = string.Format(
                    @"(\n)?                      # leading line = $1
                (^[ ]*)                    # leading whitespace = $2
                ({0}) [ ]+                 # list marker = $3
                ((?s:.+?)                  # list item text = $4
                (\n{{0,2}}))      
                (?= \n* (\z | \2 ({0}) [ ]+))", marker);

                var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                var matches = regex.Matches(list);
                foreach (Match m in matches) yield return ListItemEvaluator(m);
            }
            finally
            {
                _listLevel--;
            }
        }

        private ListItem ListItemEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var item = match.Groups[4].Value;
            var leadingLine = match.Groups[1].Value;

            if (!string.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
                // we could correct any bad indentation here..
                return Create<ListItem, Block>(RunBlockGamut(item));
            return Create<ListItem, Block>(RunBlockGamut(item));
        }

        /// <summary>
        ///     Turn Markdown `code spans` into HTML code tags
        /// </summary>
        private IEnumerable<Inline> DoCodeSpans(string text, bool highlight, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            //    * You can use multiple backticks as the delimiters if you want to
            //        include literal backticks in the code span. So, this input:
            //
            //        Just type ``foo `bar` baz`` at the prompt.
            //
            //        Will translate to:
            //
            //          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
            //
            //        There's no arbitrary limit to the number of backticks you
            //        can use as delimters. If you need three consecutive backticks
            //        in your code, use four for delimiters, etc.
            //
            //    * You can use spaces to get literal backticks at the edges:
            //
            //          ... type `` `bar` `` ...
            //
            //        Turns to:
            //
            //          ... type <code>`bar`</code> ...
            //
            if (highlight)
                return Evaluate(text, RegularExpressions.CodeSpan, CodeSpanEvaluatorHighlight, defaultHandler);

            return Evaluate(text, RegularExpressions.CodeSpan, CodeSpanEvaluatorNoHighlight, defaultHandler);
        }

        private Inline CodeSpanEvaluatorNoHighlight(Match match)
        {
            return CodeSpanEvaluator(match, false);
        }

        private Inline CodeSpanEvaluatorHighlight(Match match)
        {
            return CodeSpanEvaluator(match, true);
        }

        private Inline CodeSpanEvaluator(Match match, bool highlight)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            Inline result = Create<Span, Inline>(DoText(span, highlight));

            if (InlineCodeStyle != null)
                result.Style = InlineCodeStyle;

            return result;
        }

        private IEnumerable<Inline> DoHighlightText(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (!_highlight)
            {
                var run = new Run(text);
                if (TextStyle != null)
                    run.Style = TextStyle;
                return new List<Inline> {new Span(run)};
            }

            return Evaluate(text, _highlightReg, HighlightedTextEvaluator, DoTextNoHighlight);
        }

        private Inline HighlightedTextEvaluator(Match match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var content = match.Groups[1].Value;
            var result = new Run(content);
            if (HighlightedTextStyle != null)
                result.Style = HighlightedTextStyle;

            _highlights.Add(result);

            return result;
        }

        /// <summary>
        ///     Turn Markdown *italics* and **bold** into HTML strong and em tags
        /// </summary>
        private IEnumerable<Inline> DoItalicsAndBold(string text, bool highlight, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            // <strong> must go first, then <em>
            if (StrictBoldItalic)
                return Evaluate(text, RegularExpressions.StrictBold, m => BoldEvaluator(m, 3, highlight),
                    s1 => Evaluate(s1, RegularExpressions.StrictItalic, m => ItalicEvaluator(m, 3, highlight),
                        s2 => Evaluate(s2, RegularExpressions.Strike, m => StrikeEvaluator(m, 2, highlight),
                            defaultHandler)));
            return Evaluate(text, RegularExpressions.Bold, m => BoldEvaluator(m, 2, highlight),
                s1 => Evaluate(s1, RegularExpressions.Italic, m => ItalicEvaluator(m, 2, highlight),
                    s2 => Evaluate(s2, RegularExpressions.Strike, m => StrikeEvaluator(m, 2, highlight),
                        defaultHandler)));
        }

        private Inline ItalicEvaluator(Match match, int contentGroup, bool highlight)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var content = match.Groups[contentGroup].Value;
            return Create<Italic, Inline>(RunSpanGamut(content, highlight));
        }

        private Inline StrikeEvaluator(Match match, int contentGroup, bool highlight)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var content = match.Groups[contentGroup].Value;
            var result = Create<Span, Inline>(RunSpanGamut(content, highlight));
            result.TextDecorations = TextDecorations.Strikethrough;

            return result;
        }

        private Inline BoldEvaluator(Match match, int contentGroup, bool highlight)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var content = match.Groups[contentGroup].Value;
            return Create<Bold, Inline>(RunSpanGamut(content, highlight));
        }

        /// <summary>
        ///     convert all tabs to _tabWidth spaces;
        ///     standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF);
        ///     makes sure text ends with a couple of newlines;
        ///     removes any blank lines (only spaces) in the text
        /// </summary>
        private string Normalize(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var output = new StringBuilder(text.Length);
            var line = new StringBuilder();
            var valid = false;

            for (var i = 0; i < text.Length; i++)
                switch (text[i])
                {
                    case '\n':
                        if (valid)
                            output.Append(line);
                        output.Append('\n');
                        line.Length = 0;
                        valid = false;
                        break;
                    case '\r':
                        if (i < text.Length - 1 && text[i + 1] != '\n')
                        {
                            if (valid)
                                output.Append(line);
                            output.Append('\n');
                            line.Length = 0;
                            valid = false;
                        }

                        break;
                    case '\t':
                        var width = SharedSettings.TabWidth - line.Length % SharedSettings.TabWidth;
                        for (var k = 0; k < width; k++)
                            line.Append(' ');
                        break;
                    case '\x1A':
                        break;
                    default:
                        if (!valid && text[i] != ' ')
                            valid = true;
                        line.Append(text[i]);
                        break;
                }

            if (valid)
                output.Append(line);
            output.Append('\n');

            // add two newlines to the end before return
            return output.Append("\n\n").ToString();
        }

        private TResult Create<TResult, TContent>(IEnumerable<TContent> content)
            where TResult : IAddChild, new()
        {
            var result = new TResult();
            foreach (var c in content) result.AddChild(c);

            return result;
        }

        private IEnumerable<T> Evaluate<T>(string text, Regex expression, Func<Match, T> build, Func<string, IEnumerable<T>> rest)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var matches = expression.Matches(text);
            var index = 0;
            foreach (Match m in matches)
            {
                if (m.Index > index)
                {
                    var prefix = text.Substring(index, m.Index - index);
                    foreach (var t in rest(prefix)) yield return t;
                }

                yield return build(m);

                index = m.Index + m.Length;
            }

            if (index < text.Length)
            {
                var suffix = text.Substring(index, text.Length - index);
                foreach (var t in rest(suffix)) yield return t;
            }
        }

        public IEnumerable<Inline> DoTextNoHighlight(string text)
        {
            return DoText(text, false);
        }

        public IEnumerable<Inline> DoTextHighlight(string text)
        {
            return DoText(text, true);
        }

        public IEnumerable<Inline> DoText(string text, bool highlight)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var t = EolnRegex.Replace(text, " ");
            if (highlight && !string.IsNullOrWhiteSpace(_highlightText) && t.IndexOf(_highlightText, StringComparison.InvariantCultureIgnoreCase) > -1)
                return DoHighlightText(t);

            var run = new Run(t);
            if (TextStyle != null)
                run.Style = TextStyle;

            return new Inline[] {run};
        }

        public static string FileToBase64String(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var allData = reader.ReadBytes((int) reader.BaseStream.Length);
                return Convert.ToBase64String(allData);
            }
        }
        //private List<PrimitiveElement> _mathParams = new List<PrimitiveElement>();

        #region -- Propertys --

        /// <summary>
        ///     when true, bold and italic require non-word characters on either side
        ///     WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public bool StrictBoldItalic { get; set; }

        public ICommand HyperlinkCommand { get; set; }
        public ICommand CheckBoxCheckedCommand { get; set; }

        public Style DocumentStyle
        {
            get => (Style) GetValue(DocumentStyleProperty);
            set => SetValue(DocumentStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for DocumentStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentStyleProperty =
            DependencyProperty.Register("DocumentStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading1Style
        {
            get => (Style) GetValue(Heading1StyleProperty);
            set => SetValue(Heading1StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading1Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading1StyleProperty =
            DependencyProperty.Register("Heading1Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading2Style
        {
            get => (Style) GetValue(Heading2StyleProperty);
            set => SetValue(Heading2StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading2Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading2StyleProperty =
            DependencyProperty.Register("Heading2Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading3Style
        {
            get => (Style) GetValue(Heading3StyleProperty);
            set => SetValue(Heading3StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading3Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading3StyleProperty =
            DependencyProperty.Register("Heading3Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading4Style
        {
            get => (Style) GetValue(Heading4StyleProperty);
            set => SetValue(Heading4StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading4Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading4StyleProperty =
            DependencyProperty.Register("Heading4Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style CodeBlockStyle
        {
            get => (Style) GetValue(CodeBlockStyleProperty);
            set => SetValue(CodeBlockStyleProperty, value);
        }

        public static readonly DependencyProperty CodeBlockStyleProperty =
            DependencyProperty.Register("CodeBlockStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));


        public Style InlineCodeStyle
        {
            get => (Style) GetValue(InlineCodeStyleProperty);
            set => SetValue(InlineCodeStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for InlineCodeStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InlineCodeStyleProperty =
            DependencyProperty.Register("InlineCodeStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style QuoteStyle
        {
            get => (Style) GetValue(QuoteStyleProperty);
            set => SetValue(QuoteStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for InlineCodeStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QuoteStyleProperty =
            DependencyProperty.Register("QuoteStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableStyle
        {
            get => (Style) GetValue(TableStyleProperty);
            set => SetValue(TableStyleProperty, value);
        }

        public static readonly DependencyProperty TableStyleProperty =
            DependencyProperty.Register("TableStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableHeadStyle
        {
            get => (Style) GetValue(TableHeadStyleProperty);
            set => SetValue(TableHeadStyleProperty, value);
        }

        public static readonly DependencyProperty TableHeadStyleProperty =
            DependencyProperty.Register("TableHeadStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableCellStyle
        {
            get => (Style) GetValue(TableCellStyleProperty);
            set => SetValue(TableCellStyleProperty, value);
        }

        public static readonly DependencyProperty TableCellStyleProperty =
            DependencyProperty.Register("TableCellStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableRowStyle
        {
            get => (Style) GetValue(TableRowStyleProperty);
            set => SetValue(TableRowStyleProperty, value);
        }

        public static readonly DependencyProperty TableRowStyleProperty =
            DependencyProperty.Register("TableRowStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableColumnStyle
        {
            get => (Style) GetValue(TableColumnStyleProperty);
            set => SetValue(TableColumnStyleProperty, value);
        }

        public static readonly DependencyProperty TableColumnStyleProperty =
            DependencyProperty.Register("TableColumnStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style ImageStyle
        {
            get => (Style) GetValue(ImageStyleProperty);
            set => SetValue(ImageStyleProperty, value);
        }

        public static readonly DependencyProperty ImageStyleProperty =
            DependencyProperty.Register("ImageStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style LinkStyle
        {
            get => (Style) GetValue(LinkStyleProperty);
            set => SetValue(LinkStyleProperty, value);
        }

        public static readonly DependencyProperty LinkStyleProperty =
            DependencyProperty.Register("LinkStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public static readonly DependencyProperty ParagraphStyleProperty =
            DependencyProperty.Register("ParagraphStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style ParagraphStyle
        {
            get => (Style) GetValue(ParagraphStyleProperty);
            set => SetValue(ParagraphStyleProperty, value);
        }

        public static readonly DependencyProperty TextStyleProperty =
            DependencyProperty.Register("TextStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TextStyle
        {
            get => (Style) GetValue(TextStyleProperty);
            set => SetValue(TextStyleProperty, value);
        }


        public static readonly DependencyProperty HighlightedTextStyleProperty =
            DependencyProperty.Register("HighlightedTextStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style HighlightedTextStyle
        {
            get => (Style) GetValue(HighlightedTextStyleProperty);
            set => SetValue(HighlightedTextStyleProperty, value);
        }

        public static readonly DependencyProperty TodoCheckBoxStyleProperty =
            DependencyProperty.Register("TodoCheckBoxStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TodoCheckBoxStyle
        {
            get => (Style) GetValue(TodoCheckBoxStyleProperty);
            set => SetValue(TodoCheckBoxStyleProperty, value);
        }

        public static readonly DependencyProperty TodoTextStyleProperty =
            DependencyProperty.Register("TodoTextStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TodoTextStyle
        {
            get => (Style) GetValue(TodoTextStyleProperty);
            set => SetValue(TodoTextStyleProperty, value);
        }


        public Note Note
        {
            get => (Note) GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        // Using a DependencyProperty as the backing store for InlineCodeStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register("Note", typeof(Note), typeof(Markdown), new PropertyMetadata(null));

        private int _checkBoxNumber;

        #endregion
    }

    internal class LinkReference
    {
        public LinkReference(string id, string link, string title)
        {
            ID = id;
            Link = link;
            Title = title;
        }

        public string ID { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
    }
}