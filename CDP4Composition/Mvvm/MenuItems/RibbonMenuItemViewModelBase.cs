// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;    
    using CDP4Dal;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Navigation;
    using Navigation.Events;
    using Navigation.Interfaces;
    using PluginSettingService;
    using ReactiveUI;
    using ViewModels;

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
            this.MenuItemContent = menuItemContent;

            this.Disposables = new List<IDisposable>();

            this.PanelNavigationServive = ServiceLocator.Current.GetInstance<IPanelNavigationService>();
            this.PermissionService = this.Session.PermissionService;
            this.ThingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();
            this.DialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            this.PluginSettingsService = ServiceLocator.Current.GetInstance<IPluginSettingsService>();

            this.ShowOrClosePanelCommand = ReactiveCommand.Create();
            this.ShowOrClosePanelCommand.Subscribe(x => this.ExecuteShowOrHide());

            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.ViewModel == this.PanelViewModel && x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.HandleClosedPanel());
        }

        /// <summary>
        /// Gets the <see cref="IPanelViewModel"/> to open associated with this menu item
        /// </summary>
        public IPanelViewModel PanelViewModel { get; private set; }

        /// <summary>
        /// Gets the content displayed in the user interface for this <see cref="RibbonMenuItemViewModelBase"/>
        /// </summary>
        public string MenuItemContent
        {
            get { return this.menuItemContent; }
            protected set { this.RaiseAndSetIfChanged(ref this.menuItemContent, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is checked
        /// </summary>
        public bool IsChecked
        {
            get { return this.isChecked; }
            set { this.RaiseAndSetIfChanged(ref this.isChecked, value); }
        }

        /// <summary>
        /// Gets the command to show or close a model browser panel
        /// </summary>
        public ReactiveCommand<object> ShowOrClosePanelCommand { get; private set; }

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
        /// Executes the show or hide panel command
        /// </summary>
        protected void ExecuteShowOrHide()
        {
            if (this.IsChecked)
            {
                this.PanelViewModel = this.InstantiatePanelViewModel();
                if (this.PanelViewModel == null)
                {
                    throw new InvalidOperationException("The view-model to navigate to is null.");
                }

                this.PanelNavigationServive.Open(this.PanelViewModel);
            }
            else
            {
                if (this.PanelViewModel == null)
                {
                    return;
                }

                if (this.PanelViewModel.IsDirty)
                {
                    var confirmation = new GenericConfirmationDialogViewModel("Warning", MessageHelper.ClosingPanelConfirmation);
                    var result = this.DialogNavigationService.NavigateModal(confirmation);
                    if (result != null && result.Result.HasValue && result.Result.Value)
                    {
                        this.PanelNavigationServive.Close(this.PanelViewModel);
                    }
                    else
                    {
                        this.IsChecked = true;
                    }
                }
                else
                {
                    this.PanelNavigationServive.Close(this.PanelViewModel);
                }
            }
        }

        /// <summary>
        /// Handles the close panel event sent by the Navigation Service
        /// </summary>
        private void HandleClosedPanel()
        {
            this.IsChecked = false;
            this.PanelViewModel = null;
        }
    }
}