// -------------------------------------------------------------------------------------------------
// <copyright file="DefinitionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Kamil Wojnowski, Nathanael Smiechowski
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
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Definition"/>
    /// </summary>
    public partial class DefinitionDialogViewModel : DialogViewModelBase<Definition>
    {
        /// <summary>
        /// Backing field for <see cref="LanguageCode"/>
        /// </summary>
        private string languageCode;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Backing field for <see cref="SelectedCitation"/>
        /// </summary>
        private CitationRowViewModel selectedCitation;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DefinitionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionDialogViewModel"/> class
        /// </summary>
        /// <param name="definition">
        /// The <see cref="Definition"/> that is the subject of the current view-model. This is the object
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
        public DefinitionDialogViewModel(Definition definition, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(definition, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as DefinedThing;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type DefinedThing",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the LanguageCode
        /// </summary>
        public virtual string LanguageCode
        {
            get { return this.languageCode; }
            set { this.RaiseAndSetIfChanged(ref this.languageCode, value); }
        }

        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        public virtual string Content
        {
            get { return this.content; }
            set { this.RaiseAndSetIfChanged(ref this.content, value); }
        }

        /// <summary>
        /// Backing field for Note
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> note;

        /// <summary>
        /// Gets or sets the Note
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Note
        {
            get { return this.note; }
            set { this.RaiseAndSetIfChanged(ref this.note, value); }
        }

        /// <summary>
        /// Backing field for Example
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> example;

        /// <summary>
        /// Gets or sets the Example
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Example
        {
            get { return this.example; }
            set { this.RaiseAndSetIfChanged(ref this.example, value); }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="CitationRowViewModel"/>
        /// </summary>
        public CitationRowViewModel SelectedCitation
        {
            get { return this.selectedCitation; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCitation, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Citation"/>
        /// </summary>
        public ReactiveList<CitationRowViewModel> Citation { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Citation
        /// </summary>
        public ReactiveCommand<object> CreateCitationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Citation
        /// </summary>
        public ReactiveCommand<object> DeleteCitationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Citation
        /// </summary>
        public ReactiveCommand<object> EditCitationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Citation
        /// </summary>
        public ReactiveCommand<object> InspectCitationCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteCreateCitationCommand = this.WhenAny(vm => vm.ChainOfContainer, v => v.Value.Any(x => x is ReferenceDataLibrary));
            var canExecuteInspectSelectedCitationCommand = this.WhenAny(vm => vm.SelectedCitation, v => v.Value != null);
            var canExecuteEditSelectedCitationCommand = this.WhenAny(vm => vm.SelectedCitation, v => v.Value != null && !this.IsReadOnly);

            this.CreateCitationCommand = ReactiveCommand.Create(canExecuteCreateCitationCommand);
            this.CreateCitationCommand.Subscribe(_ => this.ExecuteCreateCommand<Citation>(this.PopulateCitation));

            this.DeleteCitationCommand = ReactiveCommand.Create(canExecuteEditSelectedCitationCommand);
            this.DeleteCitationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedCitation.Thing, this.PopulateCitation));

            this.EditCitationCommand = ReactiveCommand.Create(canExecuteEditSelectedCitationCommand);
            this.EditCitationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedCitation.Thing, this.PopulateCitation));

            this.InspectCitationCommand = ReactiveCommand.Create(canExecuteInspectSelectedCitationCommand);
            this.InspectCitationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedCitation.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.LanguageCode = this.LanguageCode;
            clone.Content = this.Content;
            clone.Note.Clear();
            clone. Note.AddOrderedItems(this.Note.Select(x => new OrderedItem { K = x.Index, V = x.Value }));
 
            clone.Example.Clear();
            clone. Example.AddOrderedItems(this.Example.Select(x => new OrderedItem { K = x.Index, V = x.Value }));
 
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Note = new ReactiveList<PrimitiveRow<string>>();
            this.Example = new ReactiveList<PrimitiveRow<string>>();
            this.Citation = new ReactiveList<CitationRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.LanguageCode = this.Thing.LanguageCode;
            this.Content = this.Thing.Content;
            this.PopulateNote();
            this.PopulateExample();
            this.PopulateCitation();
        }

        /// <summary>
        /// Populates the <see cref="Note"/> property
        /// </summary>
        protected virtual void PopulateNote()
        {
            this.Note.Clear();
            foreach(var value in this.Thing.Note.SortedItems)
            {
                this.Note.Add(new PrimitiveRow<string> { Index = value.Key, Value = value.Value });
            }
        }

        /// <summary>
        /// Populates the <see cref="Example"/> property
        /// </summary>
        protected virtual void PopulateExample()
        {
            this.Example.Clear();
            foreach(var value in this.Thing.Example.SortedItems)
            {
                this.Example.Add(new PrimitiveRow<string> { Index = value.Key, Value = value.Value });
            }
        }

        /// <summary>
        /// Populates the <see cref="Citation"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateCitation()
        {
            this.Citation.Clear();
            foreach (var thing in this.Thing.Citation.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new CitationRowViewModel(thing, this.Session, this);
                this.Citation.Add(row);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        /// <remarks>
        /// This method is called by the <see cref="ThingDialogNavigationService"/> when the Dialog is closed
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach(var citation in this.Citation)
            {
                citation.Dispose();
            }
        }
    }
}
