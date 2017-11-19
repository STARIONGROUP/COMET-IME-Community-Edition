// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditorRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Ribbon;
    using Microsoft.Practices.Prism.Mvvm;
    using ViewModels;

    /// <summary>
    /// Interaction logic for RelationshipEditorRibbon.xaml
    /// </summary>
    [Export(typeof(RelationshipEditorRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class RelationshipEditorRibbon : ExtendedRibbonPageGroup, IView
    {
        public RelationshipEditorRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new RelationshipEditorRibbonViewModel();
        }
    }
}
