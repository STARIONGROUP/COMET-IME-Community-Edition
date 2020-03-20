// ------------------------------------------------------------------------------------------------
// <copyright file="CDP4DiagramEditor.xaml.cs" company="RHEA S.A.">
//   Copyright (c) 2020 RHEA S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Views.ContextualRibbonPages
{
    using System.ComponentModel.Composition;

    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for DiagramDesignRibbonPage.xaml
    /// </summary>
    [Export(typeof(DiagramDesignRibbonPage))]
    public partial class DiagramDesignRibbonPage : IView
    {
        public DiagramDesignRibbonPage()
        {
            this.InitializeComponent();
        }
    }
}
