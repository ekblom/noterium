using System;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;
using Noterium.Core.DataCarriers;

namespace Noterium.Controls.Behaviors
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty GiveMeTheTextProperty =
            DependencyProperty.Register("GiveMeTheText", typeof (string), typeof (AvalonEditBehaviour),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));
        public static readonly DependencyProperty EditedNoteProperty =
            DependencyProperty.Register("EditedNote", typeof (Note), typeof (AvalonEditBehaviour),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, PropertyChangedCallback));

        public string GiveMeTheText
        {
            get { return (string) GetValue(GiveMeTheTextProperty); }
            set { SetValue(GiveMeTheTextProperty, value); }
        }
        public Note EditedNote
        {
            get { return (Note) GetValue(EditedNoteProperty); }
            set { SetValue(EditedNoteProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            if (textEditor?.Document != null)
            {
                GiveMeTheText = textEditor.Document.Text;
            }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehaviour;
            if (behavior?.AssociatedObject != null && dependencyPropertyChangedEventArgs.Property == GiveMeTheTextProperty)
            {
                var editor = behavior.AssociatedObject;
                if (editor.Document != null)
                {
                    string text = dependencyPropertyChangedEventArgs.NewValue?.ToString() ?? string.Empty;

                    var caretOffset = editor.CaretOffset;
                    editor.Document.Text = text;
                    if (editor.Document.Text.Length < caretOffset)
                        caretOffset = 0;

                    editor.CaretOffset = caretOffset;
                }
            }
        }
    }
}