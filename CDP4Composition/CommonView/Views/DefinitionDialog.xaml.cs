// -------------------------------------------------------------------------------------------------
// <copyright file="DefinitionDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for DefinitionDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.Definition)]
    public partial class DefinitionDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionDialog"/> class.
        /// </summary>
        public DefinitionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public DefinitionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}