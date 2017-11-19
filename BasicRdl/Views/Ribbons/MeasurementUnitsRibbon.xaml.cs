// -------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;
    using BasicRdl.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for MeasurementUnitsRibbon.xaml
    /// </summary>
    [Export(typeof(MeasurementUnitsRibbon))]
    public partial class MeasurementUnitsRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsRibbon"/> class.
        /// </summary>
        public MeasurementUnitsRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new MeasurementUnitsRibbonViewModel();
        }
    }
}
