namespace CDP4Requirements.Views.Dialogs
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for RequirementsContainerParameterValueDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.RequirementsContainerParameterValue)]
    public partial class RequirementsContainerParameterValueDialog : DXWindow, IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsContainerParameterValueDialog"/> class.
        /// </summary>
        public RequirementsContainerParameterValueDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsContainerParameterValueDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public RequirementsContainerParameterValueDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
