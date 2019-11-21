// -------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Services;

    using NLog;

    /// <summary>
    /// Interaction logic for MeasurementScalesBrowser
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class MeasurementScalesBrowser : IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScalesBrowser"/> class.
        /// </summary>
        public MeasurementScalesBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScalesBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public MeasurementScalesBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                FilterStringService.FilterString.AddGridControl(this.MeasurementScalesGridControl);
            }
        }
    }
}
