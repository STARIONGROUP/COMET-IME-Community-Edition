// ------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for PossibleFiniteStateListDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.PossibleFiniteStateList)]
    public partial class PossibleFiniteStateListDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialog"/> class.
        /// </summary>
        public PossibleFiniteStateListDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public PossibleFiniteStateListDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
