// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BehavioralParameterRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.CommonView.ViewModels
{
    using System;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// A row view model that represents a <see cref="BehavioralParameter"/> 
    /// </summary>
    public class BehavioralParameterRowViewModel : RowViewModelBase<BehavioralParameter>
    {
        /// <summary>
        /// Backing field for <see cref="VariableName"/> property
        /// </summary>
        private string variableName;

        /// <summary>
        /// Backing field for the <see cref="Parameter"/> property
        /// </summary>
        private Parameter parameter;

        /// <summary>
        /// Backing field for the <see cref="ParameterKind"/> property
        /// </summary>
        private BehavioralParameterKind parameterKind;

        /// <summary>
        /// Backing field for the <see cref="IsValid"/> property
        /// </summary>
        private bool isValid = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralParameterRowViewModel"/> class
        /// </summary>
        /// <param name="behavioralParameter">The <see cref="BehavioralParameter"/> represented by this view model</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="BehaviorDialogViewModel"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public BehavioralParameterRowViewModel(BehavioralParameter behavioralParameter, ISession session, BehaviorDialogViewModel containerViewModel)
            : base(behavioralParameter, session, containerViewModel)
        {
            this.UpdateProperties();
            this.IsReadOnly = containerViewModel.IsReadOnly;

            this.WhenAnyValue(x => x.VariableName, x => x.Parameter).Subscribe(_ => this.IsValid = this.ValidateRow());
        }

        /// <summary>
        /// Validates the row data
        /// </summary>
        /// <returns>The validity status of the row</returns>
        private bool ValidateRow()
        {
            return !string.IsNullOrWhiteSpace(this.VariableName) &&
                   this.Parameter is not null;
        }

        /// <summary>
        /// The valid status of this row
        /// </summary>
        public bool IsValid
        {
            get => this.isValid;
            set => this.RaiseAndSetIfChanged(ref this.isValid, value);
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
            string error = null;
            switch (columnName)
            {
                case nameof(this.VariableName):
                    error = string.IsNullOrWhiteSpace(newValue.ToString()) ? "Variable Name cannot be empty" : null;
                    break;
                case nameof(this.Parameter):
                    error = newValue is null ? "A parameter must be selected" : null;
                    break;
            }

            if (error is not null)
            {
                this.IsValid = false;
            }

            return error;
        }

        /// <summary>
        /// Gets a value indicating whether this row is read-only
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// Gets or sets the <see cref="VariableName"/> column value
        /// </summary>
        public string VariableName
        {
            get => this.variableName;
            set => this.RaiseAndSetIfChanged(ref this.variableName, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Parameter"/> column value
        /// </summary>
        public Parameter Parameter
        {
            get => this.parameter;
            set => this.RaiseAndSetIfChanged(ref this.parameter, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ParameterKind"/> column value
        /// </summary>
        public BehavioralParameterKind ParameterKind
        {
            get => this.parameterKind;
            set => this.RaiseAndSetIfChanged(ref this.parameterKind, value);
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
        /// Creates, update and write a clone in the data-source when inline-editing with a new value for one of its property
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="fieldName">The property name</param>
        public override void CreateCloneAndWrite(object newValue, string fieldName)
        {
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.VariableName = this.Thing.VariableName;
            this.Parameter = this.Thing.Parameter;
            this.ParameterKind = this.Thing.BehavioralParameterKind;
            this.IsValid = this.ValidateRow();
        }

        /// <summary>
        /// Update the transaction with the <see cref="BehavioralParameter"/> information represented by this view model
        /// </summary>
        /// <param name="transaction">The transaction for the <see cref="Behavior"/></param>
        public void UpdateTransaction(IThingTransaction transaction)
        {
            var clone = this.Thing;
            clone.VariableName = this.VariableName;
            clone.Parameter = this.Parameter;
            clone.BehavioralParameterKind = this.ParameterKind;
            transaction.CreateOrUpdate(clone);
        }
    }
}
