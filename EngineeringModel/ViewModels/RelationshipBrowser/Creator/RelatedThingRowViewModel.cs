// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedThingRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The row-view-model used to represent related things for a <see cref="MultiRelationship"/> in the <see cref="MultiRelationshipCreatorViewModel"/> 
    /// </summary>
    public class RelatedThingRowViewModel : ReactiveObject, IDisposable
    {
        /// <summary>
        /// Backing field for <see cref="Denomination"/>
        /// </summary>
        private string denomination;

        /// <summary>
        /// Backing field for <see cref="ClassKind"/>
        /// </summary>
        private string classKind;

        /// <summary>
        /// The collection of <see cref="IDisposable"/>
        /// </summary>
        private readonly List<IDisposable> Subscriptions = new List<IDisposable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipRelatedThingRowViewModel"/> class
        /// </summary>
        /// <param name="thing">The associated <see cref="Thing"/></param>
        /// <param name="callBack">The <see cref="Action"/> to call back</param>
        public RelatedThingRowViewModel(Thing thing, Action<RelatedThingRowViewModel> callBack)
        {
            this.Thing = thing;

            var subscriber = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
                .Where(msg => msg.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Subscriptions.Add(subscriber);

            this.RemoveRelatedThingCommand = ReactiveCommand.Create();
            var commandDsubscriber = this.RemoveRelatedThingCommand.Subscribe(x => callBack(this));
            this.Subscriptions.Add(commandDsubscriber);

            this.SetProperties();
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> represented by the view-model
        /// </summary>
        public Thing Thing { get; private set; }

        /// <summary>
        /// Gets the command to remove the current represented <see cref="Thing"/> from the related things
        /// </summary>
        public ReactiveCommand<object> RemoveRelatedThingCommand { get; private set; }

        /// <summary>
        /// Gets the human-readable string that represents the current <see cref="Thing"/>
        /// </summary>
        public string Denomination
        {
            get { return this.denomination; }
            set { this.RaiseAndSetIfChanged(ref this.denomination, value); }
        }

        /// <summary>
        /// Gets the class-kind of the current <see cref="Thing"/>
        /// </summary>
        public string ClassKind
        {
            get { return this.classKind; }
            set { this.RaiseAndSetIfChanged(ref this.classKind, value); }
        }

        /// <summary>
        /// Disposes of this view-model
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in this.Subscriptions)
            {
                subscription.Dispose();
            }
        }

        /// <summary>
        /// Set the properties of the current view-model
        /// </summary>
        private void SetProperties()
        {
            this.Denomination = this.Thing.UserFriendlyName;
            this.ClassKind = this.Thing.ClassKind.ToString();
        }
    }
}