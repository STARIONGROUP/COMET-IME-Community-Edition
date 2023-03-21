// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using CommonServiceLocator;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// The Base row-class for <see cref="ParameterBase"/>
    /// </summary>
    /// <typeparam name="T">A <see cref="ParameterBase"/> type</typeparam>
    public abstract class ParameterBaseRowViewModel<T> : CDP4CommonView.ParameterBaseRowViewModel<T>, IDialogValueSetRow where T : ParameterBase
    {
        /// <summary>
        /// The manual field name
        /// </summary>
        private const string ManualPropertyName = "Manual";

        /// <summary>
        /// The reference field name
        /// </summary>
        private const string ReferencePropertyName = "Reference";

        /// <summary>
        /// Backing field for <see cref="Formula"/>
        /// </summary>
        private string formula;

        /// <summary>
        /// A value indicating whether the dialog is read-only
        /// </summary>
        private readonly bool isDialogReadOnly;

        /// <summary>
        /// The <see cref="IDialogViewModelBase{T}"/> context
        /// </summary>
        private readonly IDialogViewModelBase<T> dialogViewModel;

        /// <summary>
        /// Backing field for <see cref="LoggerFactory"/> 
        /// </summary>
        private ILoggerFactory loggerFactory;

        /// <summary>
        /// The INJECTED <see cref="ILoggerFactory"/> 
        /// </summary>
        protected ILoggerFactory LoggerFactory
        {
            get
            {
                if (this.loggerFactory == null)
                {
                    this.loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
                }

                return this.loggerFactory;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBaseRowViewModel{T}"/> class. 
        /// </summary>
        /// <param name="parameterBase">
        /// The associated <see cref="ParameterBase"/>
        /// </param>
        /// <param name="session">
        /// The associated <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The <see cref="ElementBase{T}"/> row that contains this row.
        /// </param>
        /// <param name="isDialogReadOnly">
        /// Value indicating whether this row should be read-only because the dialog is read-only
        /// </param>
        protected ParameterBaseRowViewModel(T parameterBase, ISession session, IDialogViewModelBase<T> containerViewModel, bool isDialogReadOnly)
            : base(parameterBase, session, containerViewModel)
        {
            this.dialogViewModel = containerViewModel;
            this.isCompoundType = this.Thing.ParameterType is CompoundParameterType;
            this.ParameterType = this.Thing.ParameterType;
            this.ParameterTypeClassKind = this.Thing.ParameterType.ClassKind;

            this.isDialogReadOnly = isDialogReadOnly;
            this.WhenAnyValue(vm => vm.Switch).Subscribe(_ => this.UpdateActualValue());
            this.WhenAnyValue(vm => vm.Manual).Subscribe(_ => this.UpdateActualValue());
            this.WhenAnyValue(vm => vm.Computed).Subscribe(_ => this.UpdateActualValue());
            this.WhenAnyValue(vm => vm.Reference).Subscribe(_ => this.UpdateActualValue());

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="Parameter"/> is a <see cref="EnumerationParameterType"/>
        /// </summary>
        public bool IsMultiSelect
        {
            get
            {
                var enumPt = this.ParameterType as EnumerationParameterType;

                if (enumPt == null)
                {
                    return false;
                }

                return enumPt.AllowMultiSelect;
            }
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
        /// Gets or sets the Formula column value
        /// </summary>
        public string Formula
        {
            get => this.formula;
            set => this.RaiseAndSetIfChanged(ref this.formula, value);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ParameterBase"/> is a <see cref="CompoundParameterType"/>
        /// </summary>
        protected bool isCompoundType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this <see cref="IValueSetRow"/>
        /// </summary>
        public ClassKind ParameterTypeClassKind { get; protected set; }

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
                    var rule = this.dialogViewModel.ValidationErrors.SingleOrDefault(x => x.PropertyName == ManualPropertyName);

                    if (rule != null)
                    {
                        this.dialogViewModel.ValidationErrors.Remove(rule);
                    }

                    var validationMsg = ParameterValueValidator.Validate(this.Manual, this.ParameterType, this.Scale, this.LoggerFactory);

                    if (!string.IsNullOrWhiteSpace(validationMsg))
                    {
                        var validationRule = new ValidationService.ValidationRule
                        {
                            ErrorText = validationMsg,
                            PropertyName = ManualPropertyName
                        };

                        this.dialogViewModel.ValidationErrors.Add(validationRule);
                        return validationMsg;
                    }

                    return validationMsg;
                }

                if (columnName == ReferencePropertyName)
                {
                    var rule = this.dialogViewModel.ValidationErrors.SingleOrDefault(x => x.PropertyName == ReferencePropertyName);

                    if (rule != null)
                    {
                        this.dialogViewModel.ValidationErrors.Remove(rule);
                    }

                    var validationMsg = ParameterValueValidator.Validate(this.Reference, this.ParameterType, this.Scale, this.LoggerFactory);

                    if (!string.IsNullOrWhiteSpace(validationMsg))
                    {
                        var validationRule = new ValidationService.ValidationRule
                        {
                            ErrorText = validationMsg,
                            PropertyName = ReferencePropertyName
                        };

                        this.dialogViewModel.ValidationErrors.Add(validationRule);
                        return validationMsg;
                    }

                    return validationMsg;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Update this ParameterBase row and its child nodes
        /// </summary>
        private void UpdateProperties()
        {
            this.Name = this.Thing.ParameterType.Name;

            if (this.Thing is ParameterSubscription subscription)
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

            this.ClearValues();

            this.ContainedRows.ClearAndDispose();

            if (this.Thing.IsOptionDependent)
            {
                this.SetOptionProperties();
            }
            else if (this.Thing.StateDependence != null)
            {
                this.SetStateProperties(this, null);
            }
            else if (this.isCompoundType)
            {
                this.SetComponentProperties(this, null, null);
            }
            else
            {
                this.SetProperties();
            }
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
                    break;
            }
        }

        /// <summary>
        /// Sets the option dependent rows contained in this row.
        /// </summary>
        private void SetOptionProperties()
        {
            var iteration = this.Thing.GetContainerOfType<Iteration>();

            if (iteration == null)
            {
                throw new InvalidOperationException("No Iteration Container were found.");
            }

            foreach (Option optionAvailable in iteration.Option)
            {
                var row = new ParameterOptionRowViewModel(this.Thing, optionAvailable, this.Session, this, this.isDialogReadOnly);

                if (this.Thing.StateDependence != null)
                {
                    this.SetStateProperties(row, optionAvailable);
                }
                else if (this.isCompoundType)
                {
                    this.SetComponentProperties(row, optionAvailable, null);
                }
                else
                {
                    row.UpdateValues();
                }

                this.ContainedRows.Add(row);
            }
        }

        /// <summary>
        /// Sets the state dependent rows contained in this row.
        /// </summary>
        private void SetStateProperties(IRowViewModelBase<Thing> row, Option actualOption)
        {
            var stateList = this.Thing.StateDependence;

            foreach (var state in stateList.ActualState.Where(s => s.Kind != ActualFiniteStateKind.FORBIDDEN))
            {
                var stateRow = new ParameterStateRowViewModel(this.Thing, actualOption, state, this.Session, row, this.isDialogReadOnly);

                if (this.Thing.ParameterType is CompoundParameterType)
                {
                    this.SetComponentProperties(stateRow, actualOption, state);
                }
                else
                {
                    stateRow.UpdateValues();
                }

                row.ContainedRows.Add(stateRow);
            }
        }

        /// <summary>
        /// Creates the component rows for this <see cref="CompoundParameterType"/> <see cref="ParameterRowViewModel"/>.
        /// </summary>
        private void SetComponentProperties(IRowViewModelBase<Thing> row, Option actualOption, ActualFiniteState actualState)
        {
            for (var i = 0; i < ((CompoundParameterType)this.Thing.ParameterType).Component.Count; i++)
            {
                var componentRow = new ParameterComponentValueRowViewModel(this.Thing, i, this.Session, actualOption, actualState, row, this.isDialogReadOnly);
                componentRow.UpdateValues();
                row.ContainedRows.Add(componentRow);
            }
        }

        /// <summary>
        /// Clear the values
        /// </summary>
        private void ClearValues()
        {
            this.Manual = null;
            this.Reference = null;
            this.Value = null;
            this.Formula = null;
            this.Computed = null;
            this.Switch = null;
            this.Scale = null;
        }

        /// <summary>
        /// Sets the values of this row in case where the <see cref="ParameterBase"/> is neither option-dependent nor state-dependent and is a <see cref="ScalarParameterType"/>
        /// </summary>
        public abstract void SetProperties();
    }
}
