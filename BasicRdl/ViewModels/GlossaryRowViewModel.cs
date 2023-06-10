// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlossaryRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="GlossaryRowViewModel"/> is to represent a <see cref="Glossary"/> object in a list or tree like structure
    /// </summary>
    public class GlossaryRowViewModel : CDP4CommonView.GlossaryRowViewModel, IDropTarget
    {
        /// <summary>
        /// Backing field for <see cref="ContainerRdlShortName"/>
        /// </summary>
        private string containerRdlShortName;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryRowViewModel"/> class. 
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public GlossaryRowViewModel(Glossary thing, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(thing, session, containerViewModel)
        {
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Gets or sets the Container RDL ShortName.
        /// </summary>
        public string ContainerRdlShortName
        {
            get => this.containerRdlShortName;
            set => this.RaiseAndSetIfChanged(ref this.containerRdlShortName, value);
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
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Category category)
            {
                dropInfo.Effects = CategoryApplicationValidationService.ValidateDragDrop(this.Session.PermissionService, this.Thing, category, logger);
                return;
            }

            var termPayload = dropInfo.Payload as Term;
            var canDropTerm = this.PermissionService.CanWrite(ClassKind.Term, this.Thing);

            if (termPayload == null || !canDropTerm)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (termPayload.Container == this.Thing)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (termPayload.IDalUri.ToString() == this.Session.DataSourceUri)
            {
                // Move only possible within the glossary of the same chain of rdl
                var payloadRdl = (ReferenceDataLibrary)termPayload.Container.GetContainerOfType(typeof(ReferenceDataLibrary));
                var possibleNewRdls = payloadRdl.GetRequiredRdls().ToList();
                possibleNewRdls.Add(payloadRdl);

                var currentRdl = (ReferenceDataLibrary)this.Thing.Container;
                if (!possibleNewRdls.Contains(currentRdl))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                dropInfo.Effects = DragDropEffects.Move;
                return;
            }

            // different data-source
            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Category category)
            {
                var clone = this.Thing.Clone(false);
                clone.Category.Add(category);
                await this.DalWrite(clone);
                return;
            }

            var termPayload = dropInfo.Payload as Term;
            if (termPayload == null)
            {
                return;
            }

            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var termClone = termPayload.Clone(false);
                var currentGlossary = this.Thing.Clone(false);
                currentGlossary.Term.Add(termClone);

                var context = TransactionContextResolver.ResolveContext(this.Thing);
                var transaction = new ThingTransaction(context);

                transaction.CreateOrUpdate(currentGlossary);
                await this.DalWrite(transaction);
            }
            else if (dropInfo.Effects == DragDropEffects.Copy)
            {
                throw new NotImplementedException("drag and drop on a different data-source is not implemented yet.");
            }
        }

        /// <summary>
        /// Updates the columns values
        /// </summary>
        private void UpdateProperties()
        {
            var container = (ReferenceDataLibrary)this.Thing.Container;
            this.ContainerRdlShortName = container.ShortName;
            this.PopulateTerms();
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void PopulateTerms()
        {
            var current = this.ContainedRows.Select(x => x.Thing).OfType<Term>().ToList();
            var updated = this.Thing.Term;

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var term in added)
            {
                this.AddTermRowViewModel(term);
            }

            foreach (var term in removed)
            {
                this.RemoveTermRowViewModel(term);
            }
        }

        /// <summary>
        /// The add term row view model.
        /// </summary>
        /// <param name="term">
        /// The term.
        /// </param>
        private void AddTermRowViewModel(Term term)
        {
            var row = new TermRowViewModel(term, this.Session, this);
            this.ContainedRows.Add(row);
        }

        /// <summary>
        /// The remove term row view model.
        /// </summary>
        /// <param name="term">
        /// The term.
        /// </param>
        private void RemoveTermRowViewModel(Term term)
        {
            var row = this.ContainedRows.SingleOrDefault(x => x.Thing == term);
            if (row != null)
            {
                this.ContainedRows.RemoveAndDispose(row);
            }
        }
    }
}