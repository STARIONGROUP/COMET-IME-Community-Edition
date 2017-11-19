// ------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for DomainOfExpertiseDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.DomainOfExpertise)]
    public partial class DomainOfExpertiseDialog : DXWindow, IThingDialogView
    {
        public DomainOfExpertiseDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public DomainOfExpertiseDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}