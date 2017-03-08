using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Noterium.Code.AttachedProperties
{
    public static class RichTextboxAssistant
    {
        public static readonly DependencyProperty BoundDocument =
           DependencyProperty.RegisterAttached("BoundDocument", typeof(FlowDocument), typeof(RichTextboxAssistant),
           new FrameworkPropertyMetadata(null,
               FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
               OnBoundDocumentChanged)
               );

        private static void OnBoundDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox box = d as RichTextBox;

            if (box == null)
                return;

            RemoveEventHandler(box);

			FlowDocument newXaml = GetBoundDocument(d);

	        box.Document = newXaml;
            //box.Document.Blocks.Clear();

			//if (!string.IsNullOrEmpty(newXAML))
			//{
			//	using (MemoryStream xamlMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(newXAML)))
			//	{
			//		ParserContext parser = new ParserContext();
			//		parser.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			//		parser.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
			//		FlowDocument doc = new FlowDocument();
			//		Section section = XamlReader.Load(xamlMemoryStream, parser) as Section;

			//		box.Document.Blocks.Add(section);

			//	}
			//}

            AttachEventHandler(box);

        }

        private static void RemoveEventHandler(RichTextBox box)
        {
            Binding binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null)
            {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {

                    box.LostFocus -= HandleLostFocus;
                }
                else
                {
                    box.TextChanged -= HandleTextChanged;
                }
            }
        }

        private static void AttachEventHandler(RichTextBox box)
        {
            Binding binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null)
            {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {

                    box.LostFocus += HandleLostFocus;
                }
                else
                {
                    box.TextChanged += HandleTextChanged;
                }
            }
        }

        private static void HandleLostFocus(object sender, RoutedEventArgs e)
        {
	        RichTextBox box = sender as RichTextBox;

			//TextRange tr = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);

			//using (MemoryStream ms = new MemoryStream())
			//{
			//	tr.Save(ms, DataFormats.Xaml);
			//	string xamlText = ASCIIEncoding.Default.GetString(ms.ToArray());
			//	SetBoundDocument(box, xamlText);
			//}
	        if (box != null) 
				SetBoundDocument(box, box.Document);
        }

	    private static void HandleTextChanged(object sender, RoutedEventArgs e)
        {
	        // TODO: TextChanged is currently not working!
            RichTextBox box = sender as RichTextBox;

			//TextRange tr = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);

			//using (MemoryStream ms = new MemoryStream())
			//{
			//	tr.Save(ms, DataFormats.Xaml);
			//	string xamlText = ASCIIEncoding.Default.GetString(ms.ToArray());
			//	SetBoundDocument(box, box.Document);
			//}
	        if (box != null) 
				SetBoundDocument(box, box.Document);
        }

	    public static FlowDocument GetBoundDocument(DependencyObject dp)
        {
			var fd = dp.GetValue(BoundDocument) as FlowDocument;
		    return fd ?? new FlowDocument();
        }

        public static void SetBoundDocument(DependencyObject dp, FlowDocument value)
        {
			//dp.SetValue(BoundDocument, value);
        }
    }

 
}