// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypesBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition;

    using NLog;

    /// <summary>
    /// Interaction logic for Parameter types browser
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class ParameterTypesBrowser : IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypesBrowser"/> class.
        /// </summary>
        public ParameterTypesBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypesBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ParameterTypesBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                this.View.ShowSearchPanel(true);
            }
        }
    }
}
