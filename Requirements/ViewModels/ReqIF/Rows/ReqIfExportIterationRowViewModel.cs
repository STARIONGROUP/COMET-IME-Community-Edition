// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportIterationRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Globalization;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The iteration row in the reqIF export view-model
    /// </summary>
    public class ReqIfExportIterationRowViewModel
    {
        /// <summary>
        /// The <see cref="Iteration"/> represented
        /// </summary>
        public readonly Iteration Iteration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfExportIterationRowViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> represented</param>
        public ReqIfExportIterationRowViewModel(Iteration iteration)
        {
            this.Iteration = iteration;
            this.IterationNumber = this.Iteration.IterationSetup.IterationNumber.ToString(CultureInfo.InvariantCulture);
            var model = (EngineeringModel)this.Iteration.Container;
            this.Model = model.EngineeringModelSetup.Name;
            this.DataSourceUri = this.Iteration.IDalUri.ToString();
        }

        /// <summary>
        /// Gets the Iteration number
        /// </summary>
        public string IterationNumber { get; private set; }

        /// <summary>
        /// Gets the name of the <see cref="EngineeringModel"/>
        /// </summary>
        public string Model { get; private set; }

        /// <summary>
        /// Gets the uri of the data-source
        /// </summary>
        public string DataSourceUri { get; private set; }
    }
}
