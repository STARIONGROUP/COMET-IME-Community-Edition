// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetRowViewModelBase.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using Services;

    public abstract class BudgetRowViewModelBase<T> : ReactiveObject, IBudgetRowViewModelBase where T : ISubSystemBudgetResult
    {
        /// <summary>
        /// Backing field for <see cref="SubSystemName"/>
        /// </summary>
        private string subSystemName;

        /// <summary>
        /// Backing field for <see cref="SelectedSystemLevel"/>
        /// </summary>
        protected SystemLevelKind selectedSystemLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetRowViewModelBase{T}"/> class
        /// </summary>
        /// <param name="result">The <see cref="T"/></param>
        /// <param name="subSystem">The sub-system</param>
        /// <param name="recomputeTotal">the action to recompute the system summary</param>
        protected BudgetRowViewModelBase(T result, SubSystem subSystem, Action recomputeTotal)
        {
            this.Result = result;
            this.SubSystem = subSystem;
            this.SubSystemName = this.SubSystem.SubSystemElementUsage.Name;
            this.SelectedSystemLevel = this.Result.SystemLevelUsed;
            this.WhenAnyValue(x => x.SelectedSystemLevel).Skip(1).Subscribe(x =>
            {
                this.SetValues();
                recomputeTotal();
            });

            this.SetValues();
        }

        /// <summary>
        /// Gets the sub-system
        /// </summary>
        protected SubSystem SubSystem { get; }

        /// <summary>
        /// Gets or sets the system level to use for the budget calculation
        /// </summary>
        public SystemLevelKind SelectedSystemLevel
        {
            get { return this.selectedSystemLevel; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSystemLevel, value); }
        }

        /// <summary>
        /// The result to display
        /// </summary>
        protected T Result { get; private set; }

        /// <summary>
        /// Gets the subsystem name
        /// </summary>
        public string SubSystemName
        {
            get { return this.subSystemName; }
            private set { this.RaiseAndSetIfChanged(ref this.subSystemName, value); }
        }

        /// <summary>
        /// Set the values
        /// </summary>
        protected void SetValues()
        {
            if (this.SelectedSystemLevel == SystemLevelKind.Equipment)
            {
                this.SetValuesFromEquipment();
            }
            else
            {
                this.SetValuesFromSubSystem();
            }
        }

        /// <summary>
        /// Computes the budget using the sub-system values
        /// </summary>
        protected abstract void SetValuesFromSubSystem();

        /// <summary>
        /// Computes the budget using the equipment values
        /// </summary>
        protected abstract void SetValuesFromEquipment();
    }
}
