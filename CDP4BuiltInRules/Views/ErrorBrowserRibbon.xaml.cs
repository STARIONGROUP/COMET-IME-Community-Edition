// -------------------------------------------------------------------------------------------------
// <copyright file="ErrorBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Views
{
    using System.ComponentModel.Composition;
    using CDP4BuiltInRules.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ErrorBrowserRibbon.xaml
    /// </summary>
    [Export(typeof(ErrorBrowserRibbon))]
    public partial class ErrorBrowserRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBrowserRibbon"/> class.
        /// </summary>
        public ErrorBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ErrorBrowserRibbonViewModel();
        }
    }
}
