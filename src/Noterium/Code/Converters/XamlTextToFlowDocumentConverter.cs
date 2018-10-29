using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace Noterium.Code.Converters
{
    public class XamlTextToFlowDocumentConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!(value is string))
                return null;

            var text = (string) value;

            if (string.IsNullOrWhiteSpace(text))
                return new FlowDocument();

            return GetNewDocument(text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public FlowDocument GetNewDocument(string xamlString)
        {
            var stringReader = new StringReader(xamlString);
            var xmlReader = XmlReader.Create(stringReader);
            return XamlReader.Load(xmlReader) as FlowDocument;
        }
    }
}