// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoControls.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Ribbon;
    using CDP4LogInfo.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;
    
    /// <summary>
    /// Interaction logic for LogInfoControls.xaml
    /// </summary>
    [Export(typeof(LogInfoControls))]
    public partial class LogInfoControls : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoControls"/> class
        /// </summary>
        [ImportingConstructor]
        public LogInfoControls(IDialogNavigationService dialogNavigationService)
        {
            this.InitializeComponent();
            this.DataContext = new LogInfoControlsViewModel(dialogNavigationService);
        }
    }
}