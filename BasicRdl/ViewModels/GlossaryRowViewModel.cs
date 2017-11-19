﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlossaryRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="GlossaryRowViewModel"/> is to represent a <see cref="Glossary"/> object in a list or tree like structure
    /// </summary>
    public class GlossaryRowViewModel : CDP4CommonView.GlossaryRowViewModel, IDropTarget
    {
        #region Fields
        /// <summary>
        /// Backing field for <see cref="ContainerRdlShortName"/>
        /// </summary>
        private string containerRdlShortName;
        #endregion

        #region Constructors
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
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the Container RDL ShortName.
        /// </summary>
        public string ContainerRdlShortName 
        {
            get { return this.containerRdlShortName; }
            set { this.RaiseAndSetIfChanged(ref this.containerRdlShortName, value); }
        }
        #endregion

        #region RowViewModelBase Implementation
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
        #endregion

        #region IDropTarget
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
        #endregion

        #region Own Private Methods
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
                this.ContainedRows.Remove(row);
                row.Dispose();
            }
        }
        #endregion
    }
}
