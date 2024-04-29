// ------------------------------------------------------------------------------------------------
// <copyright file="OrganizationDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for OrganizationDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.Organization)]
    public partial class OrganizationDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationDialog"/> class.
        /// </summary>
        public OrganizationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public OrganizationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
