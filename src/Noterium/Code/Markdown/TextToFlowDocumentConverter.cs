using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using CommonMark;
using Noterium.Code.Helpers;
using Noterium.Core.DataCarriers;

namespace Noterium.Code.Markdown
{
    public class TextToFlowDocumentConverter : DependencyObject, IMultiValueConverter
    {
        // Using a DependencyProperty as the backing store for Markdown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XamlFormatterProperty = DependencyProperty.Register("XamlFormatter", typeof(XamlFormatter), typeof(TextToFlowDocumentConverter), new PropertyMetadata(null));

        private readonly Lazy<XamlFormatter> _markdown = new Lazy<XamlFormatter>(() => new XamlFormatter());
        private readonly CommonMarkSettings _settings;
        private string _text;

        public TextToFlowDocumentConverter()
        {
            _settings = CommonMarkSettings.Default.Clone();
            _settings.OutputFormat = OutputFormat.CustomDelegate;
            _settings.AdditionalFeatures = CommonMarkAdditionalFeatures.StrikethroughTilde;
        }

        public XamlFormatter XamlFormatter
        {
            get => (XamlFormatter) GetValue(XamlFormatterProperty);
            set => SetValue(XamlFormatterProperty, value);
        }

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

            var engine = XamlFormatter ?? _markdown.Value;
            engine.CurrentNote = CurrentNote;

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

        public FlowDocument GetNewDocument()
        {
            var sw = new Stopwatch();
            sw.Start();
            var text = NoteMathHelper.ReplaceMathTokens(_text);
            using (var reader = new StringReader(text))
            {
                var document = CommonMarkConverter.ProcessStage1(reader, _settings);
                CommonMarkConverter.ProcessStage2(document, _settings);
                var engine = XamlFormatter ?? _markdown.Value;
                var doc = engine.BlocksToXaml(document, _settings);

                return doc;
            }
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