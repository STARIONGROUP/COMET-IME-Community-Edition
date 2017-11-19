// ------------------------------------------------------------------------------------------------
// <copyright file="IPanelNavigationService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Dal;
    using CDP4Dal.Composition;
    using Interfaces;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// The Interface for Panel Navigation Service
    /// </summary>
    public interface IPanelNavigationService
    {
        /// <summary>
        /// Opens the view associated to the provided view-model
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened
        /// </param>
        /// <param name="useRegionManager">
        /// A value indicating whether handling the opening of the view shall be message-based or not. In case it is
        /// NOT message-based, the <see cref="IRegionManager"/> handles opening and placement of the view.
        /// </param>
        /// <remarks>
        /// The data context of the view is the <see cref="IPanelViewModel"/>
        /// </remarks>
        void Open(IPanelViewModel viewModel, bool useRegionManager);

        /// <summary>
        /// Opens the properties of a <see cref="Thing"/> in a docking panel
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> which properties are displayed</param>
        /// <param name="session">The <see cref="ISession"/> associated</param>
        void Open(Thing thing, ISession session);

        /// <summary>
        /// Opens the view associated to a view-model. The view-model is identified by its <see cref="INameMetaData.Name"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <param name="session">The <see cref="ISession"/> associated.</param>
        /// <param name="useRegionManager">A value indicating whether handling the opening of the view shall be handled by the region manager.
        /// In case this region manager does not handle this, it will be event-based using the <see cref="CDPMessageBus"/>.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        void Open(string viewModelName, ISession session, bool useRegionManager, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService);

        /// <summary>
        /// Closes the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <param name="viewModel">
        /// The view-model that is to be closed.
        /// </param>
        /// <param name="useRegionManager">
        /// A value indicating whether handling the opening of the view shall be handled by the region manager. In case this region manager does not handle
        /// this it will be event-based using the <see cref="CDPMessageBus"/>.
        /// </param>
        void Close(IPanelViewModel viewModel, bool useRegionManager);

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> which associated <see cref="IPanelViewModel"/> is of a certain Type
        /// </summary>
        /// <param name="viewModelType">The <see cref="Type"/> of the <see cref="IPanelViewModel"/> to close</param>
        void Close(Type viewModelType);

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> associated to a data-source
        /// </summary>
        /// <param name="datasourceUri">The string representation of the data-source's uri</param>
        void Close(string datasourceUri);
    }
}
