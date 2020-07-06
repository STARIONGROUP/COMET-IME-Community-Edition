// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingRibbonPage.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Ribbon;
    using CDP4Reporting.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ObjectBrowserRibbon
    /// </summary>
    [Export(typeof(ReportDesignerRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ReportDesignerRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesignerRibbon"/> class.
        /// </summary>
        /// <param name="panelNavigationService">
        /// The (MEF) injected <see cref="IPanelNavigationService"/> that is used to navigate to panels/browsers
        /// </param>
        /// <param name="dialogNavigationService">
        /// The (MEF) injected <see cref="IDialogNavigationService"/> that is used to navigate to generic dialogs
        /// </param>
        [ImportingConstructor]
        public ReportDesignerRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ReportDesignerRibbonViewModel();
        }
    }
}
