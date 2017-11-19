// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;

    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for <see cref="TeamCompositionBrowserRibbon"/> XAML
    /// </summary>
    [Export(typeof(TeamCompositionBrowserRibbon))]
    public partial class TeamCompositionBrowserRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompositionBrowserRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public TeamCompositionBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new TeamCompositionBrowserRibbonViewModel();
        }
    }
}
