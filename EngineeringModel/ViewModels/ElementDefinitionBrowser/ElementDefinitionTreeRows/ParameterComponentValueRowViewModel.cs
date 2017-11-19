// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterComponentValueRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
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
        /// <param name="isReadOnly">
        /// A value indicating whether the row is read-only
        /// </param>
        public ParameterComponentValueRowViewModel(ParameterBase parameterBase, int valueIndex, ISession session, Option actualOption, ActualFiniteState actualState, IViewModelBase<Thing> containerRow, bool isReadOnly)
            : base(parameterBase, session, actualOption, actualState, containerRow, valueIndex, isReadOnly)
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

            var component = compoundParameterType.Component[valueIndex];
            var subscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(component)
                        .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => this.Name = component.ShortName);
            this.Disposables.Add(subscription);

            this.Name = component.ShortName;
            this.Option = this.ActualOption;
            this.State = (this.ActualState != null) ? this.ActualState.Name : string.Empty;

            this.WhenAnyValue(x => x.Switch).Skip(1).Subscribe(x =>
            {
                var valueBaseRow = this.ContainerViewModel as ParameterValueBaseRowViewModel;
                if (valueBaseRow != null)
                {
                    foreach (ParameterComponentValueRowViewModel row in valueBaseRow.ContainedRows)
                    {
                        row.Switch = x;
                    }

                    return;
                }

                var parameterBaseRow = this.ContainerViewModel as ParameterOrOverrideBaseRowViewModel;
                if (parameterBaseRow != null)
                {
                    foreach (ParameterComponentValueRowViewModel row in parameterBaseRow.ContainedRows)
                    {
                        row.Switch = x;
                    }

                    return;
                }

                var subscriptionRow = this.ContainerViewModel as ParameterSubscriptionRowViewModel;
                if (subscriptionRow != null)
                {
                    foreach (ParameterComponentValueRowViewModel row in subscriptionRow.ContainedRows)
                    {
                        row.Switch = x;
                    }

                    return;
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
        public override void SetValues()
        {
            var compoundParameterType = (CompoundParameterType)this.Thing.ParameterType;
            base.SetValues();
            this.Scale = compoundParameterType.Component[this.ValueIndex].Scale;
            this.ScaleShortName = this.Scale == null ? "-" : this.Scale.ShortName;
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
            if (columnName == "Manual" || columnName == "Reference")
            {
                var parameterType = this.ParameterType;
                var scale = this.Scale;

                return ParameterValueValidator.Validate(newValue, parameterType, scale);
            }

            return null;
        }
    }
}
