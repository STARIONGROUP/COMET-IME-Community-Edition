// ------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for TelephoneNumberDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.TelephoneNumber)]
    public partial class TelephoneNumberDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelephoneNumberDialog"/> class.
        /// </summary>
        public TelephoneNumberDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelephoneNumberDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public TelephoneNumberDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
