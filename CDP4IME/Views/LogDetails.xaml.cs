// -------------------------------------------------------------------------------------------------
// <copyright file="LogDetails.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for ErrorDetails.xaml
    /// </summary>
    [DialogViewExport("LogDetails", "The dialog detailing a log message")]
    public partial class LogDetails : DXWindow, IDialogView
    {
        public LogDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogDetails"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public LogDetails(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}