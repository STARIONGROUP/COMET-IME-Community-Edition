// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinitionDialogViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4CommonView.ViewModels
{
    using System;
    using CDP4CommonView;
    using ReactiveUI;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Utilities;
    using CDP4Dal;
    using System.Reactive;

    [ThingDialogViewModelExport(ClassKind.Definition)]
    public class DefinitionDialogViewModel : CDP4CommonView.DefinitionDialogViewModel, IThingDialogViewModel
    {
        #region Field
        /// <summary>
        /// Backing field for the <see cref="SelectedNote"/> property.
        /// </summary>
        private PrimitiveRow<string> selectedNote;

        /// <summary>
        /// Backing field for <see cref="SelectedLanguageCode"/>
        /// </summary>
        private LanguageCodeUsage selectedLanguageCode;

        /// <summary>
        /// Backing field for the <see cref="SelectedExample"/> property.
        /// </summary>
        private PrimitiveRow<string> selectedExample;
        #endregion

        #region Constructor
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
        /// Initializes a new instance of the <see cref="DefinitionDialogViewModel"/> class.
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
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The <see cref="DefinitionDialogViewModel.container"/> for this <see cref="DefinitionDialogViewModel.Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public DefinitionDialogViewModel(Definition definition, ThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(definition, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the create note command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateNoteCommand { get; private set; }

        /// <summary>
        /// Gets the delete note command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteNoteCommand { get; private set; }

        /// <summary>
        /// Gets the move up note command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpNoteCommand { get; private set; }

        /// <summary>
        /// Gets the move down note command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownNoteCommand { get; private set; }

        /// <summary>
        /// Gets the create example command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateExampleCommand { get; private set; }

        /// <summary>
        /// Gets the delete example command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteExampleCommand { get; private set; }

        /// <summary>
        /// Gets the move up example command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpExampleCommand { get; private set; }

        /// <summary>
        /// Gets the move down example command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownExampleCommand { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> for this definition
        /// </summary>
        public LanguageCodeUsage SelectedLanguageCode
        {
            get { return this.selectedLanguageCode; }
            set { this.RaiseAndSetIfChanged(ref this.selectedLanguageCode, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="LanguageCodeUsage"/>
        /// </summary>
        public ReactiveList<LanguageCodeUsage> PossibleLanguageCode { get; private set; }
        #endregion

        /// <summary>
        /// Initializes the view-model
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleLanguageCode = new ReactiveList<LanguageCodeUsage>();
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommandCreator"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteSelectedMoveNoteCommand = this.WhenAny(vm => vm.SelectedNote, v => v.Value != null && !this.IsReadOnly);
            var canExecuteSelectedMoveExampleCommand = this.WhenAny(vm => vm.SelectedExample, v => v.Value != null && !this.IsReadOnly);

            this.CreateNoteCommand = ReactiveCommandCreator.Create(this.ExecuteCreateNoteCommand);
            this.DeleteNoteCommand = ReactiveCommandCreator.Create(this.ExecuteDeleteNoteCommand);
            this.MoveUpNoteCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveUpCommand(this.Note, this.SelectedNote), canExecuteSelectedMoveNoteCommand);
            this.MoveDownNoteCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveDownCommand(this.Note, this.SelectedNote), canExecuteSelectedMoveNoteCommand);
            this.MoveUpExampleCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveUpCommand(this.Example, this.SelectedExample), canExecuteSelectedMoveExampleCommand);
            this.MoveDownExampleCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveDownCommand(this.Example, this.SelectedExample), canExecuteSelectedMoveExampleCommand);
            this.CreateExampleCommand = ReactiveCommandCreator.Create(this.ExecuteCreateExampleCommand);
            this.DeleteExampleCommand = ReactiveCommandCreator.Create(this.ExecuteDeleteExampleCommand);

            this.WhenAnyValue(x => x.SelectedLanguageCode).Where(x => x != null).Subscribe(culture => this.LanguageCode = culture.Name);
        }

        /// <summary>
        /// Update the property of the dialog
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            
            var definedThing = (DefinedThing)this.Container;
            this.PossibleLanguageCode.Clear();
            var usedCodes = definedThing.Definition.Select(x => x.LanguageCode);            
            var languageCodeUsages = new List<LanguageCodeUsage>();
            foreach (var usedCode in usedCodes)
            {
                if (usedCode == null)
                {
                    continue;
                }

                var cultureInfo = CultureInfoUtility.CultureInfoAvailable.SingleOrDefault(x => x.Name == usedCode);

                if (cultureInfo == null)
                {
                    try
                    {
                        cultureInfo = new CultureInfo(usedCode);
                        var languageCodeUsage = new LanguageCodeUsage(cultureInfo, true);
                        languageCodeUsages.Add(languageCodeUsage);
                    }
                    catch (CultureNotFoundException ex)
                    {
                        var languageCodeUsage = new LanguageCodeUsage(usedCode, usedCode, true);
                        languageCodeUsages.Add(languageCodeUsage);
                        logger.Debug(ex, "The culture {0} could not be found and is ignored", usedCode);
                    }
                }
            }
            
            this.PossibleLanguageCode.AddRange(languageCodeUsages.OrderBy(x => x.FullName));

            foreach (var cultureInfo in CultureInfoUtility.CultureInfoAvailable)
            {
                if (languageCodeUsages.All(x => x.Name != cultureInfo.Name))
                {
                    var languageCodeUsage = new LanguageCodeUsage(cultureInfo, false);
                    this.PossibleLanguageCode.Add(languageCodeUsage);
                }
            }

            if (string.IsNullOrEmpty(this.Thing.LanguageCode))
            {
                this.SelectedLanguageCode = this.PossibleLanguageCode.Single(x => x.Name == CultureInfoUtility.DefaultCultureName);
            }
            else
            {
                this.SelectedLanguageCode = this.PossibleLanguageCode.Single(x => x.Name == this.Thing.LanguageCode);
            }
        }

        /// <summary>
        /// Executes the delete note command.
        /// </summary>
        private void ExecuteDeleteNoteCommand()
        {
            if (this.SelectedNote != null)
            {
                this.Note.Remove(this.SelectedNote);
                this.SelectedNote = null;
            }
        }

        /// <summary>
        /// Executes the create note command.
        /// </summary>
        private void ExecuteCreateNoteCommand()
        {
            var newNoteRow = new PrimitiveRow<string>
            {
                Index = this.Note.Count + 1,
                Value = string.Format("Note{0}", this.Note.Count + 1)
            };

            this.Note.Add(newNoteRow);

            // select in grid
            this.SelectedNote = newNoteRow;
        }

        /// <summary>
        /// Gets or sets the selected note row.
        /// </summary>
        public PrimitiveRow<string> SelectedNote
        {
            get
            {
                return this.selectedNote;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedNote, value);
            }
        }

        /// <summary>
        /// Executes the delete example command.
        /// </summary>
        private void ExecuteDeleteExampleCommand()
        {
            if (this.SelectedExample != null)
            {
                this.Example.Remove(this.SelectedExample);
                this.SelectedExample = null;
            }
        }

        /// <summary>
        /// Executes the create example command.
        /// </summary>
        private void ExecuteCreateExampleCommand()
        {
            var newExampleRow = new PrimitiveRow<string>
            {
                Index = this.Example.Count + 1,
                Value = string.Format("Example{0}", this.Example.Count + 1)
            };

            this.Example.Add(newExampleRow);

            // select in grid
            this.SelectedExample = newExampleRow;
        }

        /// <summary>
        /// Gets or sets the selected example row.
        /// </summary>
        public PrimitiveRow<string> SelectedExample
        {
            get
            {
                return this.selectedExample;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedExample, value);
            }
        }

        /// <summary>
        /// Execute the "Move Up" Command which puts a selected item one level up
        /// </summary>
        /// <param name="orderedList">
        /// The list of ordered rows
        /// </param>
        /// <param name="selectedItem">
        /// the row to move
        /// </param>
        private void ExecuteMoveUpCommand(ReactiveList<PrimitiveRow<string>> orderedList, PrimitiveRow<string> selectedItem)
        {
            var selectedIndex = orderedList.IndexOf(selectedItem);

            if (selectedIndex == 0)
            {
                return;
            }

            orderedList.Move(selectedIndex, selectedIndex - 1);
        }

        /// <summary>
        /// Execute the "Move Down" Command which puts a selected item one level down
        /// </summary>
        /// <param name="orderedList">
        /// The list of ordered rows
        /// </param>
        /// <param name="selectedItem">
        /// the row to move
        /// </param>
        protected void ExecuteMoveDownCommand(ReactiveList<PrimitiveRow<string>> orderedList, PrimitiveRow<string> selectedItem)
        {
            var selectedIndex = orderedList.IndexOf(selectedItem);
            if (selectedIndex == orderedList.Count - 1)
            {
                return;
            }

            orderedList.Move(selectedIndex, selectedIndex + 1);
        }
    }
}