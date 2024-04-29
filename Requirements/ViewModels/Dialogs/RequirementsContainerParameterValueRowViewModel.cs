// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsContainerParameterValueRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.Dialogs
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Composition.ViewModels;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The row-view-model representing a row of the <see cref="ValueArray{T}"/> of a <see cref="SimpleParameterValue"/>
    /// </summary>
    public class RequirementsContainerParameterValueRowViewModel : CDP4CommonView.RequirementsContainerParameterValueRowViewModel, IValueSetRow
    {
        /// <summary>
        /// The Index of the <see cref="ValueArray{T}"/> in which the value is contained
        /// </summary>
        private readonly int valueIndex;
        
        /// <summary>
        /// Backing field for <see cref="IsReadOnly"/>
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// Backing field for <see cref="Manual"/>
        /// </summary>
        private object manual;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsContainerParameterValueRowViewModel"/> class
        /// </summary>
        /// <param name="simpleParameterValue">The <see cref="RequirementsContainerParameterValue"/> associated with this row </param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        /// <param name="valueIndex">The value index</param>
        /// <param name="isReadOnly">Indicates whether the row is read-only</param>
        public RequirementsContainerParameterValueRowViewModel(RequirementsContainerParameterValue simpleParameterValue, ISession session, IViewModelBase<Thing> containerViewModel, int valueIndex, bool isReadOnly)
            : base(simpleParameterValue, session, containerViewModel)
        {
            this.valueIndex = valueIndex;
            this.IsReadOnly = isReadOnly;
            this.PossibleScale = new ReactiveList<MeasurementScale>();
            this.SetValues();
            this.PopulatePossibleScale();
        }

        /// <summary>
        /// Gets or sets the value for this <see cref="SimpleParameterValue"/>
        /// </summary>
        public object Manual
        {
            get { return this.manual; }
            set { this.RaiseAndSetIfChanged(ref this.manual, value); }
        }

        /// <summary>
        /// Gets a value indicating whether this row is read-only
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
            private set { this.RaiseAndSetIfChanged(ref this.isReadOnly, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="MeasurementScale"/> for this row
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScale { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the scale is editable
        /// </summary>
        /// <remarks>
        /// the scale is not editable for the values of a compound's components
        /// </remarks>
        public bool IsScaleReadOnly
        {
            get { return this.ParameterType is CompoundParameterType; }
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this row
        /// </summary>
        public ClassKind ParameterTypeClassKind { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="SimpleParameterValue"/> is a <see cref="EnumerationParameterType"/>
        /// </summary>
        public bool IsMultiSelect
        {
            get
            {
                var enumPt = this.Thing.ParameterType as EnumerationParameterType;
                if (enumPt != null)
                {
                    return enumPt.AllowMultiSelect;
                }

                var cpt = this.Thing.ParameterType as CompoundParameterType;
                if (cpt == null)
                {
                    return false;
                }

                enumPt = cpt.Component[this.valueIndex].ParameterType as EnumerationParameterType;
                if (enumPt == null)
                {
                    return false;
                }

                return enumPt.AllowMultiSelect;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the editor shall be visible
        /// </summary>
        public bool IsValueSetEditorActive
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the list of possible <see cref="EnumerationValueDefinition"/> for this <see cref="Parameter"/>
        /// </summary>
        public ReactiveList<EnumerationValueDefinition> EnumerationValueDefinition
        {
            get
            {
                var enumValues = new ReactiveList<EnumerationValueDefinition>();
                if (this.Thing == null)
                {
                    return enumValues;
                }

                var enumPt = this.Thing.ParameterType as EnumerationParameterType;
                if (enumPt != null)
                {
                    enumValues.AddRange(enumPt.ValueDefinition);
                    return enumValues;
                }

                var cpt = this.Thing.ParameterType as CompoundParameterType;
                if (cpt != null)
                {
                    enumPt = cpt.Component[this.valueIndex].ParameterType as EnumerationParameterType;
                    if (enumPt != null)
                    {
                        enumValues.AddRange(enumPt.ValueDefinition);
                    }
                }

                return enumValues;
            }
        }


        /// <summary>
        /// Set the Values of this row
        /// </summary>
        private void SetValues()
        {
            if (this.Thing.ParameterType == null)
            {
                return;
            }

            var cptPt = this.Thing.ParameterType as CompoundParameterType;
            if (cptPt == null)
            {
                this.ParameterType = this.Thing.ParameterType;
                this.Scale = this.Thing.Scale;
            }
            else
            {
                var cpt = cptPt.Component[this.valueIndex];
                this.ParameterType = cpt.ParameterType;
                this.Scale = cpt.Scale;
            }

            this.ParameterTypeName = this.ParameterType.Name;
            this.ParameterTypeClassKind = this.ParameterType.ClassKind;
            this.Manual = this.Thing.Value.Count() > this.valueIndex ? this.Thing.Value[this.valueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
        }

        /// <summary>
        /// Populates the possible scale
        /// </summary>
        private void PopulatePossibleScale()
        {
            var quantityKind = this.Thing.ParameterType as QuantityKind;
            if (quantityKind == null && this.Scale != null)
            {
                this.PossibleScale.Add(this.Scale);
            }
            else if (quantityKind != null)
            {
                this.PossibleScale.AddRange(quantityKind.AllPossibleScale.OrderBy(x => x.Name));
            }
        }
    }
}