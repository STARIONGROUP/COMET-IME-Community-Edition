// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipCreatorMainViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    
    using CDP4Dal.Operations;
    using CDP4Dal;
    
    using NLog;
    
    using ReactiveUI;

    /// <summary>
    /// The view-model to create a new <see cref="Relationship"/> through means of drag and drop
    /// </summary>
    public class RelationshipCreatorMainViewModel : ReactiveObject, IDisposable
    {
        /// <summary>
        /// Logger instance used to log using ILog Facade
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Iteration"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The current <see cref="ISession"/>
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The collection of <see cref="IDisposable"/>
        /// </summary>
        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        /// <summary>
        /// Backing field for <see cref="SelectedRelationshipCreator"/>
        /// </summary>
        private IRelationshipCreatorViewModel selectedRelationshipCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipCreatorMainViewModel"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        public RelationshipCreatorMainViewModel(ISession session, Iteration iteration)
        {
            this.session = session;
            this.iteration = iteration;
            this.RelationshipCreators = new List<IRelationshipCreatorViewModel>();
            this.BinaryRelationshipCreator = new BinaryRelationshipCreatorViewModel(this.iteration, this.session);
            this.MultiRelationshipCreator = new MultiRelationshipCreatorViewModel(this.iteration, this.session);
            this.RelationshipCreators.Add(this.BinaryRelationshipCreator);
            this.RelationshipCreators.Add(this.MultiRelationshipCreator);
            this.SelectedRelationshipCreator = this.BinaryRelationshipCreator;

            this.subscriptions.Add(this.WhenAnyValue(x => x.SelectedRelationshipCreator).Where(x => x != null).Subscribe(x => this.InitializeCreateCommand()));
        }

        /// <summary>
        /// Gets the <see cref="IRelationshipCreatorViewModel"/>
        /// </summary>
        public List<IRelationshipCreatorViewModel> RelationshipCreators { get; private set; }

        /// <summary>
        /// Gets the view-model used to set the properties of a <see cref="BinaryRelationship"/> to create
        /// </summary>
        public BinaryRelationshipCreatorViewModel BinaryRelationshipCreator { get; private set; }

        /// <summary>
        /// Gets the view-model used to set the properties of a <see cref="MultiRelationship"/> to create
        /// </summary>
        public MultiRelationshipCreatorViewModel MultiRelationshipCreator { get; private set; }

        /// <summary>
        /// Gets or sets the type of <see cref="Relationship"/> to create
        /// </summary>
        public IRelationshipCreatorViewModel SelectedRelationshipCreator
        {
            get { return this.selectedRelationshipCreator; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRelationshipCreator, value); }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to create a <see cref="Relationship"/>
        /// </summary>
        public ReactiveCommand<object> CreateRelationshipCommand { get; private set; }

        /// <summary>
        /// Initializes the <see cref="CreateRelationshipCommand"/>
        /// </summary>
        private void InitializeCreateCommand()
        {
            this.CreateRelationshipCommand = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.SelectedRelationshipCreator.CanCreate));
            this.CreateRelationshipCommand.Subscribe(x => this.ExecuteCreateRelationshipCommand());
        }

        /// <summary>
        /// Execute the <see cref="CreateRelationshipCommand"/>
        /// </summary>
        private async Task ExecuteCreateRelationshipCommand()
        {
            var relationship = this.SelectedRelationshipCreator.CreateRelationshipObject();
            relationship.Owner = this.session.OpenIterations[this.iteration].Item1;

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.iteration));
            var iterationClone = this.iteration.Clone(false);
            iterationClone.Relationship.Add(relationship);
            transaction.CreateOrUpdate(iterationClone);
            transaction.Create(relationship);

            try
            {
                await this.session.Write(transaction.FinalizeTransaction());
                this.SelectedRelationshipCreator.ReInitializeControl();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        /// <summary>
        /// Disposes of this view-model
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in this.subscriptions)
            {
                subscription.Dispose();
            }

            this.BinaryRelationshipCreator.Dispose();
            this.MultiRelationshipCreator.Dispose();
        }
    }
}