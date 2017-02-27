using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using CommonMark;
using Noterium.Core.DataCarriers;

namespace Noterium.Code.Markdown
{
	public class TextToFlowDocumentConverter : DependencyObject, IMultiValueConverter
	{
		public TextToFlowDocumentConverter()
		{
			_settings = CommonMarkSettings.Default.Clone();
			_settings.OutputFormat = OutputFormat.CustomDelegate;
			_settings.AdditionalFeatures = CommonMarkAdditionalFeatures.All;
		}
		public XamlFormatter Markdown
		{
			get { return (XamlFormatter)GetValue(MarkdownProperty); }
			set { SetValue(MarkdownProperty, value); }
		}

	    public bool Pause { get; set; }

	    // Using a DependencyProperty as the backing store for Markdown.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register("XamlFormatter", typeof(XamlFormatter), typeof(TextToFlowDocumentConverter), new PropertyMetadata(null));

		/// <summary>
		/// Converts a value. 
		/// </summary>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
		{
		    if (Pause)
		        return CurrentDocument;

			if (value == null || value.Length != 3)
				return null;

			if (!(value[0] is string && value[1] is Note && value[2] is string))
				return null;

			_text = (string)value[0];
			var searchText = (string)value[2];
			var note = (Note)value[1];

			var engine = Markdown ?? _markdown.Value;
			engine.CurrentNote = note;

            if (string.IsNullOrWhiteSpace(_text))
                CurrentDocument = new FlowDocument();
			else
            {
	            CurrentDocument = GetNewDocument();
            }

		    CurrentDocument.FocusVisualStyle = null;
            CurrentDocument.PagePadding = new Thickness(20);
            CurrentDocument.PreviewKeyDown += CurrentDocument_PreviewKeyDown;

			return CurrentDocument;
		}

	    public FlowDocument GetNewDocument()
	    {
			using (var reader = new StringReader(_text))
			{
				var document = CommonMarkConverter.ProcessStage1(reader, _settings);
				CommonMarkConverter.ProcessStage2(document, _settings);

				var engine = Markdown ?? _markdown.Value;
				return engine.BlocksToXaml(document, _settings);
			}
		}

        private void CurrentDocument_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftCtrl && e.Key == Key.F)
            {
                e.Handled = true;
            }
        }

        public FlowDocument CurrentDocument { get; set; }

	    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Converts a value. 
		/// </summary>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public object[] ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private readonly Lazy<XamlFormatter> _markdown = new Lazy<XamlFormatter>(() => new XamlFormatter());
	    private string _text;
		private readonly CommonMarkSettings _settings;
	}
}
