// -----------------------------------------------------------------------
// <copyright file="IIterationTrackParameterViewModel.cs" company="RHEA">
// Copyright (c) 2020 RHEA Group. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget
{
    using System.Windows;

    using CDP4Composition.Mvvm;

    using CDP4Dashboard.ViewModels.Charts;

    using ReactiveUI;

    /// <summary>
    /// The interface that describes the IterationTrackParameterViewModel />
    /// </summary>
    public interface IIterationTrackParameterViewModel
    {
        /// <summary>
        /// Gets or sets the visibility of the widget's chart component
        /// </summary>
        Visibility ChartVisible { get; set; }

        /// <summary>
        /// Sets or gets the widget's collection of <see cref="LineSeriesCollection"/> instances
        /// </summary>
        ReactiveList<LineSeries> LineSeriesCollection { get; set; }
    }
}
