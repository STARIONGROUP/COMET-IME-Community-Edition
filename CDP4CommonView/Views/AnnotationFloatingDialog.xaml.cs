// -------------------------------------------------------------------------------------------------
// <copyright file="AnnotationFloatingDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Views
{
    using System.ComponentModel;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;
    using ViewModels;

    /// <summary>
    /// Interaction logic for AnnotationFloatingDialog.xaml
    /// </summary>
    [DialogViewExport("AnnotationFloatingDialog", "The dialog responsible for annotation display")]
    public partial class AnnotationFloatingDialog : DXWindow, IDialogView
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationFloatingDialog"/> class
        /// </summary>
        public AnnotationFloatingDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationFloatingDialog"/> class
        /// </summary>
        public AnnotationFloatingDialog(IDialogNavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.InitializeComponent();
        }

        /// <summary>
        /// The closing event handler
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs"/></param>
        /// <remarks>
        /// The view-model shall be disposed when closed
        /// </remarks>
        protected override void OnClosing(CancelEventArgs e)
        {
            var vm = this.DataContext as AnnotationFloatingDialogViewModel;
            if (vm != null)
            {
                vm.Dispose();
                this.navigationService.ClosingFloatingWindow(this);
            }

            base.OnClosing(e);
        }
    }
}