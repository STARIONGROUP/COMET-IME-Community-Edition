// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelection.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;
    
    /// <summary>
    /// Interaction logic for DataSourceSelection
    /// </summary>
    [DialogViewExport("DataSourceSelection", "The Dialog to log in into a data-source")]
    public partial class DataSourceSelection : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceSelection"/> class.
        /// </summary>
        public DataSourceSelection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceSelection"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public DataSourceSelection(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}