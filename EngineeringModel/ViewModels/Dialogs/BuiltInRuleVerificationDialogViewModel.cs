// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleVerificationDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Operations;
    
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The view-model used to create, edit or inspect a <see cref="BuiltInRuleVerification"/> from the associated dialog.
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.BuiltInRuleVerification)]
    public class BuiltInRuleVerificationDialogViewModel : CDP4CommonView.BuiltInRuleVerificationDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The default <see cref="IBuiltInRuleMetaData"/> that the <see cref="SelectedBuiltInRuleMetaData"/> is set to if the
        /// <see cref="BuiltInRule"/>'s name does not match any of the available <see cref="BuiltInRule"/>s.
        /// </summary>
        private readonly IBuiltInRuleMetaData empytBuiltInRule = new BuiltInRuleMetaDataExportAttribute("-", "-", "-");

        /// <summary>
        /// Backing field for the <see cref="selectedBuiltInRuleMetaData"/> property.
        /// </summary>
        private IBuiltInRuleMetaData selectedBuiltInRuleMetaData;

        /// <summary>
        /// Backing field for the <see cref="IsNameReadOnly"/> property
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> isNameReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleVerificationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BuiltInRuleVerificationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleVerificationDialogViewModel"/> class
        /// </summary>
        /// <param name="builtInRuleVerification">
        /// The <see cref="BuiltInRuleVerification"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public BuiltInRuleVerificationDialogViewModel(BuiltInRuleVerification builtInRuleVerification, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers)
            : base(builtInRuleVerification, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {            
            this.LoadAvailableBuiltInRules();
            this.SetSelectedBuiltInRuleMetaData();

            this.WhenAnyValue(vm => vm.SelectedBuiltInRuleMetaData)
                    .Select(metaData => metaData != this.empytBuiltInRule || this.IsReadOnly)
                    .ToProperty(this, vm => vm.IsNameReadOnly, out this.isNameReadOnly);

            this.WhenAnyValue(vm => vm.SelectedBuiltInRuleMetaData).Subscribe(_ => this.UpdateName());
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Name"/> property is readonly or not.
        /// </summary>
        /// <remarks>
        /// The <see cref="Name"/> property is only editable when the selected <see cref="BuiltInRule"/>
        /// is set to the <see cref="empytBuiltInRule"/>.
        /// </remarks>
        public bool IsNameReadOnly
        {
            get
            {
                return this.isNameReadOnly.Value;
            }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            if (this.dialogKind == ThingDialogKind.Create)
            {
                var container = (RuleVerificationList)this.Container;
                this.Owner = string.Format("{0} [{1}]", container.Owner.Name, container.Owner.ShortName);
            }
            else
            {
                this.Owner = string.Format("{0} [{1}]", this.Thing.Owner.Name, this.Thing.Owner.ShortName);
            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        /// <remarks>
        /// The method has an override due to the fact that the generated base class sets the "name" property
        /// which is a derived property for <see cref="UserRuleVerification"/>.
        /// </remarks>
        protected override void UpdateTransaction()
        {
            var clone = this.Thing;

            clone.Name = this.Name;
            clone.IsActive = this.IsActive;
            clone.ExecutedOn = this.ExecutedOn;
            clone.Status = this.Status;
        }

        /// <summary>
        /// Gets the owner of the <see cref="UserRuleVerification"/> that is represented by the view-model.
        /// </summary>
        public string Owner { get; private set; }

        /// <summary>
        /// Gets the available <see cref="IBuiltInRuleMetaData"/>
        /// </summary>
        public IEnumerable<IBuiltInRuleMetaData> AvailableBuiltInRules { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="IBuiltInRuleMetaData"/>.
        /// </summary>
        public IBuiltInRuleMetaData SelectedBuiltInRuleMetaData
        {
            get { return this.selectedBuiltInRuleMetaData; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBuiltInRuleMetaData, value); }
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.InspectOwnerCommand = ReactiveCommand.Create(this.ExecuteInspectOwnerCommand);
        }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRule"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectOwnerCommand { get; protected set; }

        /// <summary>
        /// Executes the <see cref="InspectOwnerCommand"/>
        /// </summary>
        private void ExecuteInspectOwnerCommand()
        {
            var owner = this.Thing.Owner;

            this.thingDialogNavigationService.Navigate(owner, null, this.Session, false, ThingDialogKind.Inspect, this.thingDialogNavigationService);
        }

        /// <summary>
        /// Loads the available <see cref="BuiltInRule"/>s from the <see cref="IRuleVerificationService"/>
        /// </summary>
        private void LoadAvailableBuiltInRules()
        {
            var ruleVerificationService = ServiceLocator.Current.GetInstance<IRuleVerificationService>();

            var availableBuiltInRules = new List<IBuiltInRuleMetaData>();
            availableBuiltInRules.Add(this.empytBuiltInRule);

            var lazyRules = ruleVerificationService.BuiltInRules;
            foreach (var lazyRule in lazyRules)
            {
                availableBuiltInRules.Add(lazyRule.Metadata); 
            }

            this.AvailableBuiltInRules = availableBuiltInRules;
        }

        /// <summary>
        /// Sets the <see cref="SelectedBuiltInRuleMetaData"/> according to the name of the current <see cref="BuiltInRule"/>.
        /// </summary>
        private void SetSelectedBuiltInRuleMetaData()
        {
            var name = this.Thing.Name;
            var metaData = this.AvailableBuiltInRules.SingleOrDefault(x => x.Name == name);
            if (metaData != null)
            {
                this.SelectedBuiltInRuleMetaData = metaData;
            }
            else
            {
                this.SelectedBuiltInRuleMetaData = this.empytBuiltInRule;
            }
        }

        /// <summary>
        /// Update the <see cref="Name"/> property based on the selected <see cref="BuiltInRule"/>.
        /// </summary>
        private void UpdateName()
        {
            if (this.dialogKind != ThingDialogKind.Inspect && this.SelectedBuiltInRuleMetaData != this.empytBuiltInRule)
            {
                this.Name = this.SelectedBuiltInRuleMetaData.Name;                
            }
        }
    }
}
