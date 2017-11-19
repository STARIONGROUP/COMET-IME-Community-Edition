// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementBrowserRibbon.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.ComponentModel.Composition;
    using CDP4Requirements.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for RequirementBrowserRibbon.xaml
    /// </summary>
    public partial class RequirementBrowserRibbon : IView
    {
        [ImportingConstructor]
        public RequirementBrowserRibbon()
        {
            InitializeComponent();
            this.DataContext = new RequirementRibbonViewModel();
        }
    }
}