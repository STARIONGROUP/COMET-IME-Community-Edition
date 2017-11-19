// -------------------------------------------------------------------------------------------------
// <copyright file="DefinitionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

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
    using CDP4Common.Operations;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Utilities;
    using CDP4Dal;

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
        public ReactiveCommand<object> CreateNoteCommand { get; private set; }

        /// <summary>
        /// Gets the delete note command.
        /// </summary>
        public ReactiveCommand<object> DeleteNoteCommand { get; private set; }

        /// <summary>
        /// Gets the create example command.
        /// </summary>
        public ReactiveCommand<object> CreateExampleCommand { get; private set; }

        /// <summary>
        /// Gets the delete example command.
        /// </summary>
        public ReactiveCommand<object> DeleteExampleCommand { get; private set; }

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
        /// Initializes the <see cref="ReactiveCommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateNoteCommand = ReactiveCommand.Create();
            this.CreateNoteCommand.Subscribe(_ => this.ExecuteCreateNoteCommand());

            this.DeleteNoteCommand = ReactiveCommand.Create();
            this.DeleteNoteCommand.Subscribe(_ => this.ExecuteDeleteNoteCommand());

            this.CreateExampleCommand = ReactiveCommand.Create();
            this.CreateExampleCommand.Subscribe(_ => this.ExecuteCreateExampleCommand());

            this.DeleteExampleCommand = ReactiveCommand.Create();
            this.DeleteExampleCommand.Subscribe(_ => this.ExecuteDeleteExampleCommand());

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
    }
}