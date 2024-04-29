// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsGroupDialog.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for RequirementsGroupDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.RequirementsGroup)]
    public partial class RequirementsGroupDialog : DXWindow, IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsGroupDialog"/> class.
        /// </summary>
        public RequirementsGroupDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsGroupDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public RequirementsGroupDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}