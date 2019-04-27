// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// A row view model that represents a <see cref="ParameterType"/>
    /// </summary>
    public class ParameterTypeRowViewModel : CDP4CommonView.ParameterTypeRowViewModel<ParameterType>, IDropTarget
    {
        /// <summary>
        /// The subscription for the default scale
        /// </summary>
        private IDisposable defaultScaleSubscription;

        /// <summary>
        /// The default <see cref="MeasurementScale"/> if the <see cref="ParameterType"/> is a <see cref="QuantityKind"/>
        /// </summary>
        private MeasurementScale defaultScaleObject;

        /// <summary>
        /// Backing field for the <see cref="DefaultScale"/> property
        /// </summary>
        private string defaultScale;

        /// <summary>
        /// Backing field for the <see cref="ContainerRdl"/> property
        /// </summary>
        private string containerRdl;

        /// <summary>
        /// Backing field for the is base quantity kind.
        /// </summary>
        private bool isBaseQuantityKind;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeRowViewModel"/> class.
        /// </summary>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is represented by the current view-model
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParameterTypeRowViewModel(ParameterType parameterType, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterType, session, containerViewModel)
        {
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Gets or sets the default scale of the referenced <see cref="ParameterType"/>
        /// </summary>
        public string DefaultScale
        {
            get
            {
                return this.defaultScale;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.defaultScale, value);
            }
        }

        /// <summary>
        /// Gets or sets the Container <see cref="ReferenceDataLibrary"/>.
        /// </summary>
        public string ContainerRdl
        {
            get
            {
                return this.containerRdl;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.containerRdl, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> that is represented by the current row-view-model
        /// </summary>
        public string Type
        {
            get
            {
                return this.Thing.ClassKind.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="QuantityKind"/> that is represented by the current row-view-model is base
        /// </summary>
        public bool IsBaseQuantityKind
        {
            get
            {
                return this.isBaseQuantityKind;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isBaseQuantityKind, value);
            }
        }

        /// <summary>
        /// Updates the properties of the current view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CreateDefaultScaleUpdateSubscription();
            var container = this.Thing.Container as ReferenceDataLibrary;
            this.ContainerRdl = container == null ? string.Empty : container.ShortName;
            var quantityKind = this.Thing as QuantityKind;
            this.IsBaseQuantityKind = container != null && (quantityKind != null && container.BaseQuantityKind.Contains(quantityKind));
        }

        /// <summary>
        /// Create the default scale update subscription
        /// </summary>
        private void CreateDefaultScaleUpdateSubscription()
        {
            var quantityKind = this.Thing as QuantityKind;
            if (quantityKind == null)
            {
                return;
            }

            if (this.defaultScaleObject != null && quantityKind.DefaultScale == this.defaultScaleObject)
            {
                return;
            }

            if (this.defaultScaleSubscription != null)
            {
                this.defaultScaleSubscription.Dispose();
            }

            this.defaultScaleObject = quantityKind.DefaultScale;
            this.defaultScaleSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.defaultScaleObject)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => { this.DefaultScale = this.defaultScaleObject.ShortName; });

            this.DefaultScale = this.defaultScaleObject.ShortName;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.defaultScaleSubscription != null)
            {
                this.defaultScaleSubscription.Dispose();
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        public override void StartDrag(IDragInfo dragInfo)
        {
            var quantityKind = this.Thing as QuantityKind;
            var scale = quantityKind != null ? quantityKind.DefaultScale : null;

            var payload = new Tuple<ParameterType, MeasurementScale>(this.Thing, scale);
            dragInfo.Payload = payload;
            dragInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Category category)
            {
                dropInfo.Effects = CategoryApplicationValidationService.ValidateDragDrop(this.Session.PermissionService, this.Thing, category, logger);
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var category = dropInfo.Payload as Category;
            if (category != null)
            {
                await this.Drop(category);
                return;
            }
        }

        /// <summary>
        /// Handles the drop action of a <see cref="Category"/>
        /// </summary>
        /// <param name="category">The dropped <see cref="Category"/></param>
        private async Task Drop(Category category)
        {
            var clone = this.Thing.Clone(false);
            clone.Category.Add(category);
            await this.DalWrite(clone);
        }
    }
}
