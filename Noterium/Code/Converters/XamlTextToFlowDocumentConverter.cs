using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Noterium.Code.Converters
{
	public class XamlTextToFlowDocumentConverter : DependencyObject, IValueConverter
	{
		public FlowDocument GetNewDocument(string xamlString)
	    {
			StringReader stringReader = new StringReader(xamlString);
			System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
			return XamlReader.Load(xmlReader) as FlowDocument;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			if (!(value is string))
				return null;

			string text = (string)value;

			if (string.IsNullOrWhiteSpace(text))
				return new FlowDocument();

			return GetNewDocument(text);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
