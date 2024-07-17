// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueBaseRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using CDP4Composition.Mvvm;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The base row view-model that displays the value-set of a <see cref="ParameterBase"/> 
    /// when its type is not a <see cref="ScalarParameterType"/> and it is not option and scale dependent
    /// </summary>
    public abstract class ParameterValueBaseRowViewModel : CDP4CommonView.ParameterBaseRowViewModel<ParameterBase>, IDialogValueSetRow
    {
        /// <summary>
        /// The manual field name
        /// </summary>
        protected const string ManualPropertyName = "Manual";

        /// <summary>
        /// The reference field name
        /// </summary>
        protected const string ReferencePropertyName = "Reference";

        /// <summary>
        /// Backing field for <see cref="Formula"/>
        /// </summary>
        private string formula;

        /// <summary>
        /// The Index of the <see cref="ValueArray"/> in which the value is contained
        /// </summary>
        protected readonly int ValueIndex;

        /// <summary>
        /// The <see cref="Option"/> associated with this row if the <see cref="ParameterBase"/> is option-dependent
        /// </summary>
        public readonly Option ActualOption;

        /// <summary>
        /// The <see cref="ActualFiniteState"/> associated with this row if the <see cref="ParameterBase"/> is state-dependent
        /// </summary>
        public readonly ActualFiniteState ActualState;

        /// <summary>
        /// A value that indicates whether the value set's listener is initialized
        /// </summary>
        private bool isValueSetListenerInitialized;

        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// The row code
        /// </summary>
        /// <remarks>
        /// This represent the row code used to identify the current row in the validation errors list
        /// </remarks>
        protected readonly string RowCode;

        /// <summary>
        /// The <see cref="IDialogViewModelBase{ParameterBase}"/> context
        /// </summary>
        protected readonly IDialogViewModelBase<ParameterBase> DialogViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueBaseRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">
        /// The associated <see cref="ParameterBase"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        /// <param name="actualOption">
        /// The actual <see cref="Option"/> represented if any
        /// </param>
        /// <param name="actualState">
        /// The actual <see cref="ActualFiniteState"/> represented if any
        /// </param>
        /// <param name="containerRow">
        /// The row container
        /// </param>
        /// <param name="valueIndex">
        /// The index of the component if applicable
        /// </param>
        /// <param name="isDialogReadOnly">
        /// Value indicating whether this row should be read-only because the dialog is read-only
        /// </param>
        protected ParameterValueBaseRowViewModel(ParameterBase parameterBase, ISession session, Option actualOption, ActualFiniteState actualState, IViewModelBase<Thing> containerRow, int valueIndex = 0, bool isDialogReadOnly = false)
            : base(parameterBase, session, containerRow)
        {
            this.ActualOption = actualOption;
            this.ActualState = actualState;
            this.ValueIndex = valueIndex;
            this.ParameterTypeClassKind = this.ParameterType.ClassKind;

            this.WhenAnyValue(vm => vm.Switch).Skip(1).Subscribe(_ => this.UpdateActualValue());
            this.WhenAnyValue(vm => vm.Manual).Skip(1).Subscribe(_ => this.UpdateActualValue());
            this.WhenAnyValue(vm => vm.Computed).Skip(1).Subscribe(_ => this.UpdateActualValue());
            this.WhenAnyValue(vm => vm.Reference).Skip(1).Subscribe(_ => this.UpdateActualValue());

            var option = this.ActualOption == null ? string.Empty : this.ActualOption.ShortName;
            var state = this.ActualState == null ? string.Empty : this.ActualState.ShortName;
            this.RowCode = string.Format("{0}.{1}.{2}", option, state, this.ValueIndex);

            this.DialogViewModel = (IDialogViewModelBase<ParameterBase>)this.TopContainerViewModel;

            var subscription = this.Thing as ParameterSubscription;

            if (subscription != null)
            {
                var parameter = (ParameterOrOverrideBase)subscription.Container;

                if (parameter.Owner != null)
                {
                    this.OwnerName = "[" + parameter.Owner.Name + "]";
                    this.OwnerShortName = "[" + parameter.Owner.ShortName + "]";
                }
            }
            else
            {
                if (this.Owner != null)
                {
                    this.OwnerName = this.Owner.Name;
                    this.OwnerShortName = this.Owner.ShortName;
                }
            }
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

                enumPt = cpt.Component[this.ValueIndex].ParameterType as EnumerationParameterType;

                if (enumPt == null)
                {
                    return false;
                }

                return enumPt.AllowMultiSelect;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ActualFiniteState"/> associated with this row is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get => this.isDefault;
            set => this.RaiseAndSetIfChanged(ref this.isDefault, value);
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
                    enumPt = cpt.Component[this.ValueIndex].ParameterType as EnumerationParameterType;

                    if (enumPt != null)
                    {
                        enumValues.AddRange(enumPt.ValueDefinition);
                    }
                }

                return enumValues;
            }
        }

        /// <summary>
        /// Gets or sets the Formula column value
        /// </summary>
        public string Formula
        {
            get => this.formula;
            set => this.RaiseAndSetIfChanged(ref this.formula, value);
        }

        /// <summary>
        /// Check that the values of this row are valid
        /// </summary>
        /// <param name="scale">The <see cref="MeasurementScale"/></param>
        public virtual void CheckValues(MeasurementScale scale)
        {
            this.Scale = scale;

            if (this.ContainedRows.Count == 0)
            {
                this.RaisePropertyChanged(ManualPropertyName);
                this.RaisePropertyChanged(ReferencePropertyName);
                return;
            }

            foreach (IDialogValueSetRow row in this.ContainedRows)
            {
                row.CheckValues(scale);
            }
        }

        /// <summary>
        /// Set the Values of this row
        /// </summary>
        /// <remarks>
        /// This method update the values of this view-model. This is called by the container that manages its value-set 
        /// i.e. <see cref="ParameterRowViewModel"/>, <see cref="ParameterOverrideRowViewModel"/>, <see cref="ParameterSubscriptionRowViewModel"/> through their super-type <see cref="ParameterBaseRowViewModel{T}"/>
        /// </remarks>
        public virtual void UpdateValues()
        {
            if (this.Thing is ParameterSubscription)
            {
                this.SetParameterSubscriptionValues();
            }
            else
            {
                this.SetParameterOrOverrideValues();
            }

            this.ScaleShortName = this.Thing.Scale == null ? "-" : this.Thing.Scale.ShortName;
        }

        /// <summary>
        /// Update the actual value
        /// </summary>
        private void UpdateActualValue()
        {
            if (this.Switch == null)
            {
                return;
            }

            switch (this.Switch)
            {
                case ParameterSwitchKind.COMPUTED:
                    this.Value = this.Computed;
                    break;
                case ParameterSwitchKind.MANUAL:
                    this.Value = this.Manual.ToValueSetString(this.ParameterType);
                    break;
                case ParameterSwitchKind.REFERENCE:
                    this.Value = this.Reference.ToValueSetString(this.ParameterType);
                    break;
                default:
                    this.Value = this.Reference.ToValueSetString(this.ParameterType);
                    logger.Error("There is a problem in the Parameter switch value in the {0} with id {1}.\nThe Reference value is used.", this.Thing.ClassKind, this.Thing.Iid);
                    break;
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        /// <remarks>
        /// Used by the view through the IDataErrorInfo interface to validate a field onPropertyChanged
        /// </remarks>
        public override string this[string columnName]
        {
            get
            {
                if (columnName == ManualPropertyName)
                {
                    var propertyCode = this.RowCode + ManualPropertyName;

                    var rule = this.DialogViewModel.ValidationErrors.SingleOrDefault(x => x.PropertyName == propertyCode);

                    if (rule != null)
                    {
                        this.DialogViewModel.ValidationErrors.Remove(rule);
                    }

                    var validationMsg = ParameterValueValidator.Validate(this.Manual, this.ParameterType, this.Scale);

                    if (!string.IsNullOrWhiteSpace(validationMsg))
                    {
                        var validationRule = new ValidationService.ValidationRule
                        {
                            ErrorText = validationMsg,
                            PropertyName = propertyCode
                        };

                        this.DialogViewModel.ValidationErrors.Add(validationRule);
                        return validationMsg;
                    }

                    return validationMsg;
                }

                if (columnName == ReferencePropertyName)
                {
                    var propertyCode = this.RowCode + ReferencePropertyName;

                    var rule = this.DialogViewModel.ValidationErrors.SingleOrDefault(x => x.PropertyName == propertyCode);

                    if (rule != null)
                    {
                        this.DialogViewModel.ValidationErrors.Remove(rule);
                    }

                    var validationMsg = ParameterValueValidator.Validate(this.Reference, this.ParameterType, this.Scale);

                    if (!string.IsNullOrWhiteSpace(validationMsg))
                    {
                        var validationRule = new ValidationService.ValidationRule
                        {
                            ErrorText = validationMsg,
                            PropertyName = propertyCode
                        };

                        this.DialogViewModel.ValidationErrors.Add(validationRule);
                        return validationMsg;
                    }

                    return validationMsg;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Computes the entire row or specific property of the row is editable based on the
        /// result of the <see cref="PermissionService.CanWrite"/> method and potential
        /// conditions of the property of the Row that is being edited.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property for which the value is computed. This allows to include the
        /// specific property of the row-view-model in the computation. If the propertyname is empty
        /// then the whole row is taken into account. If a property is specified only that property
        /// is taken into account.
        /// </param>
        /// <returns>
        /// True if the row or more specific the property is editable or not.
        /// </returns>
        public override bool IsEditable(string propertyName = "")
        {
            if (this.ContainedRows.Any())
            {
                return false;
            }

            if (!string.IsNullOrEmpty(this.Error))
            {
                return false;
            }

            var parameterSubscription = this.Thing as ParameterSubscription;

            if (parameterSubscription != null && propertyName == "Reference")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the values of this row in the case where the represented thing is a <see cref="ParameterSubscription"/>
        /// </summary>
        private void SetParameterSubscriptionValues()
        {
            var parameterSubscription = this.Thing as ParameterSubscription;

            if (parameterSubscription == null)
            {
                return;
            }

            if (this.ContainedRows.Any())
            {
                return;
            }

            var valueSet = this.GetParameterSubscriptionValueSet();

            if (valueSet == null)
            {
                logger.Error("No Value set was found for the option: {0}, state: {1}", this.ActualOption == null ? "null" : this.ActualOption.Name, this.ActualState == null ? "null" : this.ActualState.Name);
                return;
            }

            this.Switch = valueSet.ValueSwitch;
            this.Computed = valueSet.Computed.Count() > this.ValueIndex ? valueSet.Computed[this.ValueIndex] : "-";
            this.Manual = valueSet.Manual.Count() > this.ValueIndex ? valueSet.Manual[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Reference = valueSet.Reference.Count() > this.ValueIndex ? valueSet.Reference[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Value = valueSet.ActualValue.Count() > this.ValueIndex ? valueSet.ActualValue[this.ValueIndex] : "-";
        }

        /// <summary>
        /// Sets the values of this row in the case where the represented thing is a <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        private void SetParameterOrOverrideValues()
        {
            if (this.ContainedRows.Any())
            {
                return;
            }

            ParameterValueSetBase valueSet;

            if (this.Thing is Parameter)
            {
                valueSet = this.GetParameterValueSet();
            }
            else
            {
                valueSet = this.GetParameterOverrideValueSet();
            }

            if (valueSet == null)
            {
                logger.Error("No Value set was found for the option: {0}, state: {1}", this.ActualOption == null ? "null" : this.ActualOption.Name, this.ActualState == null ? "null" : this.ActualState.Name);
                return;
            }

            this.Computed = valueSet.Computed.Count() > this.ValueIndex ? valueSet.Computed[this.ValueIndex] : "-";
            this.Manual = valueSet.Manual.Count() > this.ValueIndex ? valueSet.Manual[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Reference = valueSet.Reference.Count() > this.ValueIndex ? valueSet.Reference[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Value = valueSet.ActualValue.Count() > this.ValueIndex ? valueSet.ActualValue[this.ValueIndex] : "-";
            this.Formula = valueSet.Formula.Count() > this.ValueIndex ? valueSet.Formula[this.ValueIndex] : "-";
            this.State = valueSet.ActualState != null ? valueSet.ActualState.Name : "-";
            this.Option = valueSet.ActualOption;
            this.Switch = valueSet.ValueSwitch;
            this.Published = valueSet.Published.Count() > this.ValueIndex ? valueSet.Published[this.ValueIndex] : "-";
            this.IsDefault = valueSet.ActualState?.IsDefault ?? false;

            if (valueSet.Published.Count() <= this.ValueIndex)
            {
                this.Error = "No ValueSet found for this component";
            }
        }

        /// <summary>
        /// Gets the <see cref="ParameterOverrideValueSet"/> of this <see cref="ParameterOverride"/> if applicable
        /// </summary>
        /// <returns>The <see cref="ParameterOverrideValueSet"/></returns>
        private ParameterOverrideValueSet GetParameterOverrideValueSet()
        {
            var parameterOverride = (ParameterOverride)this.Thing;
            ParameterOverrideValueSet valueSet = null;

            if (this.ActualOption == null && this.ActualState == null)
            {
                return parameterOverride.ValueSet.FirstOrDefault();
            }

            if (this.ActualOption != null && this.ActualState == null)
            {
                valueSet = parameterOverride.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption);
            }

            if (this.ActualOption == null && this.ActualState != null)
            {
                valueSet = parameterOverride.ValueSet.FirstOrDefault(v => v.ActualState == this.ActualState);
            }

            if (this.ActualOption != null && this.ActualState != null)
            {
                valueSet = parameterOverride.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption && v.ActualState == this.ActualState);
            }

            return valueSet;
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSet"/> of this <see cref="Parameter"/> if applicable
        /// </summary>
        /// <returns>The <see cref="ParameterValueSet"/></returns>
        private ParameterValueSet GetParameterValueSet()
        {
            var parameter = (Parameter)this.Thing;

            if (this.ActualOption == null && this.ActualState == null)
            {
                return parameter.ValueSet.FirstOrDefault();
            }

            if (this.ActualOption != null && this.ActualState == null)
            {
                return parameter.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption);
            }

            if (this.ActualOption == null && this.ActualState != null)
            {
                return parameter.ValueSet.FirstOrDefault(v => v.ActualState == this.ActualState);
            }

            return parameter.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption && v.ActualState == this.ActualState);
        }

        /// <summary>
        /// Gets the <see cref="ParameterSubscriptionValueSet"/> of this <see cref="ParameterSubscription"/>
        /// </summary>
        /// <returns>The <see cref="ParameterSubscriptionValueSet"/></returns>
        private ParameterSubscriptionValueSet GetParameterSubscriptionValueSet()
        {
            var parameterSubscription = (ParameterSubscription)this.Thing;

            ParameterSubscriptionValueSet valueSet;

            if (this.ActualOption == null && this.ActualState == null)
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault();
            }
            else if (this.ActualOption != null && this.ActualState == null)
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption);
            }
            else if (this.ActualOption == null && this.ActualState != null)
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault(v => v.ActualState == this.ActualState);
            }
            else
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption && v.ActualState == this.ActualState);
            }

            return valueSet;
        }
    }
}
