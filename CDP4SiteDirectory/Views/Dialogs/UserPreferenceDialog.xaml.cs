// ------------------------------------------------------------------------------------------------
// <copyright file="UserPreferenceDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for UserPreference.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.UserPreference)]
    public partial class UserPreferenceDialog : DXWindow, IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferenceDialog"/> class.
        /// </summary>
        public UserPreferenceDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferenceDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public UserPreferenceDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
