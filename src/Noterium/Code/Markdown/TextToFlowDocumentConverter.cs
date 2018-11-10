using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using Markdig;
using Markdig.Wpf;
using Noterium.Code.Helpers;
using Noterium.Core.Annotations;
using Noterium.Core.DataCarriers;

namespace Noterium.Code.Markdown
{
    public class TextToFlowDocumentConverter : DependencyObject, IMultiValueConverter
    {
        private string _text;

        public bool Pause { get; set; }
        public Note CurrentNote { get; set; }

        public FlowDocument CurrentDocument { get; set; }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Pause)
                return CurrentDocument;

            if (value == null || value.Length != 2)
                return null;

            if (!(value[0] is string))
                return null;

            _text = (string) value[0];
            var searchText = (string) value[1];

            if (string.IsNullOrWhiteSpace(_text))
                CurrentDocument = new FlowDocument();
            else
                CurrentDocument = GetNewDocument();

            CurrentDocument.FocusVisualStyle = null;
            CurrentDocument.PagePadding = new Thickness(20);
            CurrentDocument.PreviewKeyDown += CurrentDocument_PreviewKeyDown;

            return CurrentDocument;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static MarkdownPipeline BuildPipeline()
        {
            return new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .UseAutoIdentifiers()
                .Build();
        }

        public FlowDocument GetNewDocument()
        {
            var sw = new Stopwatch();
            sw.Start();
            var text = NoteMathHelper.ReplaceMathTokens(_text);

            return ToFlowDocument(text, BuildPipeline());
        }

        public static FlowDocument ToFlowDocument([NotNull] string markdown, MarkdownPipeline pipeline = null)
        {
            if (markdown == null) throw new ArgumentNullException(nameof(markdown));
            pipeline = pipeline ?? new MarkdownPipelineBuilder().Build();

            // We override the renderer with our own writer
            var result = new FlowDocument();
            var renderer = new WpfRenderer(result);
            pipeline.Setup(renderer);

            var document = Markdig.Markdown.Parse(markdown, pipeline);
            renderer.Render(document);

            return result;
        }


        private void CurrentDocument_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftCtrl && e.Key == Key.F) e.Handled = true;
        }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object[] ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}