// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleParameterValueRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-COMET-IME Community Edition. 
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="SimpleParameterValue"/> row view model.
    /// </summary>
    public class SimpleParameterValueRowViewModel : CDP4CommonView.SimpleParameterValueRowViewModel, IDeprecatableThing, IValueSetRow
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
        /// Backing field for <see cref="Value"/> property.
        /// </summary>
        private object value;
        
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="IsValueSetEditorActive"/> property.
        /// </summary>
        private bool isValueSetEditorActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterValueRowViewModel"/> class
        /// </summary>
        /// <param name="simpleParameterValue">The <see cref="SimpleParameterValue"/> associated with this row </param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public SimpleParameterValueRowViewModel(SimpleParameterValue simpleParameterValue, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(simpleParameterValue, session, containerViewModel)
        {
            this.PossibleScales = new ReactiveList<MeasurementScale>();
            this.UpdateProperties();

            this.WhenAnyValue(x => x.Scale).Where(x => x != null).Subscribe(
                x =>
                {
                    if (!this.Thing.IsCached())
                    {
                        this.Thing.Scale = x;
                    }
                });
        }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this <see cref="IValueSetRow"/>
        /// </summary>
        public ClassKind ParameterTypeClassKind { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="Parameter"/> is a <see cref="EnumerationParameterType"/>
        /// </summary>
        public bool IsMultiSelect
        {
            get
            {
                if (this.Thing.ParameterType is EnumerationParameterType enumPt)
                {
                    return enumPt.AllowMultiSelect;
                }

                if (this.Thing.ParameterType is not CompoundParameterType cpt)
                {
                    return false;
                }

                enumPt = cpt.Component[0].ParameterType as EnumerationParameterType;

                if (enumPt == null)
                {
                    return false;
                }

                return enumPt.AllowMultiSelect;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the value set editors are active
        /// </summary>
        public bool IsValueSetEditorActive
        {
            get { return this.isValueSetEditorActive; }
            set { this.RaiseAndSetIfChanged(ref this.isValueSetEditorActive, value); }
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
            get { return this.Scale != null ? $"{this.shortName} [{this.Scale.ShortName}]" : this.shortName; }
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
        /// Gets or sets Value column
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
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
        /// Gets the list of possible <see cref="EnumerationValueDefinition"/> for this <see cref="Parameter"/>
        /// </summary>
        public ReactiveList<EnumerationValueDefinition> EnumerationValueDefinition
        {
            get
            {
                var enumValues = new ReactiveList<EnumerationValueDefinition>();

                if (this.ParameterType is EnumerationParameterType enumPt)
                {
                    enumValues.AddRange(enumPt.ValueDefinition);
                }

                return enumValues;
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
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            if (this.ContainerViewModel is RequirementRowViewModel requirementRowViewModel)
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
            if (this.ContainerViewModel is RequirementRowViewModel requirementRowViewModel)
            {
                this.IsDeprecated = requirementRowViewModel.IsDeprecated;
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">The name of the property whose error message to get</param>
        /// <param name="newValue">The new value for the row</param>
        /// <returns>The error message for the property. The default is an empty string ("").</returns>
        /// <remarks>
        /// Used when inline-editing, the values are updated on focus lost
        /// </remarks>
        public override string ValidateProperty(string columnName, object newValue)
        {
            if (columnName == "Value")
            {
                return ParameterValueValidator.Validate(newValue, this.Thing.ParameterType, this.Scale);
            }

            return null;
        }

        /// <summary>
        /// Creates a clone to write it on the data-source when inline-editing with a new value for one of its property
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="fieldName">The property name</param>
        public override void CreateCloneAndWrite(object newValue, string fieldName)
        {
            var clone = this.Thing.Clone(false);
            clone.Value = new ValueArray<string>([this.Value.ToValueSetString(this.ParameterType)]);

            this.EndInlineEdit(clone);
        }

        /// <summary>
        /// Returns a value from a valueset that is usefull for edittable values
        /// </summary>
        /// <param name="valueArray">The <see cref="ValueArray{string}"/></param>
        /// <returns>ValueSet value as an object, corresponding to the correct <see cref="ParameterType"/>, which is handled by a template selector</returns>
        private object GetObjectDisplayFromValueSet(ValueArray<string> valueArray)
        {
            if (valueArray.Count > 1)
            {
                return null;
            }

            if (valueArray.Count == 1)
            {
                return valueArray.First().ToValueSetObject(this.ParameterType);
            }

            return ValueSetConverter.DefaultObject(this.ParameterType);
        }

        /// <summary>
        /// Updates the properties.
        /// </summary>
        private void UpdateProperties()
        {
            this.ParameterTypeClassKind = this.ParameterType.ClassKind;

            this.Name = this.Thing.ParameterType.Name;
            this.ShortName = this.Thing.ParameterType.ShortName;

            if (this.ParameterType != null)
            {
                this.ParameterTypeName = this.ParameterType.Name;
                this.ParameterTypeShortName = this.ParameterType.ShortName;
            }

            if (this.ParameterType is QuantityKind quantityKind)
            {
                this.PossibleScales.Clear();
                this.PossibleScales.AddRange(quantityKind.AllPossibleScale.OrderBy(x => x.Name));

                this.Scale ??= this.PossibleScales.SingleOrDefault(x => x == quantityKind.DefaultScale) ?? this.PossibleScales.FirstOrDefault();
            }

            if (this.Scale != null)
            {
                this.ScaleName = this.Scale.Name;
                this.ScaleShortName = this.Scale.ShortName;
            }

            if (this.Thing.ParameterType is CompoundParameterType)
            {
                this.IsValueSetEditorActive = false;
            }
            else
            {
                this.IsValueSetEditorActive = true;
                this.Value = this.GetObjectDisplayFromValueSet(this.Thing.Value);
            }

            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
        }
    }
}