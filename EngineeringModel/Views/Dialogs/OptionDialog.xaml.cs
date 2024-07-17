// ------------------------------------------------------------------------------------------------
// <copyright file="OptionDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for OptionDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.Option)]
    public partial class OptionDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialog"/> class.
        /// </summary>
        public OptionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public OptionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
