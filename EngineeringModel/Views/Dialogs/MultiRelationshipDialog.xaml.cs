// ------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2022 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for MultiRelationshipDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.MultiRelationship)]
    public partial class MultiRelationshipDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipDialog"/> class.
        /// </summary>
        public MultiRelationshipDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public MultiRelationshipDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
