// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common;

    using CDP4Composition.MessageBus;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using NLog;

    using ReactiveUI;

    using Thing = CDP4Common.CommonData.Thing;

    /// <summary>
    /// Abstract base class from which all view-models that represent a <see cref="Thing"/> need to derive
    /// </summary>
    /// <typeparam name="T">
    /// A type of Thing that is represented by the view-model
    /// </typeparam>
    public abstract class ViewModelBase<T> : ReactiveObject, IViewModelBase<T>, IISession where T : Thing
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger logger;

        /// <summary>
        /// Gets or sets the parent <see cref="IViewModelBase{T}"/>
        /// </summary>
        public IViewModelBase<Thing> ContainerViewModel { get; protected set; }

        /// <summary>
        /// Field that indicates that MessageBusSubscriptions are allowed for this instance.
        /// </summary>
        protected bool AllowMessageBusSubscriptions = true;

        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Backing field for the <see cref="ThemeName"/>property.
        /// </summary>
        private string themeName;

        /// <summary>
        /// Backing field for <see cref="RevisionNumber"/>
        /// </summary>
        private int revisionNumber;

        /// <summary>
        /// Backing field for <see cref="ModifiedOn"/> property.
        /// </summary>
        private DateTime modifiedOn;

        /// <summary>
        /// Backing Field For isBusy
        /// </summary>
        private bool? isBusy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{T}"/> class.
        /// </summary>
        protected ViewModelBase()
        {
            this.InitializeLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{T}"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <param name="session">
        /// The session this view model belongs to.
        /// </param>
        protected ViewModelBase(T thing, ISession session)
        {
            this.InitializeLogger();
            this.Initialize(thing, session);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{T}"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <param name="session">
        /// The session this view model belongs to.
        /// </param>
        /// <param name="containerViewModel">
        /// The parent ViewModel
        /// </param>
        protected ViewModelBase(T thing, ISession session, IViewModelBase<Thing> containerViewModel)
        {
            this.InitializeLogger();
            this.ContainerViewModel = containerViewModel;
            this.Initialize(thing, session);
        }

        /// <summary>
        /// Initializes this instance of <see cref="ViewModelBase{T}"/>
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        private void Initialize(T thing, ISession session)
        {
            this.BeforeInitialize();
            this.ThemeName = AppliedTheme.ThemeName;

            this.PermissionService = session.PermissionService;
            this.Disposables = new List<IDisposable>();
            this.Thing = thing;
            this.Session = session;

            this.RevisionNumber = thing.RevisionNumber;
            this.IDalUri = thing.IDalUri;

            Func<ObjectChangedEvent, bool> discriminator =
                objectChange =>
                objectChange.EventKind == EventKind.Updated 
                && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber;

            Action<ObjectChangedEvent> action = this.ObjectChangeEventHandler;

            if (this.AllowMessageBusSubscriptions)
            {
                var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.Disposables.Add(thingSubscription);
            }
            else
            {
                var thingObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(T));

                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(
                    thingObserver, 
                    new ObjectChangedMessageBusEventHandlerSubscription(
                        this.Thing, 
                        discriminator,
                        action)));
            }
        }

        /// <summary>
        /// Initializes the <see cref="logger"/>
        /// </summary>
        private void InitializeLogger()
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
        }

        /// <summary>
        /// Execute code that needs te run before all code in the hierarchy of constructors is executed, but after a ContainerViewModel was added
        /// </summary>
        protected virtual void BeforeInitialize()
        {
            var containerViewModel = this.ContainerViewModel;

            while (containerViewModel != null)
            {
                if (containerViewModel is IHaveMessageBusHandler hasMessageBusHandlers)
                {
                    this.MessageBusHandler = hasMessageBusHandlers.MessageBusHandler;

                    break;
                }

                if (containerViewModel is IHaveContainerViewModel hasContainerViewModel)
                {
                    containerViewModel = hasContainerViewModel.ContainerViewModel;
                }
                else
                {
                    containerViewModel = null;
                }
            }

            this.AllowMessageBusSubscriptions = this.MessageBusHandler == null;
        }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPermissionService"/>
        /// </summary>
        /// <remarks>
        /// This is a convenience property refernces the <see cref="ISession.PermissionService"/>
        /// </remarks>
        public IPermissionService PermissionService { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the browser is busy
        /// </summary>
        public bool? IsBusy
        {
            get { return this.isBusy; }
            set { this.RaiseAndSetIfChanged(ref this.isBusy, value); }
        }

        /// <summary>
        /// Gets the revision number of the <see cref="Thing"/> that is represented by the view-model when
        /// it was last updated
        /// </summary>
        public int RevisionNumber
        {
            get { return this.revisionNumber; }
            private set { this.RaiseAndSetIfChanged(ref this.revisionNumber, value); }
        }

        /// <summary>
        /// Gets or sets the <see ref="DateTime"/> at which the <see ref="Thing"/> was last modified.
        /// </summary>
        [CDPVersion("1.1.0")]
        public DateTime ModifiedOn
        {
            get { return this.modifiedOn; }
            set { this.RaiseAndSetIfChanged(ref this.modifiedOn, value); }
        }

        /// <summary>
        /// Gets or sets the Theme name
        /// </summary>
        public string ThemeName
        {
            get { return this.themeName; }
            set { this.RaiseAndSetIfChanged(ref this.themeName, value); }
        }

        /// <summary>
        /// Gets the Uri of the <see cref="IDal"/> from which the <see cref="Thing"/> represented in the view-model comes from
        /// </summary>
        public Uri IDalUri { get; private set; }

        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by the view-model
        /// </summary>
        public T Thing { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="IDisposable"/> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; private set; }
        
        /// <summary>
        /// Gets or sets the <see cref="MessageBusHandler"/>.
        /// </summary>
        public MessageBusHandler MessageBusHandler { get; private set; }

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
            if (this.isDisposed)
            {
                return;
            }

            if (disposing) //Free any other managed objects here
            {
                // Clear all property values that maybe have been set
                // when the class was instantiated
                this.RevisionNumber = 0;
                this.Thing = null;

                if (this.Disposables != null)
                {
                    foreach (var disposable in this.Disposables)
                    {
                        disposable.Dispose();
                    }
                }
                else
                {
                    logger.Trace("The Disposables collection of the {0} is null", this.GetType().Name);
                }
            }

            // Indicate that the instance has been disposed.
            this.isDisposed = true;
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected virtual void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            this.RevisionNumber = objectChange.ChangedThing.RevisionNumber;
        }
    }
}
