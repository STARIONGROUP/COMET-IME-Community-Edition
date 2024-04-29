// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TermRowViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    /// <summary>
    /// The term row view model.
    /// </summary>
    public class TermRowViewModel : CDP4CommonView.TermRowViewModel, IDropTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TermRowViewModel"/> class.
        /// </summary>
        /// <param name="term">The term</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public TermRowViewModel(Term term, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(term, session, containerViewModel)
        {
            this.UpdateProperties();
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
        /// Updates this row values
        /// </summary>
        private void UpdateProperties()
        {
            // TODO: use the first definition in the language that the user has selected to view the items in
            if (this.Thing.Definition.Any())
            {
                this.DefinitionValue = this.Thing.Definition.First().Content;
            }
        }

        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        public override void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Payload = this.Thing;
            dragInfo.Effects = DragDropEffects.Move;
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
            var termPayload = dropInfo.Payload as Term;
            var canDropTerm = this.PermissionService.CanWrite(ClassKind.Term, this.Thing);

            if (termPayload == null || !canDropTerm)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            // if container is the same
            if (termPayload.Container == this.Thing.Container)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (termPayload.IDalUri.ToString() == this.Session.DataSourceUri)
            {
                // Move only possible within the glossary of the same chain of rdl
                var payloadRdl = (ReferenceDataLibrary)termPayload.GetContainerOfType(typeof(ReferenceDataLibrary));
                var possibleNewRdls = payloadRdl.GetRequiredRdls().ToList();
                possibleNewRdls.Add(payloadRdl);

                var currentRdl = this.Thing.GetContainerOfType(typeof(ReferenceDataLibrary));
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
                var currentGlossary = (Glossary)this.Thing.Container.Clone(false);
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
    }
}
