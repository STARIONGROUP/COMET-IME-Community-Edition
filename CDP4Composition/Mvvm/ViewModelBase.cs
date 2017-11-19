// ------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using CDP4Common;
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
    public abstract class ViewModelBase<T> : ReactiveObject, IViewModelBase<T>, IDisposable, IISession where T : Thing
    {
        #region Fields

        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger logger;
        
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
        private bool isBusy;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{T}"/> class.
        /// </summary>
        protected ViewModelBase()
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
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
            logger = LogManager.GetLogger(this.GetType().FullName);
            this.ThemeName = AppliedTheme.ThemeName;
            
            this.PermissionService = session.PermissionService;
            this.Disposables = new List<IDisposable>();
            this.Thing = thing;            
            this.Session = session;

            this.RevisionNumber = thing.RevisionNumber;
            this.IDalUri = thing.IDalUri;

            var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.ObjectChangeEventHandler);
            this.Disposables.Add(thingSubscription);
        }
        #endregion

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
        public bool IsBusy
        {
            get { return this.isBusy; }
            protected set { this.RaiseAndSetIfChanged(ref this.isBusy, value); }
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
