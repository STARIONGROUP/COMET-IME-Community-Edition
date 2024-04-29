﻿// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipParameterValueRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    
    /// <summary>
    /// The simple parameter value row view model.
    /// </summary>
    public class RelationshipParameterValueRowViewModel : CDP4CommonView.RelationshipParameterValueRowViewModel, IDeprecatableThing
    {
        /// <summary>
        /// Backing field for <see cref="Definition"/> property.
        /// </summary>
        private string definition;

        /// <summary>
        /// Backing field for <see cref="Name"/> property.
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/> property.
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipParameterValueRowViewModel"/> class
        /// </summary>
        /// <param name="parameterValue">The <see cref="RelationshipParameterValue"/> associated with this row </param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public RelationshipParameterValueRowViewModel(RelationshipParameterValue parameterValue, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterValue, session, containerViewModel)
        {
            this.PossibleScales = new ReactiveList<MeasurementScale>();
            this.WhenAnyValue(x => x.Scale).Where(x => x != null).Subscribe(
                x =>
                {
                    if (!this.Thing.IsCached())
                    {
                        this.Thing.Scale = x;
                    }
                });
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the possible <see cref="MeasurementScale"/>
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScales { get; private set; }

        /// <summary>
        /// Gets or sets Name column
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets ShortName column
        /// </summary>
        public string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Definition"/>
        /// </summary>
        public string Definition
        {
            get { return this.definition; }
            set { this.RaiseAndSetIfChanged(ref this.definition, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
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
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var requirementRowViewModel = this.ContainerViewModel as RequirementRowViewModel;
            if (requirementRowViewModel != null)
            {
                var containerIsDeprecatedSubscription = requirementRowViewModel.WhenAnyValue(vm => vm.IsDeprecated)
                .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());
                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="RequirementRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            var requirementRowViewModel = this.ContainerViewModel as RequirementRowViewModel;
            if (requirementRowViewModel != null)
            {
                this.IsDeprecated = requirementRowViewModel.IsDeprecated;
            }            
        }

        /// <summary>
        /// Updates the properties.
        /// </summary>
        private void UpdateProperties()
        {
            this.Name = this.Thing.ParameterType.Name;
            this.ShortName = this.Thing.ParameterType.ShortName;
            this.Definition = string.Join(", ", this.Thing.Value);

            if (this.ParameterType != null)
            {
                this.ParameterTypeName = this.ParameterType.Name;
                this.ParameterTypeShortName = this.ParameterType.ShortName;
            }

            var quantityKind = this.ParameterType as QuantityKind;
            if (quantityKind != null)
            {
                this.PossibleScales.Clear();
                this.PossibleScales.AddRange(quantityKind.AllPossibleScale.OrderBy(x => x.Name));

                if (this.Scale == null)
                {
                    this.Scale = this.PossibleScales.SingleOrDefault(x => x == quantityKind.DefaultScale) ?? this.PossibleScales.FirstOrDefault();
                }
            }

            if (this.Scale != null)
            {
                this.ScaleName = this.Scale.Name;
                this.ScaleShortName = this.Scale.ShortName;
            }

            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
        }
    }
}