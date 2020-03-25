// ------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views.Dialogs
{
    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for DomainFileStoreDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.DomainFileStore)]
    public partial class DomainFileStoreDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreDialog"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by MEF to instation the view. The view is instantiated to enable navigation using the <see cref="IThingDialogNavigationService"/>
        /// </remarks>
        public DomainFileStoreDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public DomainFileStoreDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
