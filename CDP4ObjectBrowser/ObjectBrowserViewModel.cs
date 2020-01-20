// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Reactive.Linq;
    using CDP4Composition;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The <see cref="ObjectBrowserViewModel"/> is a View Model that is responsible for managing the data and interactions with that data for a view
    /// that shows all the <see cref="Thing"/>s contained by a data-source following the containment tree that is modelled in 10-25 and the CDP4 extensions.
    /// </summary>
    [Export(typeof(ObjectBrowserViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ObjectBrowserViewModel : ReactiveObject, IPanelViewModel
    {
        /// <summary>
        /// Backing field for <see cref="Person"/> property
        /// </summary>
        private string person;

        /// <summary>
        /// The <see cref="ISession"/> the current browser is bound to.
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowserViewModel"/> class.
        /// </summary>
        public ObjectBrowserViewModel(ISession session, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            this.Identifier = Guid.NewGuid();
            this.session = session;
            this.Disposables = new List<IDisposable>();
            
            this.Sessions = new DisposableReactiveList<SessionRowViewModel>();
            this.AddSession();

            var activePerson = this.session.ActivePerson;
            this.Person = (activePerson == null) ? string.Empty : this.session.ActivePerson.Name;
            if (activePerson != null)
            {
                var personSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(activePerson)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(
                        _ =>
                        {
                            this.Person = this.session.ActivePerson.Name;
                        });
                this.Disposables.Add(personSubscription);
            }

            this.Caption = "CDP4 Object Browser";
            this.ToolTip = string.Format("{0}\n{1}", session.DataSourceUri, session.ActivePerson.Name);
        }

        /// <summary>
        /// Gets the Caption of the control
        /// </summary>
        public string Caption { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets the tooltip of the control
        /// </summary>
        public string ToolTip { get; private set; }

        /// <summary>
        /// Gets the data-source
        /// </summary>
        public string DataSource
        {
            get { return this.session.DataSourceUri; }
        }

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the name of the active <see cref="Person"/>
        /// </summary>
        public string Person
        {
            get { return this.person; }
            set { this.RaiseAndSetIfChanged(ref this.person, value); }
        }

        /// <summary>
        /// Gets the <see cref="SessionRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<SessionRowViewModel> Sessions { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="IDisposable"/> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; private set; }

        /// <summary>
        /// Add the <see cref="Session"/> to the browser
        /// </summary>
        /// <param name="session">
        /// The <see cref="Session"/> that is to be added
        /// </param>
        private void AddSession()
        {
            var siteDirectory = this.session.RetrieveSiteDirectory();
            var rowViewModel = new SessionRowViewModel(siteDirectory, this.session, null);
            this.Sessions.Add(rowViewModel);
        }

        /// <summary>
        /// Dispose of this <see cref="IPanelViewModel"/>
        /// </summary>
        public void Dispose()
        {
            foreach (var sessionRowViewModel in this.Sessions)
            {
                sessionRowViewModel.Dispose();
            }

            foreach (var disposable in this.Disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
