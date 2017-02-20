using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Noterium.Code.ListViewDragDrop;

namespace Noterium.Code.Controls
{
    public class DragDropScrollViewer : ScrollViewer
    {
        protected override void OnPreviewQueryContinueDrag(QueryContinueDragEventArgs args)
        {
            base.OnPreviewQueryContinueDrag(args);

            if (args.Action == DragAction.Cancel || args.Action == DragAction.Drop)
            {
                CancelDrag();
            }
            else if (args.Action == DragAction.Continue)
            {
                Point p = MouseUtilities.GetMousePosition(this);
                if ((p.Y < s_dragMargin) || (p.Y > RenderSize.Height - s_dragMargin))
                {
                    if (_dragScrollTimer == null)
                    {
                        _dragVelocity = s_dragInitialVelocity;
                        _dragScrollTimer = new DispatcherTimer();
                        _dragScrollTimer.Tick += TickDragScroll;
                        _dragScrollTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)s_dragInterval);
                        _dragScrollTimer.Start();
                    }
                }
            }
        }

        private void TickDragScroll(object sender, EventArgs e)
        {
            bool isDone = true;

            if (IsLoaded)
            {
                Rect bounds = new Rect(RenderSize);
                Point p = MouseUtilities.GetMousePosition(this);
                if (bounds.Contains(p))
                {
                    if (p.Y < s_dragMargin)
                    {
                        DragScroll(DragDirection.Up);
                        isDone = false;
                    }
                    else if (p.Y > RenderSize.Height - s_dragMargin)
                    {
                        DragScroll(DragDirection.Down);
                        isDone = false;
                    }
                }
            }

            if (isDone)
            {
                CancelDrag();
            }
        }

        private void CancelDrag()
        {
            if (_dragScrollTimer != null)
            {
                _dragScrollTimer.Tick -= TickDragScroll;
                _dragScrollTimer.Stop();
                _dragScrollTimer = null;
            }
        }

        private enum DragDirection
        {
            Down,
            Up
        };

        private void DragScroll(DragDirection direction)
        {
            bool isUp = (direction == DragDirection.Up);
            double offset = Math.Max(0.0, VerticalOffset + (isUp ? -(_dragVelocity * s_dragInterval) : (_dragVelocity * s_dragInterval)));
            ScrollToVerticalOffset(offset);
            _dragVelocity = Math.Min(s_dragMaxVelocity, _dragVelocity + (s_dragAcceleration * s_dragInterval));
        }

        private static readonly double s_dragInterval = 10; // milliseconds
        private static readonly double s_dragAcceleration = 0.0005; // pixels per millisecond^2
        private static readonly double s_dragMaxVelocity = 2.0; // pixels per millisecond
        private static readonly double s_dragInitialVelocity = 0.05; // pixels per millisecond
        private static double s_dragMargin = 40.0;
        private DispatcherTimer _dragScrollTimer;
        private double _dragVelocity;
    }
}