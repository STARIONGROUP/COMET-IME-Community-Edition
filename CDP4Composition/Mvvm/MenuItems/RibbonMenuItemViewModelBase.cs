// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemViewModelBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The ribbon menu-item view-model base
    /// </summary>
    public abstract class RibbonMenuItemViewModelBase : ReactiveObject, IDisposable
    {
        /// <summary>
        /// The application's <see cref="IPermissionService"/>
        /// </summary>
        protected readonly IPermissionService PermissionService;

        /// <summary>
        /// The application's <see cref="IPanelNavigationService"/>
        /// </summary>
        protected readonly IPanelNavigationService PanelNavigationServive;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        protected readonly IThingDialogNavigationService ThingDialogNavigationService;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        protected readonly IDialogNavigationService DialogNavigationService;

        /// <summary>
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </summary>
        protected readonly IPluginSettingsService PluginSettingsService;

        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Gets the list of <see cref="IDisposable"/> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; private set; }

        /// <summary>
        /// The associated <see cref="ISession"/>
        /// </summary>
        public readonly ISession Session;

        /// <summary>
        /// Backing field for <see cref="MenuItemContent"/>
        /// </summary>
        private string menuItemContent;

        /// <summary>
        /// Static part of the MenuItemCentent
        /// </summary>
        private readonly string staticMenuItemContent;

        /// <summary>
        /// Backing field for <see cref="IsChecked"/> 
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonMenuItemViewModelBase"/> class
        /// </summary>
        /// <param name="menuItemContent">The content of this item to display</param>
        /// <param name="session">The <see cref="ISession"/></param>
        protected RibbonMenuItemViewModelBase(string menuItemContent, ISession session)
        {
            this.Session = session;
            this.staticMenuItemContent = menuItemContent;
            this.menuItemContent = this.staticMenuItemContent;

            this.Disposables = new List<IDisposable>();

            this.PanelNavigationServive = ServiceLocator.Current.GetInstance<IPanelNavigationService>();
            this.PermissionService = this.Session.PermissionService;
            this.ThingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();
            this.DialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            this.PluginSettingsService = ServiceLocator.Current.GetInstance<IPluginSettingsService>();

            this.ShowPanelCommand = ReactiveCommand.Create();
            this.ShowPanelCommand.Subscribe(x => this.ExecuteShowPanel());

            this.ClosePanelsCommand = ReactiveCommand.Create();
            this.ClosePanelsCommand.Subscribe(x => this.ExecuteClosePanels());

            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => this.PanelViewModels.Contains(x.ViewModel) && x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.HandleClosedPanel(x.ViewModel));

            this.PanelViewModels.CountChanged.Subscribe(this.PanelViewModelsCountChanged);
        }

        /// <summary>
        /// Change properties when the number of panels changes
        /// </summary>
        /// <param name="panelCount">
        /// The new number of open panels
        /// </param>
        private void PanelViewModelsCountChanged(int panelCount)
        {
            this.IsChecked = panelCount > 0;
            var prefix = string.Empty;

            if (panelCount > 0)
            {
                prefix = $"({panelCount}) ";
            }

            this.MenuItemContent = $"{prefix}{this.staticMenuItemContent}";
        }

        /// <summary>
        /// Gets the <see cref="IPanelViewModel"/> to open associated with this menu item
        /// </summary>
        public ReactiveList<IPanelViewModel> PanelViewModels { get; } = new ReactiveList<IPanelViewModel>();

        /// <summary>
        /// Gets the content displayed in the user interface for this <see cref="RibbonMenuItemViewModelBase"/>
        /// </summary>
        public string MenuItemContent
        {
            get => this.menuItemContent;
            protected set => this.RaiseAndSetIfChanged(ref this.menuItemContent, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is checked
        /// </summary>
        public bool IsChecked
        {
            get => this.isChecked;
            set => this.RaiseAndSetIfChanged(ref this.isChecked, value);
        }

        /// <summary>
        /// Gets the command to open a model browser panel
        /// </summary>
        public ReactiveCommand<object> ShowPanelCommand { get; private set; }

        /// <summary>
        /// Gets the command to close browser panels
        /// </summary>
        public ReactiveCommand<object> ClosePanelsCommand { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    foreach (var disposable in this.Disposables)
                    {
                        disposable.Dispose();
                    }
                }

                // Indicate that the instance has been disposed.
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <returns>a new instance of the <see cref="IPanelViewModel"/></returns>
        protected abstract IPanelViewModel InstantiatePanelViewModel();

        /// <summary>
        /// Executes the show panel command
        /// </summary>
        protected void ExecuteShowPanel()
        {
            var panelViewModel = this.InstantiatePanelViewModel();

            if (panelViewModel == null)
            {
                throw new InvalidOperationException("The view-model to navigate to is null.");
            }

            this.PanelViewModels.Add(panelViewModel);
            this.PanelNavigationServive.OpenInDock(panelViewModel);
        }

        /// <summary>
        /// Executes the close panels command
        /// </summary>
        private void ExecuteClosePanels()
        {
            foreach (var panel in this.PanelViewModels.ToList())
            {
                if (panel.IsDirty)
                {
                    var confirmation = new GenericConfirmationDialogViewModel("Warning", MessageHelper.ClosingPanelConfirmation);
                    var result = this.DialogNavigationService.NavigateModal(confirmation);

                    if (result != null && result.Result.HasValue && result.Result.Value)
                    {
                        this.PanelNavigationServive.CloseInDock(panel);
                    }
                    else
                    {
                        this.IsChecked = true;
                    }
                }
                else
                {
                    this.PanelNavigationServive.CloseInDock(panel);
                }

                this.HandleClosedPanel(panel);
            }
        }

        /// <summary>
        /// Handles the close panel event sent by the Navigation Service
        /// </summary>
        /// <param name="panelViewModel"></param>
        private void HandleClosedPanel(IPanelViewModel panelViewModel)
        {
            this.PanelViewModels.Remove(panelViewModel);
        }
    }
}
