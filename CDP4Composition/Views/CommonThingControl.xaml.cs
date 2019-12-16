// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonThingControl.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using DevExpress.Xpf.Grid;
    using System.Windows;
    using NLog;

    /// <summary>
    /// Interaction logic for CommonThingControl
    /// </summary>
    public partial class CommonThingControl
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="GridView"/> setter method.
        /// </summary>
        private readonly static DependencyProperty GridViewProperty = DependencyProperty.Register("GridView", typeof(GridDataViewBase), typeof(CommonThingControl));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="IsFavoriteToggleEnabled"/> setter method.
        /// </summary>
        private readonly static DependencyProperty IsFavoriteToggleEnabledProperty = DependencyProperty.Register("IsFavoriteToggleEnabled", typeof(bool), typeof(CommonThingControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonThingControl"/> class.
        /// </summary>
        public CommonThingControl()
        {
            this.InitializeComponent();
            this.IsFavoriteToggleEnabled = false;
        }

        /// <summary>
        /// The <see cref="GridView"/> this <see cref="CommonThingControl"/> is associated with
        /// </summary>
        public GridDataViewBase GridView
        {
            get { return this.GetValue(GridViewProperty) as GridDataViewBase; }
            set { this.SetValue(GridViewProperty, value); }
        }

        /// <summary>
        /// The boolean that enables or disables the visibility of the favorites toggle buttons.
        /// </summary>
        public bool IsFavoriteToggleEnabled
        {
            get { return this.GetValue(IsFavoriteToggleEnabledProperty) is bool ? (bool) this.GetValue(IsFavoriteToggleEnabledProperty) : false; }
            set { this.SetValue(IsFavoriteToggleEnabledProperty, value); }
        }

        /// <summary>
        /// The ItemClick Handler
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.GridView == null)
            {
                logger.Debug("The GridView has not been set from the container view");
                return;
            }

            if (this.GridView.ActualShowSearchPanel)
            {
                this.GridView.HideSearchPanel();
            }
            else
            {
                this.GridView.ShowSearchPanel(true);
            }
        }
    }
}