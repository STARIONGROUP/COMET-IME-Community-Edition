// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleNavBarRelationViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4DiagramEditor.ViewModels.Relation
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// Represent a rule for the navigation bar
    /// </summary>
    public class RuleNavBarRelationViewModel : RuleRowViewModel<Rule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleNavBarRelationViewModel"/> class.
        /// </summary>
        /// <param name="rule">The rule referenced by this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public RuleNavBarRelationViewModel(Rule rule, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(rule, session, containerViewModel)
        {
            this.InitializeCommands();
        }

        /// <summary>
        /// Gets the public command to create a relationship.
        /// </summary>
        public ReactiveCommand<object> CreateRelationshipCommand { get; private set; }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            var canCreateRelationshipBasedOnRule = this.Thing is BinaryRelationshipRule ? Observable.Return(true) :
                ((DiagramEditorViewModel)this.ContainerViewModel).SelectedItems.Changed.Select(_ => this.CanCreateRelationship());

            this.CreateRelationshipCommand = ReactiveCommand.Create(canCreateRelationshipBasedOnRule);
            this.CreateRelationshipCommand.Subscribe(_ => this.ExecuteCreateRelationshipCommand());
        }

        /// <summary>
        /// Compute whether a <see cref="Relationship"/> can be created.
        /// This is true only when two <see cref="NamedThingDiagramContentItem"/> with <see cref="Thing"/>s are selected or
        /// this Rule describes a <see cref="BinaryRelationshipRule"/>.
        /// </summary>
        /// <returns>True when two <see cref="NamedThingDiagramContentItem"/> with <see cref="Thing"/>s are selected. </returns>
        private bool CanCreateRelationship()
        {
            // Multi relationships require more than one item to be selected.
            if (((DiagramEditorViewModel)this.ContainerViewModel).SelectedItems.Count <= 1)
            {
                return false;
            }

            return ((DiagramEditorViewModel)this.ContainerViewModel).SelectedItems.All(x => x is NamedThingDiagramContentItem);
        }

        /// <summary>
        /// Executes the <see cref="CreateRelationshipCommand"/>
        /// </summary>
        private void ExecuteCreateRelationshipCommand()
        {
            if (!(this.ContainerViewModel is DiagramEditorViewModel))
            {
                throw new NullReferenceException("The Relationship Editor view mdoel is not set");
            }
        }
    }
}