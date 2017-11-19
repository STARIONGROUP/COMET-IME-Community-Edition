// -------------------------------------------------------------------------------------------------
// <copyright file="ModelRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for DomainOfExpertiseRibbon.xaml
    /// </summary>
    [Export(typeof(ModelRibbon))]
    public partial class ModelRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseRibbon"/> class.
        /// </summary>
        public ModelRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ModelRibbonViewModel();
        }
    }
}
