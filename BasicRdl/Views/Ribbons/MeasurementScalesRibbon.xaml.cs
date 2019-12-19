// -------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;
    using BasicRdl.ViewModels;
    using DevExpress.Xpf.Bars;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for MeaurementScalesRibbon.xaml
    /// </summary>
    [Export(typeof(MeasurementScalesRibbon))]
    public partial class MeasurementScalesRibbon : IView, IBarItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScalesRibbon"/> class.
        /// </summary>
        public MeasurementScalesRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new MeasurementScalesRibbonViewModel();
        }
    }
}
