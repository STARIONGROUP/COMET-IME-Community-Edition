﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigationPanelEvent.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Events
{
    /// <summary>
    /// The panel status.
    /// </summary>
    public enum PanelStatus
    {
        /// <summary>
        /// Open status 
        /// </summary>
        Open,

        /// <summary>
        /// Closed status 
        /// </summary>
        Closed
    }

    /// <summary>
    /// The event carrying information on the status of <see cref="IPanelView"/>
    /// </summary>
    public class NavigationPanelEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPanelEvent"/> class
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> associated to the panel
        /// </param>
        /// <param name="view">
        /// The <see cref="IPanelView"/> that is to be opened
        /// </param>
        /// <param name="status">
        /// The status of the panel
        /// </param>
        /// <param name="regionName">
        /// The name of the region in which the panel shall be opened
        /// </param>
        public NavigationPanelEvent(IPanelViewModel viewModel, IPanelView view, PanelStatus status)
        {
            this.ViewModel = viewModel;
            this.View = view;
            this.PanelStatus = status;
        }

        /// <summary>
        /// Gets The <see cref="IPanelViewModel"/> associated to the panel
        /// </summary>
        public IPanelViewModel ViewModel { get; private set; }

        /// <summary>
        /// Gets The <see cref="IPanelView"/> associated to the panel
        /// </summary>
        public IPanelView View { get; private set; }

        /// <summary>
        /// Gets The status of the panel
        /// </summary>
        public PanelStatus PanelStatus { get; private set; }
    }
}
