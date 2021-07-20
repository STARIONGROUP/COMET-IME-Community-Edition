// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelNavigationService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
    using System.Threading.Tasks;

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
        private readonly IDialogNavigationService dialogNavigationService;

        [ImportingConstructor]
        public DockLayoutViewModel(IDialogNavigationService dialogNavigationService)
        {
            DockPanelViewModels = new ReactiveList<IPanelViewModel>();

            this.dialogNavigationService = dialogNavigationService;

            DockPanelClosingCommand = ReactiveCommand.CreateAsyncTask(arg => PanelClosing((ItemCancelEventArgs)arg));
            DockPanelClosedCommand = ReactiveCommand.CreateAsyncTask(arg => PanelClosed((DockItemClosedEventArgs)arg));
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

        private async Task PanelClosed(DockItemClosedEventArgs e)
        {
            foreach (var dockPanelViewModel in e.AffectedItems.Select(p => p.DataContext).OfType<IPanelViewModel>())
            {
                this.DockPanelViewModels.Remove(dockPanelViewModel);
            }
        }

        private async Task PanelClosing(ItemCancelEventArgs e)
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

            var confirmation = new GenericConfirmationDialogViewModel("Warning", MessageHelper.ClosingPanelConfirmation);

            if (dialogNavigationService.NavigateModal(confirmation)?.Result is not true)
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