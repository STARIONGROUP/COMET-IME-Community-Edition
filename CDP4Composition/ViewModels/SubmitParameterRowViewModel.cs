// -------------------------------------------------------------------------------------------------
// <copyright file="SubmitParameterRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Validation;

    using ReactiveUI;

    /// <summary>
    /// Represents a row that shows a parameter that has changed, and the new value
    /// </summary>
    public class SubmitParameterRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for the <see cref="IsSelected"/> property.
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitParameterRowViewModel"/> class.
        /// </summary>
        /// <param name="parameterValueSet">
        /// The <see cref="ParameterValueSet"/> that is represented by the current view model
        /// </param>
        /// <param name="componentIndex">
        /// The component index.
        /// </param>
        /// <param name="validationResult">
        /// The result of the Validation
        /// </param>
        public SubmitParameterRowViewModel(ParameterValueSet parameterValueSet, int componentIndex, ValidationResultKind validationResult)
        {
            this.Thing = parameterValueSet;
            this.IsSelected = true;
            this.ModelCode = parameterValueSet.ModelCode(componentIndex);
            this.ManualValue = parameterValueSet.Manual[componentIndex];
            this.ReferenceValue = parameterValueSet.Reference[componentIndex];
            this.FormulaValue = parameterValueSet.Formula[componentIndex];
            this.ComputedValue = parameterValueSet.Computed[componentIndex];
            this.ActualValue = parameterValueSet.ActualValue[componentIndex];
            this.Switch = parameterValueSet.ValueSwitch.ToString();
            this.HasValidationError = validationResult != ValidationResultKind.Valid;
            this.IsSelected = !this.HasValidationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitParameterRowViewModel"/> class.
        /// </summary>
        /// <param name="parameterOverrideValueSet">
        /// The <see cref="ParameterOverrideValueSet"/> that is represented by the current view model
        /// </param>
        /// <param name="componentIndex">
        /// The component index.
        /// </param>
        /// <param name="validationResult">
        /// The result of the Validation
        /// </param>
        public SubmitParameterRowViewModel(ParameterOverrideValueSet parameterOverrideValueSet, int componentIndex, ValidationResultKind validationResult)
        {
            this.Thing = parameterOverrideValueSet;
            this.IsSelected = true;
            this.ModelCode = parameterOverrideValueSet.ModelCode(componentIndex);
            this.ManualValue = parameterOverrideValueSet.Manual[componentIndex];
            this.ReferenceValue = parameterOverrideValueSet.Reference[componentIndex];
            this.FormulaValue = parameterOverrideValueSet.Formula[componentIndex];
            this.ComputedValue = parameterOverrideValueSet.Computed[componentIndex];
            this.ActualValue = parameterOverrideValueSet.ActualValue[componentIndex];
            this.Switch = parameterOverrideValueSet.ValueSwitch.ToString();
            this.HasValidationError = validationResult != ValidationResultKind.Valid;
            this.IsSelected = !this.HasValidationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitParameterRowViewModel"/> class.
        /// </summary>
        /// <param name="parameterSubscriptionValueSet">
        /// The <see cref="ParameterSubscriptionValueSet"/> that is represented by the current view model
        /// </param>
        /// <param name="componentIndex">
        /// The component index.
        /// </param>
        /// <param name="validationResult">
        /// The result of the Validation
        /// </param>
        public SubmitParameterRowViewModel(ParameterSubscriptionValueSet parameterSubscriptionValueSet, int componentIndex, ValidationResultKind validationResult)
        {
            this.Thing = parameterSubscriptionValueSet;
            this.IsSelected = true;
            this.ModelCode = parameterSubscriptionValueSet.ModelCode(componentIndex);
            this.ManualValue = parameterSubscriptionValueSet.Manual[componentIndex];
            this.ReferenceValue = parameterSubscriptionValueSet.Reference[componentIndex];
            this.FormulaValue = string.Empty;
            this.ComputedValue = parameterSubscriptionValueSet.Computed[componentIndex];
            this.ActualValue = parameterSubscriptionValueSet.ActualValue[componentIndex];
            this.Switch = parameterSubscriptionValueSet.ValueSwitch.ToString();
            this.HasValidationError = validationResult != ValidationResultKind.Valid;
            this.IsSelected = !this.HasValidationError;
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by the current row-view-model
        /// </summary>
        public Thing Thing { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the row has validation error
        /// </summary>
        public bool HasValidationError { get; private set; }

        /// <summary>
        /// Gets the Model code of the current row
        /// </summary>
        public string ModelCode { get; private set; }

        /// <summary>
        /// Gets the manual value of the current row
        /// </summary>
        public string ManualValue { get; private set; }

        /// <summary>
        /// Gets the formula value of the current row
        /// </summary>
        public string FormulaValue { get; private set; }

        /// <summary>
        /// Gets the computed value of the current row
        /// </summary>
        public string ComputedValue { get; private set; }

        /// <summary>
        /// Gets the reference value of the current row
        /// </summary>
        public string ReferenceValue { get; private set; }

        /// <summary>
        /// Gets the actual value of the current row
        /// </summary>
        public string ActualValue { get; private set; }

        /// <summary>
        /// Gets the string representation of the <see cref="ParameterSwitchKind"/>
        /// </summary>
        public string Switch { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current row is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.RaiseAndSetIfChanged(ref this.isSelected, value); }
        }
    }
}
