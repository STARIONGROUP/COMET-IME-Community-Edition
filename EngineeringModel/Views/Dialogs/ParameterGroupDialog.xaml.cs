// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for ParameterGroupDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.ParameterGroup)]
    public partial class ParameterGroupDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupDialog"/> class.
        /// </summary>
        public ParameterGroupDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public ParameterGroupDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
