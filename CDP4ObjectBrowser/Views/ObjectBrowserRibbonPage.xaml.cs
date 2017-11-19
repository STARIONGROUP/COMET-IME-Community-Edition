// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserRibbonPage.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser.Views
{
    using System.ComponentModel.Composition;
    using CDP4ObjectBrowser.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ObjectBrowserRibbon
    /// </summary>
    [Export(typeof(ObjectBrowserRibbonPage))]
    public partial class ObjectBrowserRibbonPage : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowserRibbonPage"/> class.
        /// </summary>
        public ObjectBrowserRibbonPage()
        {
            this.InitializeComponent();
            this.DataContext = new ObjectBrowserRibbonPageViewModel();
        }
    }
}
