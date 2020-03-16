// ------------------------------------------------------------------------------------------------
// <copyright file="DiagramDialog.xaml.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for CategoryDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.DiagramCanvas)]
    public partial class DiagramDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramDialog"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by MEF to instation the view. The view is instantiated to enable navigation using the <see cref="IThingDialogNavigationService"/>
        /// </remarks>
        public DiagramDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public DiagramDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
