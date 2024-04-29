// ------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for IterationSetupDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.IterationSetup)]
    public partial class IterationSetupDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupDialog"/> class.
        /// </summary>
        public IterationSetupDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public IterationSetupDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
