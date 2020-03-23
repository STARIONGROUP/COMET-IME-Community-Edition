// ------------------------------------------------------------------------------------------------
// <copyright file="DiagramRibbonPageCategory.xaml.cs" company="RHEA S.A.">
//   Copyright (c) 2020 RHEA S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Views.ContextualRibbonPages
{
    using System.ComponentModel.Composition;

    using CDP4DiagramEditor.ViewModels.ContextualRibbonPageViewModels;

    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for DiagramRibbonPageCategory.xaml
    /// </summary>
    [Export(typeof(DiagramRibbonPageCategory))]
    public partial class DiagramRibbonPageCategory : IView
    {
        public DiagramRibbonPageCategory()
        {
            this.InitializeComponent();

            this.DataContext = new DiagramRibbonPageCategoryViewModel();
        }
    }
}
