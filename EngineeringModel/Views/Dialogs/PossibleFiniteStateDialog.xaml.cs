// ------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for PossibleFiniteStateDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.PossibleFiniteState)]
    public partial class PossibleFiniteStateDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateDialog"/> class.
        /// </summary>
        public PossibleFiniteStateDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public PossibleFiniteStateDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
