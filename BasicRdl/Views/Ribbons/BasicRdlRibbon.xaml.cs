// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Ribbon;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for <see cref="BasicRdlRibbon"/>
    /// </summary>
    [Export(typeof(BasicRdlRibbon))]
    public partial class BasicRdlRibbon : ExtendedRibbonPage, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRdlRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public BasicRdlRibbon()
        {
            this.InitializeComponent();
        }
    }
}
