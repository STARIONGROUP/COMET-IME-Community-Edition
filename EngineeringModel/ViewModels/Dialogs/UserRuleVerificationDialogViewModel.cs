// -------------------------------------------------------------------------------------------------
// <copyright file="UserRuleVerificationDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The view-model used to create, edit or inspect a <see cref="UserRuleVerification"/> from the associated dialog.
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.UserRuleVerification)]
    public class UserRuleVerificationDialogViewModel : CDP4CommonView.UserRuleVerificationDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The default <see cref="Rule"/> that is used to populate the dialog for a <see cref="Create"/> dialog
        /// </summary>
        private readonly BinaryRelationshipRule defaultRule = new BinaryRelationshipRule(Guid.NewGuid(), null, null) { Name = "-", ShortName = "-" };

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRuleVerificationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public UserRuleVerificationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRuleVerificationDialogViewModel"/> class
        /// </summary>
        /// <param name="userRuleVerification">
        /// The <see cref="UserRuleVerification"/> that is the subject of the current view-model. This is the object
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
        public UserRuleVerificationDialogViewModel(UserRuleVerification userRuleVerification, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers)
            : base(userRuleVerification, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            if (this.dialogKind == ThingDialogKind.Create && this.Thing.Rule == null)
            {
                this.Thing.Rule = this.defaultRule;
            }

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

            clone.IsActive = this.IsActive;
            clone.ExecutedOn = this.ExecutedOn;
            clone.Status = this.Status;
            clone.Rule = this.SelectedRule;
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.InspectOwnerCommand = ReactiveCommandCreator.Create(this.ExecuteInspectOwnerCommand);
        }

        /// <summary>
        /// Populates the <see cref="PossibleRule"/> property
        /// </summary>
        protected override void PopulatePossibleRule()
        {
            base.PopulatePossibleRule();

            var rules = this.GetPossibleRule();
            foreach (var rule in rules)
            {
                this.PossibleRule.Add(rule);
            }
        }

        /// <summary>
        /// Queries all the <see cref="Rule"/> instances from the chain of <see cref="ReferenceDataLibrary"/> of the container <see cref="ReferenceDataLibrary"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{MeasurementUnit}"/> ordered by short name.
        /// </returns>
        private IEnumerable<Rule> GetPossibleRule()
        {
            var ruleVerificationList = (RuleVerificationList)this.Container;
            var containerIteration = (Iteration)ruleVerificationList.Container;
            var model = (EngineeringModel)containerIteration.Container;
            
            var rules = new List<Rule>();
            
            var rdls = this.Session.GetEngineeringModelRdlChain(model);
            foreach (var rdl in rdls)
            {
                rules.AddRange(rdl.Rule);
            }

            return rules.OrderBy(r => r.ShortName).ToList();
        }

        /// <summary>
        /// Gets the owner of the <see cref="UserRuleVerification"/> that is represented by the view-model.
        /// </summary>
        public string Owner { get; private set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRule"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectOwnerCommand { get; protected set; }

        /// <summary>
        /// Execute the <see cref="InspectOwnerCommand"/>
        /// </summary>
        private void ExecuteInspectOwnerCommand()
        {
            var owner = this.Thing.Owner;

            this.thingDialogNavigationService.Navigate(owner, null, this.Session, false, ThingDialogKind.Inspect, this.thingDialogNavigationService);
        }
    }
}
