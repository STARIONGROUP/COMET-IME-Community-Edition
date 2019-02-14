// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Ribbon;
    using Microsoft.Practices.Prism.Mvvm;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ProductTreeRibbon.xaml
    /// </summary>
    [Export(typeof(RelationshipMatrixRibbon))]
    public partial class RelationshipMatrixRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixRibbon"/> class
        /// </summary>
        [ImportingConstructor]
        public RelationshipMatrixRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new RelationshipMatrixRibbonViewModel();
        }
    }
}