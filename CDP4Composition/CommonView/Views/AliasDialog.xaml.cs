// ------------------------------------------------------------------------------------------------
// <copyright file="AliasDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for AliasDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.Alias)]
    public partial class AliasDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasDialog"/> class.
        /// </summary>
        public AliasDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public AliasDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
