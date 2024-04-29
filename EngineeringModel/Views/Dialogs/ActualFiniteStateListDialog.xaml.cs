// ------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateListDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for ActualFiniteStateListDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.ActualFiniteStateList)]
    public partial class ActualFiniteStateListDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateListDialog"/> class.
        /// </summary>
        public ActualFiniteStateListDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateListDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public ActualFiniteStateListDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
