// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DockLayoutViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reactive;

    using CDP4Composition.Navigation;

    using DevExpress.Xpf.Docking;
    using DevExpress.Xpf.Docking.Base;

    using ReactiveUI;

    /// <summary>
    /// This is the view model for the main application docking control. 
    /// </summary>
    [Export]
    public class DockLayoutViewModel : ReactiveObject
    {
        /// <summary>
        /// A dialog service for displaying user confirmations
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Initalizes a new instance of the <see cref="DockLayoutViewModel"/>
        /// </summary>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> to display user prompts</param>
        [ImportingConstructor]
        public DockLayoutViewModel(IDialogNavigationService dialogNavigationService)
        {
            this.DockPanelViewModels = new ReactiveList<IPanelViewModel>();

            this.dialogNavigationService = dialogNavigationService;

            //Forced to use async delegate without an await in order to achieve sychronous command call. Should be better options available in newer versions of reactive.
            this.DockPanelClosingCommand = ReactiveCommand.CreateAsyncTask(async arg => this.PanelClosing((ItemCancelEventArgs)arg));
            this.DockPanelClosedCommand = ReactiveCommand.CreateAsyncTask(async arg => this.PanelClosed((DockItemClosedEventArgs)arg));
        }

        /// <summary>
        /// The panel items of the dock
        /// </summary>
        public ReactiveList<IPanelViewModel> DockPanelViewModels { get; }

        /// <summary>
        /// Gets the command that is called when the dock panel is closing
        /// </summary>
        public ReactiveCommand<Unit> DockPanelClosingCommand { get; }

        /// <summary>
        /// Gets the command that is called when the dock panel is closed
        /// </summary>
        public ReactiveCommand<Unit> DockPanelClosedCommand { get; }

        /// <summary>
        /// Responds when a panel is closed and removes the view model from the collection
        /// </summary>
        /// <param name="e">The <see cref="DockItemClosedEventArgs"/></param>
        private void PanelClosed(DockItemClosedEventArgs e)
        {
            foreach (var dockPanelViewModel in e.AffectedItems.Select(p => p.DataContext).OfType<IPanelViewModel>())
            {
                this.DockPanelViewModels.Remove(dockPanelViewModel);
            }
        }

        /// <summary>
        /// Ask user for confirmation before closing panel
        /// </summary>
        /// <param name="e">The <see cref="ItemCancelEventArgs"/></param>
        private void PanelClosing(ItemCancelEventArgs e)
        {
            var docPanel = e.Item as LayoutPanel;
            if (docPanel is null)
            {
                return;
            }

            var panelViewModel = (IPanelViewModel)docPanel.Content;
            if (panelViewModel is null)
            {
                return;
            }

            if (!panelViewModel.IsDirty)
            {
                return;
            }

            var confirmation = new GenericConfirmationDialogViewModel(panelViewModel.Caption, MessageHelper.ClosingPanelConfirmation);

            if (this.dialogNavigationService.NavigateModal(confirmation)?.Result is not true)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Adds a panel to the dock
        /// </summary>
        /// <param name="panelViewModel">The <see cref="IPanelViewModel"/> to add to the dock</param>
        public void AddDockPanelViewModel(IPanelViewModel panelViewModel)
        {
            this.DockPanelViewModels.Add(panelViewModel);
            panelViewModel.IsSelected = true;
        }
    }
}
