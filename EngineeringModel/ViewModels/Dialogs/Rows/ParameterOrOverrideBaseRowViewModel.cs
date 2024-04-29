// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
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
    /// The row representing a <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public abstract class ParameterOrOverrideBaseRowViewModel : ParameterBaseRowViewModel<ParameterOrOverrideBase>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOrOverrideBaseRowViewModel"/> class
        /// </summary>
        /// <param name="parameter">
        /// The associated <see cref="ParameterOrOverrideBase"/>
        /// </param>
        /// <param name="session">
        /// The associated <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The container Row.
        /// </param>
        /// <param name="isDialogReadOnly">
        /// Value indicating whether this row should be read-only because the dialog is read-only
        /// </param>
        protected ParameterOrOverrideBaseRowViewModel(ParameterOrOverrideBase parameter, ISession session, IDialogViewModelBase<ParameterOrOverrideBase> containerViewModel, bool isDialogReadOnly)
            : base(parameter, session, containerViewModel, isDialogReadOnly)
        {
        }
        #endregion

        #region Base Override
        /// <summary>
        /// Sets the values of this row in case where the <see cref="ParameterOrOverrideBase"/> is neither option-dependent nor state-dependent and is a <see cref="ScalarParameterType"/>
        /// </summary>
        public override void SetProperties()
        {
            var valueset = this.GetValueSet();
            if (valueset == null)
            {
                return;
            }

            this.SetValueSetValues(valueset);
        }

        /// <summary>
        /// Sets the values of the <see cref="ParameterValueSetBase"/>.
        /// </summary>
        /// <param name="valueSet">
        /// The valueSet.
        /// </param>
        private void SetValueSetValues(ParameterValueSetBase valueSet)
        {
            this.Value = valueSet.ActualValue.Any() ? valueSet.ActualValue.First() : "-";
            this.ScaleShortName = this.Thing.Scale == null ? "-" : this.Thing.Scale.ShortName;
            this.Switch = valueSet.ValueSwitch;
            this.Formula = valueSet.Formula.Any() ? valueSet.Formula.First() : "-";
            this.Computed = valueSet.Computed.Any() ? valueSet.Computed.First() : "-";
            this.Manual = valueSet.Manual.Any()
                              ? valueSet.Manual.First().ToValueSetObject(this.ParameterType)
                              : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Reference = valueSet.Reference.Any()
                                 ? valueSet.Reference.First().ToValueSetObject(this.ParameterType)
                                 : ValueSetConverter.DefaultObject(this.ParameterType);
            this.State = valueSet.ActualState == null ? "-" : valueSet.ActualState.ShortName;
            this.Option = valueSet.ActualOption;
            this.Published = valueSet.Published.Any() ? valueSet.Published.First() : "-";
        }
        #endregion

        /// <summary>
        /// Gets the single <see cref="ParameterValueSetBase"/> (not dependent, not state dependent)
        /// </summary>
        /// <returns>The <see cref="ParameterValueSetBase"/></returns>
        private ParameterValueSetBase GetValueSet()
        {
            return (ParameterValueSetBase)this.Thing.ValueSets.FirstOrDefault();
        }

        #region Update Value Set Methods

        /// <summary>
        /// Update a <see cref="ParameterValueSetBase"/> that may or may not be option and/or state dependent
        /// </summary>
        /// <param name="valueSet">
        /// The <see cref="ParameterValueSetBase"/> to update
        /// </param>
        public void UpdateValueSets(ParameterValueSetBase valueSet)
        {
            var actualOption = valueSet.ActualOption;
            var actualState = valueSet.ActualState;

            if (actualOption != null)
            {
                var optionRow = this.ContainedRows.Cast<ParameterOptionRowViewModel>().Single(x => x.ActualOption == actualOption);

                if (actualState != null)
                {
                    if (actualState.Kind != ActualFiniteStateKind.FORBIDDEN)
                    {
                        var actualStateRow = optionRow.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                        this.UpdateScalarOrCompoundValueSet(valueSet, actualStateRow);
                    }
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
                    if (actualState.Kind != ActualFiniteStateKind.FORBIDDEN)
                    {
                        var actualStateRow = this.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                        this.UpdateScalarOrCompoundValueSet(valueSet, actualStateRow);
                    }
                }
                else
                {
                    this.UpdateScalarOrCompoundValueSet(valueSet);
                }
            }
        }

        /// <summary>
        /// Update a <see cref="ParameterValueSetBase"/> with the values from the current row or the passed <see cref="ParameterValueBaseRowViewModel"/>
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase"/> to update</param>
        /// <param name="row">The <see cref="ParameterValueBaseRowViewModel"/> containing the information, or if null the current row</param>
        private void UpdateScalarOrCompoundValueSet(ParameterValueSetBase valueSet, ParameterValueBaseRowViewModel row = null)
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
        private void UpdateScalarValueSet(ParameterValueSetBase valueSet, ParameterValueBaseRowViewModel row = null)
        {
            valueSet.ValueSwitch = row == null ? this.Switch.Value : row.Switch.Value;
            valueSet.Computed = row == null ? new ValueArray<string>(new List<string> { this.Computed }) : new ValueArray<string>(new List<string> { row.Computed });
            valueSet.Manual = row == null ? new ValueArray<string>(new List<string> { this.Manual.ToValueSetString(this.ParameterType) }) : new ValueArray<string>(new List<string> { row.Manual.ToValueSetString(this.ParameterType) });
            valueSet.Reference = row == null ? new ValueArray<string>(new List<string> { this.Reference.ToValueSetString(this.ParameterType) }) : new ValueArray<string>(new List<string> { ValueSetConverter.ToValueSetString(row.Reference, this.ParameterType) });
        }

        /// <summary>
        /// Update value-set for a compound parameter.
        /// </summary>
        /// <param name="valueSet">The value set to update</param>
        /// <param name="row">The value row containing the information. If null the data is retrieved from the current row.</param>
        /// <remarks>
        /// If <paramref name="row"/> is null, it means the parameter is not compound, not option dependent and not state dependent.
        /// </remarks>
        private void UpdateCompoundValueSet(ParameterValueSetBase valueSet, ParameterValueBaseRowViewModel row = null)
        {
            var componentRows = (row == null)
                ? this.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList()
                : row.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList();

            valueSet.Computed = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.Manual = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.Reference = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.ValueSwitch = componentRows[0].Switch.Value; // all the switches should have the same value
            for (var i = 0; i < componentRows.Count; i++)
            {
                valueSet.Computed[i] = componentRows[i].Computed;
                valueSet.Manual[i] = componentRows[i].Manual.ToValueSetString(this.ParameterType);
                valueSet.Reference[i] = componentRows[i].Reference.ToValueSetString(this.ParameterType);
            }
        }

        #endregion
    }
}