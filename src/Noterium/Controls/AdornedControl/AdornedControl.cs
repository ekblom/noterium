using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Noterium.Controls.AdornedControl
{
    public class AdornedControl : ContentControl
    {
        /// <summary>
        ///     Dependency properties.
        /// </summary>
        public static readonly DependencyProperty IsAdornerVisibleProperty =
            DependencyProperty.Register("IsAdornerVisible", typeof(bool), typeof(AdornedControl),
                new FrameworkPropertyMetadata(IsAdornerVisiblePropertyChanged));

        public static readonly DependencyProperty AdornerContentProperty =
            DependencyProperty.Register("AdornerContent", typeof(FrameworkElement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerContentPropertyChanged));

        public static readonly DependencyProperty HorizontalAdornerPlacementProperty =
            DependencyProperty.Register("HorizontalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        public static readonly DependencyProperty VerticalAdornerPlacementProperty =
            DependencyProperty.Register("VerticalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        public static readonly DependencyProperty AdornerOffsetXProperty =
            DependencyProperty.Register("AdornerOffsetX", typeof(double), typeof(AdornedControl));

        public static readonly DependencyProperty AdornerOffsetYProperty =
            DependencyProperty.Register("AdornerOffsetY", typeof(double), typeof(AdornedControl));

        /// <summary>
        ///     Commands.
        /// </summary>
        public static readonly RoutedCommand ShowAdornerCommand = new RoutedCommand("ShowAdorner", typeof(AdornedControl));

        public static readonly RoutedCommand HideAdornerCommand = new RoutedCommand("HideAdorner", typeof(AdornedControl));

        public AdornedControl()
        {
            DataContextChanged += AdornedControlDataContextChanged;
        }

        /// <summary>
        ///     Shows or hides the adorner.
        ///     Set to 'true' to show the adorner or 'false' to hide the adorner.
        /// </summary>
        public bool IsAdornerVisible
        {
            get => (bool) GetValue(IsAdornerVisibleProperty);
            set => SetValue(IsAdornerVisibleProperty, value);
        }

        /// <summary>
        ///     Used in XAML to define the UI content of the adorner.
        /// </summary>
        public FrameworkElement AdornerContent
        {
            get => (FrameworkElement) GetValue(AdornerContentProperty);
            set => SetValue(AdornerContentProperty, value);
        }

        /// <summary>
        ///     Specifies the horizontal placement of the adorner relative to the adorned control.
        /// </summary>
        public AdornerPlacement HorizontalAdornerPlacement
        {
            get => (AdornerPlacement) GetValue(HorizontalAdornerPlacementProperty);
            set => SetValue(HorizontalAdornerPlacementProperty, value);
        }

        /// <summary>
        ///     Specifies the vertical placement of the adorner relative to the adorned control.
        /// </summary>
        public AdornerPlacement VerticalAdornerPlacement
        {
            get => (AdornerPlacement) GetValue(VerticalAdornerPlacementProperty);
            set => SetValue(VerticalAdornerPlacementProperty, value);
        }

        /// <summary>
        ///     X offset of the adorner.
        /// </summary>
        public double AdornerOffsetX
        {
            get => (double) GetValue(AdornerOffsetXProperty);
            set => SetValue(AdornerOffsetXProperty, value);
        }

        /// <summary>
        ///     Y offset of the adorner.
        /// </summary>
        public double AdornerOffsetY
        {
            get => (double) GetValue(AdornerOffsetYProperty);
            set => SetValue(AdornerOffsetYProperty, value);
        }

        /// <summary>
        ///     Event raised when the DataContext of the adorned control changes.
        /// </summary>
        private void AdornedControlDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateAdornerDataContext();
        }

        /// <summary>
        ///     Update the DataContext of the adorner from the adorned control.
        /// </summary>
        private void UpdateAdornerDataContext()
        {
            if (AdornerContent != null) AdornerContent.DataContext = DataContext;
        }

        /// <summary>
        ///     Show the adorner.
        /// </summary>
        public void ShowAdorner()
        {
            IsAdornerVisible = true;
        }

        /// <summary>
        ///     Hide the adorner.
        /// </summary>
        public void HideAdorner()
        {
            IsAdornerVisible = false;
        }

        #region Private Data Members

        /// <summary>
        ///     Command bindings.
        /// </summary>
        private static readonly CommandBinding ShowAdornerCommandBinding = new CommandBinding(ShowAdornerCommand, ShowAdornerCommandExecuted);

        private static readonly CommandBinding HideAdornerCommandBinding = new CommandBinding(HideAdornerCommand, HideAdornerCommandExecuted);

        /// <summary>
        ///     Caches the adorner layer.
        /// </summary>
        private AdornerLayer _adornerLayer;

        /// <summary>
        ///     The actual adorner create to contain our 'adorner UI content'.
        /// </summary>
        private FrameworkElementAdorner _adorner;

        #endregion

        #region Private/Internal Functions

        /// <summary>
        ///     Static constructor to register command bindings.
        /// </summary>
        static AdornedControl()
        {
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), ShowAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), HideAdornerCommandBinding);
        }

        /// <summary>
        ///     Event raised when the Show command is executed.
        /// </summary>
        private static void ShowAdornerCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var c = (AdornedControl) target;
            c.ShowAdorner();
        }

        /// <summary>
        ///     Event raised when the Hide command is executed.
        /// </summary>
        private static void HideAdornerCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var c = (AdornedControl) target;
            c.HideAdorner();
        }

        /// <summary>
        ///     Event raised when the value of IsAdornerVisible has changed.
        /// </summary>
        private static void IsAdornerVisiblePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (AdornedControl) o;
            c.ShowOrHideAdornerInternal();
        }

        /// <summary>
        ///     Event raised when the value of AdornerContent has changed.
        /// </summary>
        private static void AdornerContentPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (AdornedControl) o;
            c.ShowOrHideAdornerInternal();
        }

        /// <summary>
        ///     Internal method to show or hide the adorner based on the value of IsAdornerVisible.
        /// </summary>
        private void ShowOrHideAdornerInternal()
        {
            if (IsAdornerVisible)
                ShowAdornerInternal();
            else
                HideAdornerInternal();
        }

        /// <summary>
        ///     Internal method to show the adorner.
        /// </summary>
        private void ShowAdornerInternal()
        {
            if (_adorner != null) return;

            if (AdornerContent != null)
            {
                if (_adornerLayer == null) _adornerLayer = AdornerLayer.GetAdornerLayer(this);

                if (_adornerLayer != null)
                {
                    _adorner = new FrameworkElementAdorner(AdornerContent, this, HorizontalAdornerPlacement, VerticalAdornerPlacement,
                        AdornerOffsetX, AdornerOffsetY);
                    _adornerLayer.Add(_adorner);

                    UpdateAdornerDataContext();
                }
            }
        }

        /// <summary>
        ///     Internal method to hide the adorner.
        /// </summary>
        private void HideAdornerInternal()
        {
            if (_adornerLayer == null || _adorner == null) return;

            _adornerLayer.Remove(_adorner);
            _adorner.DisconnectChild();

            _adorner = null;
            _adornerLayer = null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ShowOrHideAdornerInternal();
        }

        #endregion
    }
}