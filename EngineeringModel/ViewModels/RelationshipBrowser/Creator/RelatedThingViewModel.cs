// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedThingViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.DragDrop;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The view-model representing a related thing for a <see cref="BinaryRelationship"/>
    /// </summary>
    public class RelatedThingViewModel : ReactiveObject, IDropTarget, IDisposable
    {
        /// <summary>
        /// Backing field for <see cref="RelatedThing"/>
        /// </summary>
        private Thing relatedThing;

        /// <summary>
        /// Backing field for <see cref="RelatedThingDenomination"/>
        /// </summary>
        private string relatedThingDenomination;

        /// <summary>
        /// The collection of <see cref="IDisposable"/>
        /// </summary>
        private IDisposable subscription;

        /// <summary>
        /// Gets the source <see cref="Thing"/> for the <see cref="BinaryRelationship"/> to create
        /// </summary>
        public Thing RelatedThing
        {
            get { return this.relatedThing; }
            private set { this.RaiseAndSetIfChanged(ref this.relatedThing, value); }
        }

        /// <summary>
        /// Gets the string to display associated with the <see cref="RelatedThing"/>
        /// </summary>
        public string RelatedThingDenomination
        {
            get { return this.relatedThingDenomination; }
            set { this.RaiseAndSetIfChanged(ref this.relatedThingDenomination, value); }
        }

        /// <summary>
        /// Reset the control
        /// </summary>
        public void ResetControl()
        {
            this.RelatedThing = null;
            this.RelatedThingDenomination = "";
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///   Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Data"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var thing = dropInfo.Payload as Thing;
            if (thing != null && thing.TopContainer is EngineeringModel)
            {
                dropInfo.Effects = DragDropEffects.All;
            }
        }

        /// <summary>
        /// Performs a drop.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drop.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var thing = dropInfo.Payload as Thing;
            if (thing == null)
            {
                this.RelatedThingDenomination = "";
                return;
            }

            this.Dispose();

            this.RelatedThing = thing;
            this.RelatedThingDenomination = string.Format("({0}) {1}", thing.ClassKind, thing.UserFriendlyName);

            this.subscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.RelatedThing)
                .Where(msg => msg.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.RelatedThingDenomination = string.Format("({0}) {1}", thing.ClassKind, thing.UserFriendlyName));
        }

        /// <summary>
        /// dispose of this view-model
        /// </summary>
        public void Dispose()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
            }
        }
    }
}