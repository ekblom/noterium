using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommonMark;
using CommonMark.Syntax;
using Noterium.Core.DataCarriers;
using Block = CommonMark.Syntax.Block;
using Inline = CommonMark.Syntax.Inline;

namespace Noterium.Code.Markdown
{
	public class XamlFormatter : DependencyObject
	{
		//private int _checkBoxNumber = 0;

		#region -- Propertys --

		public ICommand HyperlinkCommand { get; set; }
		public ICommand CheckBoxCheckedCommand { get; set; }

		public Style DocumentStyle
		{
			get { return (Style)GetValue(DocumentStyleProperty); }
			set { SetValue(DocumentStyleProperty, value); }
		}

		public static readonly DependencyProperty DocumentStyleProperty =
			DependencyProperty.Register("DocumentStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));
		public Style LineStyle
		{
			get { return (Style)GetValue(LineStyleProperty); }
			set { SetValue(LineStyleProperty, value); }
		}

		public static readonly DependencyProperty LineStyleProperty =
			DependencyProperty.Register("LineStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style Heading1Style
		{
			get { return (Style)GetValue(Heading1StyleProperty); }
			set { SetValue(Heading1StyleProperty, value); }
		}

		public static readonly DependencyProperty Heading1StyleProperty =
			DependencyProperty.Register("Heading1Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style Heading2Style
		{
			get { return (Style)GetValue(Heading2StyleProperty); }
			set { SetValue(Heading2StyleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Heading2Style.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty Heading2StyleProperty =
			DependencyProperty.Register("Heading2Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style Heading3Style
		{
			get { return (Style)GetValue(Heading3StyleProperty); }
			set { SetValue(Heading3StyleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Heading3Style.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty Heading3StyleProperty =
			DependencyProperty.Register("Heading3Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style Heading4Style
		{
			get { return (Style)GetValue(Heading4StyleProperty); }
			set { SetValue(Heading4StyleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Heading4Style.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty Heading4StyleProperty =
			DependencyProperty.Register("Heading4Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style CodeBlockStyle
		{
			get { return (Style)GetValue(CodeBlockStyleProperty); }
			set { SetValue(CodeBlockStyleProperty, value); }
		}

		public static readonly DependencyProperty CodeBlockStyleProperty =
			DependencyProperty.Register("CodeBlockStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));


		public Style InlineCodeStyle
		{
			get { return (Style)GetValue(InlineCodeStyleProperty); }
			set { SetValue(InlineCodeStyleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for InlineCodeStyle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty InlineCodeStyleProperty =
			DependencyProperty.Register("InlineCodeStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style QuoteStyle
		{
			get { return (Style)GetValue(QuoteStyleProperty); }
			set { SetValue(QuoteStyleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for InlineCodeStyle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty QuoteStyleProperty =
			DependencyProperty.Register("QuoteStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style TableStyle
		{
			get { return (Style)GetValue(TableStyleProperty); }
			set { SetValue(TableStyleProperty, value); }
		}

		public static readonly DependencyProperty TableStyleProperty =
			DependencyProperty.Register("TableStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style TableHeadStyle
		{
			get { return (Style)GetValue(TableHeadStyleProperty); }
			set { SetValue(TableHeadStyleProperty, value); }
		}

		public static readonly DependencyProperty TableHeadStyleProperty =
			DependencyProperty.Register("TableHeadStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style TableCellStyle
		{
			get { return (Style)GetValue(TableCellStyleProperty); }
			set { SetValue(TableCellStyleProperty, value); }
		}

		public static readonly DependencyProperty TableCellStyleProperty =
			DependencyProperty.Register("TableCellStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style TableRowStyle
		{
			get { return (Style)GetValue(TableRowStyleProperty); }
			set { SetValue(TableRowStyleProperty, value); }
		}

		public static readonly DependencyProperty TableRowStyleProperty =
			DependencyProperty.Register("TableRowStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style TableColumnStyle
		{
			get { return (Style)GetValue(TableColumnStyleProperty); }
			set { SetValue(TableColumnStyleProperty, value); }
		}

		public static readonly DependencyProperty TableColumnStyleProperty =
			DependencyProperty.Register("TableColumnStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style ImageStyle
		{
			get { return (Style)GetValue(ImageStyleProperty); }
			set { SetValue(ImageStyleProperty, value); }
		}

		public static readonly DependencyProperty ImageStyleProperty =
			DependencyProperty.Register("ImageStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style LinkStyle
		{
			get { return (Style)GetValue(LinkStyleProperty); }
			set { SetValue(LinkStyleProperty, value); }
		}

		public static readonly DependencyProperty LinkStyleProperty =
			DependencyProperty.Register("LinkStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public static readonly DependencyProperty ParagraphStyleProperty =
	   DependencyProperty.Register("ParagraphStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));
		public Style ParagraphStyle
		{
			get { return (Style)GetValue(ParagraphStyleProperty); }
			set { SetValue(ParagraphStyleProperty, value); }
		}
		public static readonly DependencyProperty TextStyleProperty =
	   DependencyProperty.Register("TextStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));
		public Style TextStyle
		{
			get { return (Style)GetValue(TextStyleProperty); }
			set { SetValue(TextStyleProperty, value); }
		}


		public static readonly DependencyProperty HighlightedTextStyleProperty =
	   DependencyProperty.Register("HighlightedTextStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));
		public Style HighlightedTextStyle
		{
			get { return (Style)GetValue(HighlightedTextStyleProperty); }
			set { SetValue(HighlightedTextStyleProperty, value); }
		}

		public static readonly DependencyProperty TodoCheckBoxStyleProperty =
			DependencyProperty.Register("TodoCheckBoxStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style TodoCheckBoxStyle
		{
			get { return (Style)GetValue(TodoCheckBoxStyleProperty); }
			set { SetValue(TodoCheckBoxStyleProperty, value); }
		}

		public static readonly DependencyProperty TodoTextStyleProperty = DependencyProperty.Register("TodoTextStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style TodoTextStyle
		{
			get { return (Style)GetValue(TodoTextStyleProperty); }
			set { SetValue(TodoTextStyleProperty, value); }
		}


		public static readonly DependencyProperty ListStyleProperty = DependencyProperty.Register("ListStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style ListStyle
		{
			get { return (Style)GetValue(ListStyleProperty); }
			set { SetValue(ListStyleProperty, value); }
		}


		public static readonly DependencyProperty ListItemStyleProperty = DependencyProperty.Register("ListItemStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

		public Style ListItemStyle
		{
			get { return (Style)GetValue(ListItemStyleProperty); }
			set { SetValue(ListItemStyleProperty, value); }
		}

		public Note CurrentNote
		{
			get { return (Note)GetValue(CurrentNoteProperty); }
			set { SetValue(CurrentNoteProperty, value); }
		}

		public static readonly DependencyProperty CurrentNoteProperty = DependencyProperty.Register("CurrentNote", typeof(Note), typeof(Markdown), new PropertyMetadata(null));

		#endregion

		public XamlFormatter()
		{
			HyperlinkCommand = NavigationCommands.GoToPage;
		}

		internal static void EscapeHtml(StringContent inp, IAddChild target)
		{
			//var parts = inp.RetrieveParts();
			//for (var i = parts.Offset; i < parts.Offset + parts.Count; i++)
			//{
			//	var part = parts.Array[i];
			//}

			target.AddText(inp.ToString().TrimEnd());
		}

		public FlowDocument BlocksToXaml(Block block, CommonMarkSettings settings)
		{
			//_checkBoxNumber = 0;
			FlowDocument document = new FlowDocument();
			document.PagePadding = new Thickness(0);
			if (DocumentStyle != null)
			{
				document.Style = DocumentStyle;
			}

			BlocksToXamlInner(document, block, settings);

			return document;
		}

		internal static void PrintPosition(FrameworkContentElement writer, Block block)
		{
			writer.Tag = "DataSourcePos:" + block.SourcePosition.ToString(CultureInfo.InvariantCulture) + " - " + (block.SourcePosition + block.SourceLength).ToString(CultureInfo.InvariantCulture);
		}

		internal static void PrintPosition(FrameworkContentElement writer, Inline inline)
		{
			writer.Tag = "DataSourcePos:" + inline.SourcePosition.ToString(CultureInfo.InvariantCulture) + " - " + (inline.SourcePosition + inline.SourceLength).ToString(CultureInfo.InvariantCulture);
		}

		private void BlocksToXamlInner(FlowDocument parent, Block block, CommonMarkSettings settings)
		{
			var stack = new Stack<BlockStackEntry>();
			var inlineStack = new Stack<InlineStackEntry>();
			bool stackTight = false;
			bool tight = false;
			bool trackPositions = settings.TrackSourcePosition;
			int x;

			IAddChild blockParent = parent;

			while (block != null)
			{
				var visitChildren = false;
				IAddChild lastParent = null;

				switch (block.Tag)
				{
					case BlockTag.Document:
						stackTight = false;
						visitChildren = true;
						lastParent = parent;
						break;

					case BlockTag.Paragraph:
						if (tight)
						{
							InlinesToXaml(blockParent, block.InlineContent, settings, inlineStack);
						}
						else
						{
							Paragraph paragraph = new Paragraph();
							if (trackPositions)
								PrintPosition(paragraph, block);
							InlinesToXaml(paragraph, block.InlineContent, settings, inlineStack);
							blockParent.AddChild(paragraph);
						}
						break;
					case BlockTag.BlockQuote:
						Section blockquoteParagraph = new Section();
						if (trackPositions)
							PrintPosition(blockquoteParagraph, block);

						if (QuoteStyle != null)
							blockquoteParagraph.Style = QuoteStyle;

						blockParent.AddChild(blockquoteParagraph);
						lastParent = blockParent;
						blockParent = blockquoteParagraph;

						stackTight = true;
						visitChildren = true;
						break;

					case BlockTag.ListItem:
						ListItem listItem = new ListItem();
						if (ListItemStyle != null)
							listItem.Style = ListItemStyle;

						if (trackPositions)
							PrintPosition(listItem, block);

						blockParent.AddChild(listItem);
						lastParent = blockParent;
						blockParent = listItem;

						stackTight = tight;
						visitChildren = true;
						break;

					case BlockTag.List:
						List list = new List();
						if (ListStyle != null)
							list.Style = ListStyle;

						if (trackPositions)
							PrintPosition(list, block);

						var data = block.ListData;
						list.MarkerStyle = data.ListType == ListType.Bullet ? TextMarkerStyle.Disc : TextMarkerStyle.Decimal;

						// TODO: Check if first child starts with [ ] then it is a todo-list item
						// TODO: Set list.StartIndex if > 1

						blockParent.AddChild(list);
						lastParent = blockParent;
						blockParent = list;

						stackTight = data.IsTight;
						visitChildren = true;
						break;

					case BlockTag.AtxHeading:
					case BlockTag.SetextHeading:
						Paragraph headerParagraph = new Paragraph();
						if (trackPositions)
							PrintPosition(headerParagraph, block);

						InlinesToXaml(headerParagraph, block.InlineContent, settings, inlineStack);

						x = block.Heading.Level;

						switch (x)
						{
							case 1:
								headerParagraph.Style = Heading1Style;
								break;
							case 2:
								headerParagraph.Style = Heading2Style;
								break;
							case 3:
								headerParagraph.Style = Heading3Style;
								break;
							case 4:
								headerParagraph.Style = Heading4Style;
								break;
							default:
								headerParagraph.Style = Heading1Style;
								break;
						}

						blockParent.AddChild(headerParagraph);

						break;

					case BlockTag.IndentedCode:
					case BlockTag.FencedCode:
						Paragraph codeblockParagraph = new Paragraph();

						if (trackPositions)
							PrintPosition(codeblockParagraph, block);

						if (CodeBlockStyle != null)
							codeblockParagraph.Style = CodeBlockStyle;

						var info = block.FencedCodeData == null ? null : block.FencedCodeData.Info;
						if (info != null && info.Length > 0)
						{
							//x = info.IndexOf(' ');
							//if (x == -1)
							//	x = info.Length;

							//parent.WriteConstant(" class=\"language-");
							//EscapeHtml(new StringPart(info, 0, x), parent);
							//parent.Write('\"');
						}

						EscapeHtml(block.StringContent, codeblockParagraph);

						blockParent.AddChild(codeblockParagraph);
						break;

					case BlockTag.HtmlBlock:
						// cannot output source position for HTML blocks
						// block.StringContent.WriteTo(parent);

						//TODO: Unable to convert html to

						break;

					case BlockTag.ThematicBreak:
						var line = new Line() { X2 = 1, StrokeThickness = 1.0 };
						var container = new BlockUIContainer(line);
						if (trackPositions)
							PrintPosition(container, block);

						if (LineStyle != null)
							line.Style = LineStyle;

						blockParent.AddChild(container);
						break;
					case BlockTag.Table:
						WriteTable(block, blockParent, settings, inlineStack);
						break;
					case BlockTag.ReferenceDefinition:
						break;

					default:
						throw new CommonMarkException("Block type " + block.Tag + " is not supported.", block);
				}

				if (visitChildren)
				{
					stack.Push(new BlockStackEntry(lastParent, block.NextSibling, tight));

					tight = stackTight;
					block = block.FirstChild;
				}
				else if (block.NextSibling != null)
				{
					block = block.NextSibling;
				}
				else
				{
					block = null;
				}

				while (block == null && stack.Count > 0)
				{
					var entry = stack.Pop();

					blockParent = entry.Parent;
					tight = entry.IsTight;
					block = entry.Target;
				}
			}
		}

		/// <summary>
		/// Writes the inline list to the given parent as plain text (without any HTML tags).
		/// </summary>
		/// <seealso href="https://github.com/jgm/CommonMark/issues/145"/>
		private void InlinesToPlainText(IAddChild parent, Inline inline, Stack<InlineStackEntry> stack)
		{
			bool withinLink = false;
			bool stackWithinLink = false;
			string stackLiteral = null;
			var origStackCount = stack.Count;


			while (inline != null)
			{
				var visitChildren = false;

				switch (inline.Tag)
				{
					case InlineTag.String:
					case InlineTag.RawHtml:
						//EscapeHtml(inline.LiteralContent, parent);
						break;
					case InlineTag.Code:
						Span codeSpan = new Span(new Run(inline.LiteralContent));
						if (InlineCodeStyle != null)
							codeSpan.Style = InlineCodeStyle;

						parent.AddChild(codeSpan);

						break;

					case InlineTag.LineBreak:
					case InlineTag.SoftBreak:
						//parent.WriteLine();
						break;

					case InlineTag.Link:
						if (withinLink)
						{
							//parent.Write('[');
							stackLiteral = "]";
							visitChildren = true;
							stackWithinLink = true;
						}
						else
						{
							visitChildren = true;
							stackWithinLink = true;
							stackLiteral = string.Empty;
						}
						break;

					case InlineTag.Image:
						visitChildren = true;
						stackWithinLink = true;
						stackLiteral = string.Empty;
						break;

					case InlineTag.Strong:
					case InlineTag.Emphasis:
					case InlineTag.Strikethrough:
						stackLiteral = string.Empty;
						stackWithinLink = withinLink;
						visitChildren = true;
						break;

					case InlineTag.Placeholder:
						visitChildren = true;
						break;

					default:
						throw new CommonMarkException("Inline type " + inline.Tag + " is not supported.", inline);
				}

				if (visitChildren)
				{
					stack.Push(new InlineStackEntry(parent, inline.NextSibling, withinLink));

					withinLink = stackWithinLink;
					inline = inline.FirstChild;
				}
				else if (inline.NextSibling != null)
				{
					inline = inline.NextSibling;
				}
				else
				{
					inline = null;
				}

				while (inline == null && stack.Count > origStackCount)
				{
					var entry = stack.Pop();

					inline = entry.Target;
					withinLink = entry.IsWithinLink;
				}
			}
		}

		/// <summary>
		/// Writes the inline list to the given parent as HTML code.
		/// </summary>
		private void InlinesToXaml(IAddChild parent, Inline inline, CommonMarkSettings settings, Stack<InlineStackEntry> stack)
		{
			var uriResolver = settings.UriResolver;
			bool withinLink = false;
			bool stackWithinLink = false;
			bool trackPositions = settings.TrackSourcePosition;

			IAddChild blockParent = parent;
			if (blockParent is ListItem || blockParent is Section || blockParent is TableCell)
			{
				Paragraph p = new Paragraph();
				blockParent.AddChild(p);
				blockParent = p;
			}

			while (inline != null)
			{
				var visitChildren = false;
				IAddChild lastParent = null;

				switch (inline.Tag)
				{
					case InlineTag.String:
						//if (inline.LiteralContent.StartsWith("[ ]") || inline.LiteralContent.StartsWith("[x]"))
						//{
						//	CheckBox bt = new CheckBox
						//	{
						//		IsChecked = inline.LiteralContent.Contains("[x]"),
						//		Content = inline.LiteralContent.Substring(2),
						//		Tag = _checkBoxNumber
						//	};
						//	bt.CommandParameter = bt;
						//	bt.Command = CheckBoxCheckedCommand;
						//	bt.Style = TodoCheckBoxStyle;
						//	blockParent.AddChild(new BlockUIContainer(bt));
						//	_checkBoxNumber++;
						//}
						//else
						blockParent.AddText(inline.LiteralContent);
						break;
					case InlineTag.LineBreak:
						blockParent.AddChild(new LineBreak());
						break;

					case InlineTag.SoftBreak:
						if (settings.RenderSoftLineBreaksAsLineBreaks)
							blockParent.AddChild(new LineBreak());
						break;

					case InlineTag.Code:

						Span codeSpan = new Span(new Run(inline.LiteralContent));
						if (InlineCodeStyle != null)
							codeSpan.Style = InlineCodeStyle;

						blockParent.AddChild(codeSpan);

						break;

					case InlineTag.RawHtml:
						// cannot output source position for HTML blocks
						blockParent.AddText(inline.LiteralContent);
						break;

					case InlineTag.Link:
						if (withinLink)
						{
							//parent.Write('[');
							//stackLiteral = "]";
							stackWithinLink = true;
							visitChildren = true;
						}
						else
						{
							Hyperlink hyperlink = new Hyperlink();
							hyperlink.Command = HyperlinkCommand;

							if (LinkStyle != null)
								hyperlink.Style = LinkStyle;

							string url = inline.TargetUrl;
							if (uriResolver != null)
							{
								url = uriResolver(inline.TargetUrl);
							}

							hyperlink.CommandParameter = url;
							if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
								hyperlink.NavigateUri = new Uri(url);

							if (inline.LiteralContent.Length > 0)
								hyperlink.ToolTip = inline.LiteralContent;

							if (trackPositions)
								PrintPosition(hyperlink, inline);

							if (!(blockParent is Hyperlink))
								blockParent.AddChild(hyperlink);

							lastParent = blockParent;
							blockParent = hyperlink;

							visitChildren = true;
							stackWithinLink = true;
						}
						break;

					case InlineTag.Image:
						HandleImage(inline, parent);

						break;

					case InlineTag.Strong:
						Bold bold = new Bold();
						blockParent.AddChild(bold);
						lastParent = blockParent;
						blockParent = bold;

						if (trackPositions)
							PrintPosition(bold, inline);

						stackWithinLink = withinLink;
						visitChildren = true;
						break;

					case InlineTag.Emphasis:
						Italic italic = new Italic();
						blockParent.AddChild(italic);
						lastParent = blockParent;
						blockParent = italic;

						if (trackPositions)
							PrintPosition(italic, inline);

						visitChildren = true;
						stackWithinLink = withinLink;
						break;

					case InlineTag.Strikethrough:
						Span strikethroughSpan = new Span();
						strikethroughSpan.TextDecorations = TextDecorations.Strikethrough;

						blockParent.AddChild(strikethroughSpan);
						lastParent = blockParent;
						blockParent = strikethroughSpan;

						if (trackPositions)
							PrintPosition(strikethroughSpan, inline);

						visitChildren = true;
						stackWithinLink = withinLink;
						break;

					case InlineTag.Placeholder:
						// the slim formatter will treat placeholders like literals, without applying any further processing

						//if (blockParent is ListItem)
						//	blockParent.AddChild(new Paragraph(new Run("Placeholder")));
						//else
						//	blockParent.AddText("Placeholder");

						//visitChildren = false;

						//TODO: Handle todo-list items here
						break;

					default:
						throw new CommonMarkException("Inline type " + inline.Tag + " is not supported.", inline);
				}

				if (visitChildren)
				{
					stack.Push(new InlineStackEntry(lastParent, inline.NextSibling, withinLink));

					withinLink = stackWithinLink;
					inline = inline.FirstChild;
				}
				else if (inline.NextSibling != null)
				{
					inline = inline.NextSibling;
				}
				else
				{
					inline = null;
				}

				while (inline == null && stack.Count > 0)
				{
					var entry = stack.Pop();

					blockParent = entry.Parent;
					inline = entry.Target;
					withinLink = entry.IsWithinLink;
				}
			}
		}

		private void HandleImage(Inline inline, IAddChild parent)
		{
			NoteFile nf = CurrentNote.Files.FirstOrDefault(n => n.FileName == inline.TargetUrl);
			Paragraph result = parent as Paragraph;
			if (result == null)
			{
				result = new Paragraph();
				parent.AddChild(result);
			}

			if (nf != null && File.Exists(nf.FullName))
			{
				if (!string.IsNullOrWhiteSpace(inline.LiteralContent))
					result.ToolTip = inline.LiteralContent;

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

				if (ImageStyle != null)
					image.Style = ImageStyle;


				result.Inlines.Add(image);
			}
			else if (nf != null)
			{
				result.Inlines.Add(new Run("Missing file: " + nf.FullName));
			}
		}

		private void WriteTable(Block table, IAddChild parent, CommonMarkSettings settings, Stack<InlineStackEntry> stack)
		{
			if ((settings.AdditionalFeatures & CommonMarkAdditionalFeatures.GithubStyleTables) == 0)
			{
				throw new CommonMarkException("Table encountered in AST, but GithubStyleTables are not enabled");
			}

			var header = table.FirstChild;
			var firstRow = table.FirstChild.NextSibling;

			Table t = new Table();
			parent.AddChild(t);

			if (TableStyle != null)
				t.Style = TableStyle;

			var tableHeadRowGroup = new TableRowGroup();
			t.RowGroups.Add(tableHeadRowGroup);
			TableRow headRow = new TableRow();
			if (TableHeadStyle != null)
				headRow.Style = TableHeadStyle;
			tableHeadRowGroup.Rows.Add(headRow);

			var numHeadings = 0;

			var curHeaderCell = header.FirstChild;
			while (curHeaderCell != null)
			{
				var alignment = table.TableHeaderAlignments[numHeadings];

				numHeadings++;

				TableCell cell = new TableCell();
				if (TableCellStyle != null)
					cell.Style = TableCellStyle;

				InlinesToXaml(cell, curHeaderCell.InlineContent, settings, stack);

				if (alignment != TableHeaderAlignment.None)
				{
					switch (alignment)
					{
						case TableHeaderAlignment.Center: cell.TextAlignment = TextAlignment.Center; break;
						case TableHeaderAlignment.Left: cell.TextAlignment = TextAlignment.Left; break;
						case TableHeaderAlignment.Right: cell.TextAlignment = TextAlignment.Right; break;
						default: throw new CommonMarkException("Unexpected TableHeaderAlignment [" + alignment + "]");
					}
				}

				headRow.Cells.Add(cell);

				curHeaderCell = curHeaderCell.NextSibling;
			}

			var tableBodyRowGroup = new TableRowGroup();
			t.RowGroups.Add(tableBodyRowGroup);

			var curRow = firstRow;
			while (curRow != null)
			{
				TableRow row = new TableRow();
				if (TableRowStyle != null)
					row.Style = TableRowStyle;

				tableBodyRowGroup.Rows.Add(row);

				var curRowCell = curRow.FirstChild;

				var numCells = 0;

				while (curRowCell != null && numCells < numHeadings)
				{
					var alignment = table.TableHeaderAlignments[numCells];

					numCells++;

					TableCell cell = new TableCell();
					if (TableCellStyle != null)
						cell.Style = TableCellStyle;

					row.Cells.Add(cell);

					if (alignment != TableHeaderAlignment.None)
					{
						switch (alignment)
						{
							case TableHeaderAlignment.Center: cell.TextAlignment = TextAlignment.Center; break;
							case TableHeaderAlignment.Left: cell.TextAlignment = TextAlignment.Left; break;
							case TableHeaderAlignment.Right: cell.TextAlignment = TextAlignment.Right; break;
							default: throw new CommonMarkException("Unexpected TableHeaderAlignment [" + alignment + "]");
						}
					}

					InlinesToXaml(cell, curRowCell.InlineContent, settings, stack);

					curRowCell = curRowCell.NextSibling;
				}

				while (numCells < numHeadings)
				{
					numCells++;

					TableCell cell = new TableCell();
					if (TableCellStyle != null)
						cell.Style = TableCellStyle;

					row.Cells.Add(cell);
				}

				curRow = curRow.NextSibling;
			}
		}

		private struct BlockStackEntry
		{
			public readonly IAddChild Parent;
			public readonly Block Target;
			public readonly bool IsTight;
			public BlockStackEntry(IAddChild parent, Block target, bool isTight)
			{
				Parent = parent;
				Target = target;
				IsTight = isTight;
			}
		}
		private struct InlineStackEntry
		{
			public readonly IAddChild Parent;
			public readonly Inline Target;
			public readonly bool IsWithinLink;
			public InlineStackEntry(IAddChild parent, Inline target, bool isWithinLink)
			{
				Parent = parent;
				Target = target;
				IsWithinLink = isWithinLink;
			}
		}
	}
}
