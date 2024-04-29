// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SizeObserver.cs" company="Starion Group S.A.">
// This is a Code Snippet found here: https://gist.github.com/jrgcubano/54ecbc61cbfaaa83b4dd14de03adf296
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.Views.Widget
{
    using System.Windows;

    /// <summary>
    /// ActualHeight and ActualWidth properties are readonly, so you cannot bind to them in XAML.
    /// However, we do want to read their values in some cases.
    /// The SizeObserver is a wrapper that allows you to bind to those readonly properties
    /// When <see cref="ObserveProperty"/> is set to true, the <see cref="OnObserveChanged"/> PropertyChangedCallback
    /// methods adds Delegate to the FrameworkElement's SizeChanged event that sets <see cref="ObservedWidthProperty"/> and <see cref="ObservedHeightProperty"/>.
    /// These properties can have bindings in XAML and therefore have bindings to the ViewModel.
    /// </summary>
    public static class SizeObserver
    {
        /// <summary>
        /// <see cref="DependencyProperty"/> that can be set in XAML to indicate that the SizeObserver is active
        /// </summary>
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(SizeObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> to be used to read the ActualWidth
        /// </summary>
        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(SizeObserver));

        /// <summary>
        /// <see cref="DependencyProperty"/> to be used to read the ActualHeight
        /// </summary>
        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(SizeObserver));

        /// <summary>
        /// The <see cref="ObserveProperty"/>'s get implementation
        /// </summary>
        /// <param name="frameworkElement">The <see cref="FrameworkElement"/></param>
        /// <returns>True is Observe is active, otherwise false</returns>
        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        /// <summary>
        /// The <see cref="ObserveProperty"/>'s set implementation
        /// </summary>
        /// <param name="frameworkElement">The <see cref="FrameworkElement"/></param>
        /// <param name="observe">The value to be set, true or false</param>
        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        /// <summary>
        /// The <see cref="ObservedWidthProperty"/>'s get implementation
        /// </summary>
        /// <param name="frameworkElement">The <see cref="FrameworkElement"/></param>
        /// <returns>Double value indicating the ActualWidth of <param name="frameworkElement" /></returns>
        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        /// <summary>
        /// The <see cref="ObservedWidthProperty"/>'s set implementation
        /// </summary>
        /// <param name="frameworkElement">The <see cref="FrameworkElement"/></param>
        /// <param name="observedWidth">The Width to be set</param>
        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        /// <summary>
        /// The <see cref="ObservedHeightProperty"/>'s get implementation
        /// </summary>
        /// <param name="frameworkElement">The <see cref="FrameworkElement"/></param>
        /// <returns>Double value indicating the ActualHeight of <param name="frameworkElement" /></returns>
        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        /// <summary>
        /// The <see cref="ObservedHeightProperty"/>'s set implementation
        /// </summary>
        /// <param name="frameworkElement">The <see cref="FrameworkElement"/></param>
        /// <param name="observedHeight">The Height to be set</param>
        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        /// <summary>
        /// PropertyChangedCallback that sets the <see cref="ObservedWidthProperty"/> and <see cref="ObservedHeightProperty"/> values
        /// and adds/removes an event handler to the <see cref="DependencyObject"/>'s SizeChanged event 
        /// </summary>
        /// <param name="dependencyObject">The <see cref="DependencyObject"/></param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        /// <summary>
        /// Event handler that calls a method that sets the <see cref="ObservedWidthProperty"/> and <see cref="ObservedHeightProperty"/> values
        /// </summary>
        /// <param name="sender">The <see cref="FrameworkElement"/> as an <see cref="object"/></param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/></param>
        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        /// <summary>
        /// Sets the <see cref="ObservedWidthProperty"/> and <see cref="ObservedHeightProperty"/> values for a <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="frameworkElement">The <see cref="FrameworkElement"/></param>
        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);
        }
    }
}