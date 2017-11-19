// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageRibbon.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Ribbon;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for NaturalLanguageRibbon.xaml
    /// </summary>
    [Export(typeof(NaturalLanguageRibbon))]
    public partial class NaturalLanguageRibbon :  IView
    {
        public NaturalLanguageRibbon()
        {
            InitializeComponent();
            this.DataContext = new NaturalLanguageRibbonViewModel();
        }
    }
}
