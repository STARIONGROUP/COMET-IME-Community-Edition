// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4DiagramEditorRibbon.xaml.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Views
{
    using System.ComponentModel.Composition;
    using ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for CDP4DiagramEditorRibbon.xaml
    /// </summary>
    [Export(typeof(CDP4DiagramEditorRibbon))]
    public partial class CDP4DiagramEditorRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4DiagramEditorRibbon"/> class.
        /// </summary>
        public CDP4DiagramEditorRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new DiagramEditorRibbonViewModel();
        }
    }
}