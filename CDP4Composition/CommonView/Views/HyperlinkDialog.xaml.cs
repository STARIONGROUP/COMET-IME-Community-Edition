namespace CDP4CommonView.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for HyperLinkDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.HyperLink)]
    public partial class HyperLinkDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HyperLinkDialog"/> class.
        /// </summary>
        public HyperLinkDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperLinkDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public HyperLinkDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
