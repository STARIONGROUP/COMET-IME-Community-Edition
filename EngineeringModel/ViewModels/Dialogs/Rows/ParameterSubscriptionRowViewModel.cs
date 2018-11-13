// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Dal;

    /// <summary>
    /// The row representing a <see cref="ParameterSubscription"/> in the Element Definition browser
    /// </summary>
    public class ParameterSubscriptionRowViewModel : ParameterBaseRowViewModel<ParameterSubscription>
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionRowViewModel"/> class
        /// </summary>
        /// <param name="parameterSubscription">
        /// The associated <see cref="ParameterSubscription"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// the container view-model
        /// </param>
        /// <param name="isDialogReadOnly">
        /// The is Dialog Read Only.
        /// </param>
        public ParameterSubscriptionRowViewModel(ParameterSubscription parameterSubscription, ISession session, IDialogViewModelBase<ParameterSubscription> containerViewModel, bool isDialogReadOnly = false)
            : base(parameterSubscription, session, containerViewModel, isDialogReadOnly)
        {
        }

        #endregion

        /// <summary>
        /// Sets the values of this row in case where the <see cref="ParameterSubscription"/> is neither option-dependent nor state-dependent and is a <see cref="ScalarParameterType"/>
        /// </summary>
        public override void SetProperties()
        {
            var valueset = this.Thing.ValueSet.FirstOrDefault();

            if (valueset == null)
            {
                logger.Error("No value set was found for parameter subscription {0}", this.Thing.Iid);
                return;
            }

            this.Value = valueset.ActualValue.Any() ? valueset.ActualValue.First() : "-";
            this.ScaleShortName = this.Thing.Scale == null ? "-" : this.Thing.Scale.ShortName;
            this.Switch = valueset.ValueSwitch;
            this.Computed = valueset.Computed.Any() ? valueset.Computed.First() : "-";
            this.Manual = valueset.Manual.Any() ? valueset.Manual.First().ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Reference = valueset.Reference.Any() ? valueset.Reference.First().ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Formula = valueset.SubscribedValueSet.Formula.Any() ? valueset.SubscribedValueSet.Formula.First() : "-";
        }

        /// <summary>
        /// Update the <see cref="ParameterSubscriptionValueSet"/> with the row values
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterSubscriptionValueSet"/> to update</param>
        public void UpdateValueSets(ParameterSubscription subscription)
        {
            foreach (var valueSet in subscription.ValueSet)
            {
                var actualOption = valueSet.ActualOption;
                var actualState = valueSet.ActualState;

                if (actualOption != null)
                {
                    var optionRow = this.ContainedRows.Cast<ParameterOptionRowViewModel>().Single(x => x.ActualOption == actualOption);
                    if (actualState != null)
                    {
                        var actualStateRow = optionRow.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                        this.UpdateScalarOrCompoundValueSet(valueSet, actualStateRow);
                    }
                    else
                    {
                        this.UpdateScalarOrCompoundValueSet(valueSet, optionRow);
                    }
                }
                else
                {
                    if (actualState != null)
                    {
                        var actualStateRow = this.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                        this.UpdateScalarOrCompoundValueSet(valueSet, actualStateRow);
                    }
                    else
                    {
                        this.UpdateScalarOrCompoundValueSet(valueSet);
                    }
                }
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
            if (propertyName == "Reference")
            {
                return false;
            }

            return base.IsEditable(propertyName);
        }

        #region Update value sets Methods
        /// <summary>
        /// Call the correct update method depending on kind of parameter type (scalar, compound)
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterSubscriptionValueSet"/> to update</param>
        /// <param name="row">The <see cref="ParameterValueBaseRowViewModel"/> containing the information, or if null the current row</param>
        private void UpdateScalarOrCompoundValueSet(ParameterSubscriptionValueSet valueSet, ParameterValueBaseRowViewModel row = null)
        {
            if (this.isCompoundType)
            {
                this.UpdateCompoundValueSet(valueSet, row);
            }
            else
            {
                this.UpdateScalarValueSet(valueSet, row);
            }
        }

        /// <summary>
        /// Update value-set for a not-compound parameter.
        /// </summary>
        /// <param name="valueSet">The value set to update</param>
        /// <param name="row">The value row containing the information. If null the data is retrieved from the current row.</param>
        /// <remarks>
        /// If <paramref name="row"/> is null, it means the parameter is not compound, not option dependent and not state dependent.
        /// </remarks>
        private void UpdateScalarValueSet(ParameterSubscriptionValueSet valueSet, ParameterValueBaseRowViewModel row = null)
        {
            valueSet.ValueSwitch = row == null ? this.Switch.Value : row.Switch.Value;
            valueSet.Manual = row == null ? new ValueArray<string>(new List<string> { ValueSetConverter.ToValueSetString(this.Manual, this.ParameterType) }) : new ValueArray<string>(new List<string> { ValueSetConverter.ToValueSetString(row.Manual, this.ParameterType) });
        }

        /// <summary>
        /// Update value-set for a compound parameter.
        /// </summary>
        /// <param name="valueSet">The value set to update</param>
        /// <param name="row">The value row containing the information. If null the data is retrieved from the current row.</param>
        /// <remarks>
        /// If <paramref name="row"/> is null, it means the parameter is not compound, not option dependent and not state dependent.
        /// </remarks>
        private void UpdateCompoundValueSet(ParameterSubscriptionValueSet valueSet, ParameterValueBaseRowViewModel row = null)
        {
            var componentRows = (row == null)
                ? this.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList()
                : row.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList();

            valueSet.Manual = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.ValueSwitch = componentRows[0].Switch.Value; // all the switches should have the same value
            for (var i = 0; i < componentRows.Count; i++)
            {
                valueSet.Manual[i] = componentRows[i].Manual.ToValueSetString(this.ParameterType);
            }
        }
        #endregion
    }
}