﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Noterium.Code.AttachedProperties
{
    public class ScrollHelper
    {
        public static readonly DependencyProperty ScrollSpeedProperty =
            DependencyProperty.RegisterAttached(
                "ScrollSpeed",
                typeof(double),
                typeof(ScrollHelper),
                new FrameworkPropertyMetadata(
                    1.0,
                    FrameworkPropertyMetadataOptions.Inherits & FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnScrollSpeedChanged));

        public static double GetScrollSpeed(DependencyObject obj)
        {
            return (double) obj.GetValue(ScrollSpeedProperty);
        }

        public static void SetScrollSpeed(DependencyObject obj, double value)
        {
            obj.SetValue(ScrollSpeedProperty, value);
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer) return o;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                    continue;
                return result;
            }

            return null;
        }

        private static void OnScrollSpeedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var host = o as UIElement;
            host.PreviewMouseWheel += OnPreviewMouseWheelScrolled;
        }

        private static void OnPreviewMouseWheelScrolled(object sender, MouseWheelEventArgs e)
        {
            var scrollHost = sender as DependencyObject;

            var scrollSpeed = (double) scrollHost.GetValue(ScrollSpeedProperty);

            var scrollViewer = GetScrollViewer(scrollHost) as ScrollViewer;

            if (scrollViewer != null)
            {
                var offset = scrollViewer.VerticalOffset - e.Delta * scrollSpeed / 6;
                if (offset < 0)
                    scrollViewer.ScrollToVerticalOffset(0);
                else if (offset > scrollViewer.ExtentHeight)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
                else
                    scrollViewer.ScrollToVerticalOffset(offset);

                e.Handled = true;
            }
            else
            {
                throw new NotSupportedException("ScrollSpeed Attached Property is not attached to an element containing a ScrollViewer.");
            }
        }
    }
}