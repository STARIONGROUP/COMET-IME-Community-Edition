namespace CDP4Requirements.Views.Dialogs
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for SimpleParameterValueDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.SimpleParameterValue)]
    public partial class SimpleParameterValueDialog : DXWindow, IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterValueDialog"/> class.
        /// </summary>
        public SimpleParameterValueDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterValueDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public SimpleParameterValueDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
