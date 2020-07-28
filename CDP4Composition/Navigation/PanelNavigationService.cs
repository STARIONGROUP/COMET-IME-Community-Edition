//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PanelNavigationService.cs" company="RHEA System S.A.">
//     Copyright (c) 2015-2020 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//             Nathanael Smiechowski, Kamil Wojnowski
// 
//     This file is part of CDP4-IME Community Edition.
//     The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//     compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-IME Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or any later version.
// 
//     The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU Affero General Public License for more details.
// 
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;

    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Composition;

    using NLog;

    /// <summary>
    /// The panel navigation service class that provides services to open a docking panel given a <see cref="Thing"/> or a <see cref="IPanelViewModel"/>
    /// </summary>
    [Export(typeof(IPanelNavigationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PanelNavigationService : IPanelNavigationService
    {
        /// <summary>
        /// The fully qualified name of the PropertyGrid view-model
        /// </summary>
        private const string PropertyViewModel = "CDP4PropertyGrid.ViewModels.PropertyGridViewModel";

        /// <summary>
        /// The (injected) <see cref="IFilterStringService"/>
        /// </summary>
        private readonly IFilterStringService filterStringService;

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="PanelNavigationService"/> class
        /// </summary>
        /// <param name="panelViewKinds">
        /// The injected list of <see cref="IPanelView"/> that can be navigated to.
        /// </param>
        /// <param name="panelViewModelKinds">
        /// The MEF injected Panel view models that can be navigated to.
        /// </param>
        /// <param name="panelViewModelDecorated">
        /// The MEF injected <see cref="IPanelViewModel"/> which are decorated with <see cref="INameMetaData"/> and can be navigated to.
        /// </param>
        /// <param name="filterStringService">The MEF injected <see cref="IFilterStringService"/></param>
        [ImportingConstructor]
        public PanelNavigationService(
            [ImportMany] IEnumerable<Lazy<IPanelView, IRegionMetaData>> panelViewKinds,
            [ImportMany] IEnumerable<IPanelViewModel> panelViewModelKinds,
            [ImportMany] IEnumerable<Lazy<IPanelViewModel, INameMetaData>> panelViewModelDecorated,
            IFilterStringService filterStringService)
        {
            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("Instantiating the PanelNavigationService");

            this.filterStringService = filterStringService;
            this.PanelViewKinds = new Dictionary<string, Lazy<IPanelView, IRegionMetaData>>();

            // TODO T2428 : PanelViewModelKinds seems to be always empty and is used only one time in the Open(Thing thing, ISession session) method. We should probably refactor this part of the code.
            this.PanelViewModelKinds = new Dictionary<string, IPanelViewModel>();

            this.ViewModelViewPairs = new Dictionary<IPanelViewModel, IPanelView>();
            this.PanelViewModelDecorated = new Dictionary<string, Lazy<IPanelViewModel, INameMetaData>>();

            foreach (var panelView in panelViewKinds)
            {
                var panelViewName = panelView.Value.ToString();

                this.PanelViewKinds.Add(panelViewName, panelView);
                logger.Trace("Add panelView {0} ", panelViewName);
            }

            foreach (var panelViewModel in panelViewModelKinds)
            {
                var panelViewModelName = panelViewModel.ToString();

                this.PanelViewModelKinds.Add(panelViewModelName, panelViewModel);
                logger.Trace("Add panelViewModel {0} ", panelViewModelName);
            }

            foreach (var panelViewModel in panelViewModelDecorated)
            {
                var panelViewModelName = panelViewModel.Value.ToString();

                var panelViewModelDescribeName = panelViewModel.Metadata.Name;
                this.PanelViewModelDecorated.Add(panelViewModelDescribeName, panelViewModel);

                logger.Trace("Add panelViewModel {0} ", panelViewModelName);
            }

            sw.Stop();
            logger.Debug("The PanelNavigationService was instantiated in {0} [ms]", sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Gets the list of <see cref="IPanelView"/> in the application
        /// </summary>
        public Dictionary<string, Lazy<IPanelView, IRegionMetaData>> PanelViewKinds { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="IPanelView"/> in the application
        /// </summary>
        public Dictionary<string, IPanelViewModel> PanelViewModelKinds { get; private set; }

        /// <summary>
        /// Gets the {<see cref="IPanelViewModel"/>, <see cref="IPanelView"/>} pairs that are in the regions
        /// </summary>
        public Dictionary<IPanelViewModel, IPanelView> ViewModelViewPairs { get; private set; }

        /// <summary>
        /// Gets the list of the <see cref="IPanelViewModel"/> which are decorated with <see cref="INameMetaData"/>.
        /// </summary>
        public Dictionary<string, Lazy<IPanelViewModel, INameMetaData>> PanelViewModelDecorated { get; private set; }

        /// <summary>
        /// Opens the view associated to the provided view-model
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened
        /// </param>
        /// <remarks>
        /// The data context of the view is the <see cref="IPanelViewModel"/>
        /// </remarks>
        public void Open(IPanelViewModel viewModel)
        {
        }

        /// <summary>
        /// Opens the <see cref="Thing"/> in a property panel
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> which properties are displayed</param>
        /// <param name="session">The <see cref="ISession"/> associated to the <see cref="Thing"/></param>
        public void Open(Thing thing, ISession session)
        {
            if (!this.PanelViewModelKinds.TryGetValue(PropertyViewModel, out var vm))
            {
                logger.Warn("The plugin for the Property panel could not be found.");
                return;
            }

            var viewModelType = vm.GetType();
            var propGridVmInstance = Activator.CreateInstance(viewModelType, thing, session) as IPanelViewModel;

            var existentViewModel = this.ViewModelViewPairs.Keys.SingleOrDefault(x => x.GetType() == viewModelType);

            if (existentViewModel != null)
            {
                // Updates the view-model of the property-grid
                var existentView = this.ViewModelViewPairs[existentViewModel];
                this.ViewModelViewPairs.Remove(existentViewModel);

                existentView.DataContext = propGridVmInstance;

                if (propGridVmInstance != null)
                {
                    this.ViewModelViewPairs.Add(propGridVmInstance, existentView);
                }
            }
        }

        /// <summary>
        /// Opens the view associated to a view-model. The view-model is identified by its <see cref="INameMetaData.Name"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <param name="session">The <see cref="ISession"/> associated.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        public void Open(
            string viewModelName,
            ISession session,
            IThingDialogNavigationService thingDialogNavigationService,
            IDialogNavigationService dialogNavigationService)
        {
            if (!this.PanelViewModelDecorated.TryGetValue(viewModelName, out var returned))
            {
                throw new ArgumentOutOfRangeException($"The ViewModel with the human readable name {viewModelName} could not be found");
            }

            var siteDirectory = session.RetrieveSiteDirectory();

            // TODO T2429 : check that the view model is associated to a site directory
            var parameters = new object[]
                { session, siteDirectory, thingDialogNavigationService, this, dialogNavigationService };

            var viewModel = Activator.CreateInstance(returned.Value.GetType(), parameters) as IPanelViewModel;

            this.Open(viewModel);
        }

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
        public void Close(IPanelViewModel viewModel)
        {
            if (this.ViewModelViewPairs.TryGetValue(viewModel, out var view))
            {
                this.CleanUpPanelsAndSendCloseEvent(viewModel, view);
            }
        }

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> associated to a data-source
        /// </summary>
        /// <param name="datasourceUri">The string representation of the data-source's uri</param>
        public void Close(string datasourceUri)
        {
            logger.Debug("Starting to close all view-models related to data-source {0}", datasourceUri);

            var openViewModel = this.ViewModelViewPairs.Keys.Where(x => x.DataSource == datasourceUri).ToList();

            foreach (var panelViewModel in openViewModel)
            {
                this.Close(panelViewModel);
            }

            logger.Debug("All view-models related to data-source {0} closed", datasourceUri);
        }

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> which associated <see cref="IPanelViewModel"/> is of a certain Type
        /// </summary>
        /// <param name="viewModelType">The <see cref="Type"/> of the <see cref="IPanelViewModel"/> to close</param>
        public void Close(Type viewModelType)
        {
            var panelsToClose = this.ViewModelViewPairs.Keys.Where(vm => vm.GetType() == viewModelType).ToList();

            foreach (var panel in panelsToClose)
            {
                this.Close(panel);
            }
        }

        /// <summary>
        /// Gets the fully qualified name of the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <remarks>
        /// We assume here that for a <see cref="IPanelViewModel"/> with a fully qualified name xxx.yyy.ViewModels.DialogViewModel, the counterpart view is xxx.yyy.Views.Dialog
        /// </remarks>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/></param>
        /// <returns>The Fully qualified Name</returns>
        private Lazy<IPanelView, IRegionMetaData> GetViewType(IPanelViewModel viewModel)
        {
            var fullyQualifiedName = viewModel.ToString().Replace(".ViewModels.", ".Views.");

            // remove "ViewModel" from the name to get the View Name
            var viewName = System.Text.RegularExpressions.Regex.Replace(fullyQualifiedName, "ViewModel$", "");

            if (!this.PanelViewKinds.TryGetValue(viewName, out var returned))
            {
                throw new ArgumentOutOfRangeException($"The View associated to the viewModel {viewModel} could not be found\nMake sure the view has the proper attributes");
            }

            return returned;
        }

        /// <summary>
        /// Handles the view CollectionChanged event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">the <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void ViewCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove)
            {
                return;
            }

            foreach (var view in e.OldItems)
            {
                if (!(view is IPanelView viewPanel))
                {
                    continue;
                }

                var pair = this.ViewModelViewPairs.SingleOrDefault(x => x.Value == viewPanel);

                if (!pair.Equals(default(KeyValuePair<IPanelViewModel, IPanelView>)))
                {
                    var panelViewModel = pair.Key;
                    var panelView = pair.Value;
                    this.CleanUpPanelsAndSendCloseEvent(panelViewModel, panelView);
                }
            }
        }

        /// <summary>
        /// removes the view and view-model from the <see cref="ViewModelViewPairs"/> and send a panel close event
        /// </summary>
        /// <param name="panelViewModel">
        /// The <see cref="IPanelViewModel"/> that needs to be cleaned up
        /// </param>
        /// <param name="panelView">
        /// The <see cref="IPanelView"/> that needs to be cleaned up
        /// </param>
        private void CleanUpPanelsAndSendCloseEvent(IPanelViewModel panelViewModel, IPanelView panelView)
        {
            this.ViewModelViewPairs.Remove(panelViewModel);

            var closePanelEvent = new NavigationPanelEvent(panelViewModel, panelView, PanelStatus.Closed);
            CDPMessageBus.Current.SendMessage(closePanelEvent);

            panelView.DataContext = null;
            panelViewModel.Dispose();

            // unregister from filter string service
            this.filterStringService.UnregisterFromService(panelView);
        }
    }
}
