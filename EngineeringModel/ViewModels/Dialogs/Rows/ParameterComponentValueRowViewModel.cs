// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterComponentValueRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The Row representing a value that corresponds to a <see cref="ParameterTypeComponent"/> of a <see cref="ParameterBase"/>
    /// </summary>
    public class ParameterComponentValueRowViewModel : ParameterValueBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterComponentValueRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">
        /// The associated <see cref="ParameterBase"/>
        /// </param>
        /// <param name="valueIndex">
        /// The index of this component in the <see cref="CompoundParameterType"/>
        /// </param>
        /// <param name="session">
        /// The associated <see cref="ISession"/>
        /// </param>
        /// <param name="actualOption">
        /// The <see cref="Option"/> of this row if any
        /// </param>
        /// <param name="actualState">
        /// The <see cref="ActualFiniteState"/> of this row if any
        /// </param>
        /// <param name="containerRow">
        /// the row container
        /// </param>
        /// <param name="isDialogReadOnly">
        /// A value indicating whether the dialog is read-only
        /// </param>
        public ParameterComponentValueRowViewModel(ParameterBase parameterBase, int valueIndex, ISession session, Option actualOption, ActualFiniteState actualState, IViewModelBase<Thing> containerRow, bool isDialogReadOnly = false)
            : base(parameterBase, session, actualOption, actualState, containerRow, valueIndex, isDialogReadOnly)
        {
            var compoundParameterType = this.Thing.ParameterType as CompoundParameterType;
            if (compoundParameterType == null)
            {
                throw new InvalidOperationException("This row shall only be used for CompoundParameterType.");
            }

            if (valueIndex >= compoundParameterType.Component.Count)
            {
                throw new IndexOutOfRangeException(string.Format("The compoundParameterType {0} has only {1} components", compoundParameterType.Name, compoundParameterType.Component.Count));
            }

            if (containerRow == null)
            {
                throw new ArgumentNullException("containerRow", "The container row is mandatory");
            }

            // reset the classkind of this row to match the component classkind
            this.ParameterTypeClassKind = ((CompoundParameterType)this.ParameterType).Component[this.ValueIndex].ParameterType.ClassKind;
            this.Scale = ((CompoundParameterType)this.ParameterType).Component[this.ValueIndex].Scale;
            this.ParameterType = ((CompoundParameterType)this.ParameterType).Component[this.ValueIndex].ParameterType;

            this.Name = compoundParameterType.Component[valueIndex].ShortName;
            this.Option = this.ActualOption;
            this.State = (this.ActualState != null) ? this.ActualState.Name : string.Empty;

            this.WhenAnyValue(rowViewModel => rowViewModel.Switch).Skip(1).Subscribe(switchKind =>
            {
                var valueBaseRow = this.ContainerViewModel as ParameterValueBaseRowViewModel;
                if (valueBaseRow != null)
                {
                    foreach (ParameterComponentValueRowViewModel row in valueBaseRow.ContainedRows)
                    {
                        row.Switch = switchKind;
                    }

                    return;
                }

                var parameterBaseRow = this.ContainerViewModel as ParameterOrOverrideBaseRowViewModel;
                if (parameterBaseRow != null)
                {
                    foreach (ParameterComponentValueRowViewModel row in parameterBaseRow.ContainedRows)
                    {
                        row.Switch = switchKind;
                    }

                    return;
                }

                var subscriptionRow = this.ContainerViewModel as ParameterSubscriptionRowViewModel;
                if (subscriptionRow != null)
                {
                    foreach (ParameterComponentValueRowViewModel row in subscriptionRow.ContainedRows)
                    {
                        row.Switch = switchKind;
                    }
                }
            });
        }

        /// <summary>
        /// The row type for this <see cref="ParameterComponentValueRowViewModel"/>
        /// </summary>
        public override string RowType
        {
            get { return "Parameter Type Component"; }
        }

        /// <summary>
        /// Gets a value indicating whether the values are read only
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the reference values are read only
        /// </summary>
        public bool IsReferenceReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Setting values for this <see cref="ParameterComponentValueRowViewModel"/>
        /// </summary>
        public override void UpdateValues()
        {
            base.UpdateValues();
            var compoundParameterType = (CompoundParameterType)this.Thing.ParameterType;
            this.Scale = compoundParameterType.Component[ValueIndex].Scale;
            this.ScaleShortName = this.Scale == null ? "-" : this.Scale.ShortName;
        }

        /// <summary>
        /// Check that the values of this row are valid
        /// </summary>
        /// <param name="scale">The <see cref="MeasurementScale"/></param>
        public override void CheckValues(MeasurementScale scale)
        {
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
                var parameterType = this.ParameterType;
                var scale = this.Scale;

                if (columnName == ManualPropertyName)
                {
                    var propertyCode = this.RowCode + ManualPropertyName;

                    var rule = this.DialogViewModel.ValidationErrors.SingleOrDefault(x => x.PropertyName == propertyCode);
                    if (rule != null)
                    {
                        this.DialogViewModel.ValidationErrors.Remove(rule);
                    }

                    var validationMsg = ParameterValueValidator.Validate(this.Manual, parameterType, scale);
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

                    var validationMsg = ParameterValueValidator.Validate(this.Reference, parameterType, scale);
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
    }
}
