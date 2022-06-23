// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DialogViewModelBase.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.Exceptions;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4JsonSerializer;

    using DevExpress.Xpf.SpellChecker;
    using DevExpress.XtraSpellChecker;

    using ReactiveUI;

    /// <summary>
    /// Abstract base class from which all dialog view-models that represent a <see cref="Thing"/> need to derive
    /// </summary>
    /// <typeparam name="T">
    /// A type of Thing that is represented by the view-model
    /// </typeparam>
    public abstract class DialogViewModelBase<T> : ViewModelBase<T>, IDialogViewModelBase<T> where T : Thing
    {
        /// <summary>
        /// The relative location of the loading animation image.
        /// </summary>
        private const string CdpLogoAnimationPath = @"\Resources\Images\comet.ico";

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/> used to navigate to <see cref="IThingDialogViewModel"/>s
        /// </summary>
        protected readonly IThingDialogNavigationService thingDialogNavigationService;

        /// <summary>
        /// The parent <see cref="ThingTransaction"/> that records the changes that are executed on the <see cref="Thing"/>
        /// that is the subject of this view-model
        /// </summary>
        protected IThingTransaction parentTransaction;

        /// <summary>
        /// The <see cref="ThingTransaction"/> that records the changes that happens in this view-model and its contained items
        /// </summary>
        protected IThingTransaction transaction;

        /// <summary>
        /// Backing field for the <see cref="DialogResult"/> property.
        /// </summary>
        protected bool? dialogResult;

        /// <summary>
        /// Out property for the <see cref="HasException"/> property
        /// </summary>
        protected ObservableAsPropertyHelper<bool> hasException;

        /// <summary>
        /// The <see cref="Exception"/> that has been caught trying to 
        /// </summary>
        protected Exception writeException;

        /// <summary>
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </summary>
        protected bool isRoot;

        /// <summary>
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </summary>
        protected ThingDialogKind dialogKind;

        /// <summary>
        /// Backing field for <see cref="OkCanExecute"/> property.
        /// </summary>
        private bool okCanExecute;

        /// <summary>
        /// Backing field for the <see cref="SpellChecker"/> property.
        /// </summary>
        private SpellChecker spellChecker;

        /// <summary>
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this dialog
        /// </summary>
        private Thing container;

        /// <summary>
        /// The <see cref="ISpellDictionaryService"/> that is used to set the dictionaries of the <see cref="SpellChecker"/>
        /// </summary>
        private ISpellDictionaryService dictionaryService;

        /// <summary>
        /// The <see cref="Cdp4JsonSerializer"/> that is used to serialize an <see cref="Thing"/> to the clipboard
        /// </summary>
        private readonly Cdp4JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewModelBase{T}"/> class
        /// </summary>
        /// <param name="thingClone">The clone of the <see cref="Thing"/> that this dialog belongs to.</param>
        /// <param name="parentTransaction">
        /// The parent <see cref="ThingTransaction"/> that contains the log of recorded changes at the previous level.
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
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">The <see cref="Thing"/> that contains the created one in this <see cref="DialogViewModelBase{T}"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        /// <remarks>
        /// <paramref name="thingClone"/> shall always be a clone when opening a dialog to create or edit a thing.
        /// </remarks>
        protected DialogViewModelBase(T thingClone, IThingTransaction parentTransaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers)
            : base(thingClone, session)
        {
            // add the animation uri path            
            this.AnimationUri = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}{CdpLogoAnimationPath}";
            this.LoadingMessage = "Processing...";

            this.InitializeSpellingChecker();

            this.thingDialogNavigationService = thingDialogNavigationService;

            this.PossibleContainer = new ReactiveList<Thing>();
            this.Container = container;
            this.parentTransaction = parentTransaction;
            this.isRoot = isRoot;
            this.dialogKind = dialogKind;
            this.serializer = new Cdp4JsonSerializer(this.Session.Dal.MetaDataProvider, this.Session.DalVersion);

            this.ChainOfContainer = chainOfContainers != null ? chainOfContainers.ToList() : new List<Thing>();
            this.ChainOfContainer.Add(this.Container);

            this.WhenAnyValue(vm => vm.Container)
                .Where(x => x != null)
                .Subscribe(_ => this.UpdateChainOfContainer());

            this.Initialize();
            this.InitializeCommands();
            this.UpdateProperties();

            this.WhenAnyValue(x => x.WriteException).Select(x => x != null).ToProperty(this, x => x.HasException, out this.hasException);

            switch (this.dialogKind)
            {
                case ThingDialogKind.Create:
                    this.transaction = new ThingTransaction(this.Thing, parentTransaction, container);
                    break;
                case ThingDialogKind.Update:
                    this.transaction = new ThingTransaction(this.Thing, parentTransaction, container);
                    break;
                case ThingDialogKind.Inspect:
                    break;
                default:
                    throw new InvalidEnumArgumentException("The provided dialogKind is Invalid");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewModelBase{T}"/> class
        /// </summary>
        protected DialogViewModelBase()
        {
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that represents an "confirmation" of the dialog
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that represents an "cancellation" of the dialog
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that represents an copy of the Uri to Clipboard
        /// </summary>
        public ReactiveCommand<Unit, Unit> CopyUriCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that represents a shallow export to Clipboard
        /// </summary>
        public ReactiveCommand<Unit, Unit> ShallowExportCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that represents a deep export to Clipboard
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeepExportCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OkCommand"/> can be executed
        /// </summary>
        public bool OkCanExecute
        {
            get => this.okCanExecute;
            set => this.RaiseAndSetIfChanged(ref this.okCanExecute, value);
        }

        /// <summary>
        /// Gets or sets the container of the <see cref="Thing"/> in this dialog
        /// </summary>
        public Thing Container
        {
            get => this.container;
            set => this.RaiseAndSetIfChanged(ref this.container, value);
        }

        /// <summary>
        /// Gets the chain of <see cref="Thing"/>s that contains the current one
        /// </summary>
        public List<Thing> ChainOfContainer { get; private set; }

        /// <summary>
        /// Gets the <see cref="SpellChecker"/> instance that the <see cref="SpellingCheckerService"/> provides
        /// </summary>
        public SpellChecker SpellChecker => this.spellChecker;

        /// <summary>
        /// Gets the animation uri
        /// </summary>
        public string AnimationUri { get; private set; }

        /// <summary>
        /// Gets the Loading Message to display when teh dialog is busy
        /// </summary>
        public string LoadingMessage { get; private set; }

        /// <summary>
        /// Gets the possible container for the <see cref="Thing"/> in this dialog
        /// </summary>
        public ReactiveList<Thing> PossibleContainer { get; private set; }

        /// <summary>
        /// Gets the inspect command for the selected container
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedContainerCommand { get; private set; }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// Gets or sets an error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ValidationService.ValidationRule"/>s that are violated.
        /// </summary>
        public ReactiveList<ValidationService.ValidationRule> ValidationErrors { get; set; }

        /// <summary>
        /// Gets a value indicating whether the associated view is read-only
        /// </summary>
        public virtual bool IsReadOnly => this.dialogKind == ThingDialogKind.Inspect;

        /// <summary>
        /// Gets a value indicating whether a non-editable field is read-only
        /// </summary>
        public virtual bool IsNonEditableFieldReadOnly => this.dialogKind != ThingDialogKind.Create;

        /// <summary>
        /// Gets a value indicating whether the Ok button is visible
        /// </summary>
        public bool IsOkVisible => !this.IsReadOnly;

        /// <summary>
        /// Gets a value indicating whether the <see cref="writeException"/> is not empty.
        /// </summary>
        public bool HasException => this.hasException.Value;

        /// <summary>
        /// Gets or sets a value that forces the window to close, specifying whether the user
        /// cancelled or completed the edit.
        /// </summary>
        /// <remarks>
        /// Setting to null does not force the window to close.
        /// </remarks>
        public bool? DialogResult
        {
            get => this.dialogResult;

            set => this.RaiseAndSetIfChanged(ref this.dialogResult, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="WriteException"/> that may have occurred during a
        /// Write operation on the current <see cref="Session"/>.
        /// </summary>
        public Exception WriteException
        {
            get => this.writeException;

            set => this.RaiseAndSetIfChanged(ref this.writeException, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="IDictionaryService"/>
        /// </summary>
        public ISpellDictionaryService DictionaryService
        {
            get => this.dictionaryService;

            set
            {
                if (this.dictionaryService != null)
                {
                    this.dictionaryService.CultureInfoChanged -= this.DictionaryServiceOnCultureInfoChanged;
                }

                this.dictionaryService = value;

                if (this.dictionaryService != null)
                {
                    this.dictionaryService.CultureInfoChanged += this.DictionaryServiceOnCultureInfoChanged;
                    this.SetSpellCheckerDictonary(this.dictionaryService.ActiveDictionary, this.dictionaryService.ActiveCulture);
                }
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        /// <remarks>
        /// Used by the view through the IDataErrorInfo interface to validate a field
        /// </remarks>
        public virtual string this[string columnName] => ValidationService.ValidateProperty(columnName, this);

        /// <summary>
        /// Gets the <see cref="Uri"/> of the <see cref="Thing"/> with respect to it's data-source
        /// </summary>
        public string ThingUri { get; private set; }

        /// <summary>
        /// Updates the property indicating whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        /// <remarks>
        /// The <see cref="Container"/> may not be null and there may not be any Validation Errors
        /// </remarks>
        protected virtual void UpdateOkCanExecute()
        {
            this.OkCanExecute = this.Container != null && (!this.ValidationErrors?.Any() ?? false);
        }

        /// <summary>
        /// Initialize the dialog class
        /// </summary>
        protected virtual void Initialize()
        {
            logger.Trace("DialogViewModelBase: Initialize");
        }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommandCreator"/>s of the current view-model
        /// </summary>
        protected virtual void InitializeCommands()
        {
            this.ValidationErrors = new ReactiveList<ValidationService.ValidationRule>();

            this.ValidationErrors.Changed.Subscribe(_ => this.UpdateOkCanExecute());

            var canExecute = this.WhenAnyValue(vm => vm.OkCanExecute);

            this.OkCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteOkCommand, canExecute, RxApp.MainThreadScheduler);

            this.OkCommand.ThrownExceptions.Subscribe(
                x =>
                {
                    this.WriteException = x;
                    logger.Error(x);
                });

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancelCommand);

            this.CopyUriCommand = ReactiveCommandCreator.Create(this.ExecuteCopyUriCommand);

            this.ShallowExportCommand = ReactiveCommandCreator.Create(this.ExecuteShallowExportCommand);

            this.DeepExportCommand = ReactiveCommandCreator.Create(this.ExecuteDeepExportCommand);

            this.InspectSelectedContainerCommand = ReactiveCommandCreator.Create(() => this.ExecuteInspectCommand(this.Container));
        }

        /// <summary>
        /// Initialize the SpellChecker for the current View Model
        /// </summary>
        private void InitializeSpellingChecker()
        {
            this.spellChecker = new SpellChecker();
            this.spellChecker.SpellCheckMode = SpellCheckMode.AsYouType;
        }

        /// <summary>
        /// Set the <see cref="SpellCheckerOpenOfficeDictionary"/> and <see cref="CultureInfo"/> of the <see cref="SpellChecker"/>
        /// </summary>
        /// <param name="dictionary">
        /// The <see cref="SpellCheckerOpenOfficeDictionary"/> that is to be added to the <see cref="SpellChecker"/> if it is not yet added.
        /// </param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> that is the active <see cref="CultureInfo"/> of the <see cref="SpellChecker"/>
        /// </param>        
        private void SetSpellCheckerDictonary(SpellCheckerOpenOfficeDictionary dictionary, CultureInfo culture)
        {
            if (!this.spellChecker.Dictionaries.Contains(dictionary))
            {
                this.spellChecker.Dictionaries.Add(dictionary);
            }

            this.spellChecker.Culture = culture;
        }

        /// <summary>
        /// Event handler for the CultureInfoChanged of the <see cref="DictionaryService"/>
        /// </summary>
        /// <param name="source">
        /// The source of the event
        /// </param>
        /// <param name="cultureInfoChangedEventArgs">
        /// The event arguments that carry the <see cref="CultureInfo"/> and <see cref="SpellCheckerOpenOfficeDictionary"/>
        /// </param>
        private void DictionaryServiceOnCultureInfoChanged(object source, CultureInfoChangedEventArgs cultureInfoChangedEventArgs)
        {
            if (this.spellChecker == null)
            {
                return;
            }

            this.SetSpellCheckerDictonary(cultureInfoChangedEventArgs.Dictionary, cultureInfoChangedEventArgs.Culture);
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>.
        /// </summary>
        /// <remarks>
        /// The changes that have been set on the view-model will not be set on the <see cref="Thing"/>,
        /// the <see cref="ThingTransaction"/> is not updated with any of these changes.
        /// </remarks>
        protected virtual void ExecuteCancelCommand()
        {
            logger.Trace("Create or Edit on {0}:{1} has been cancelled", this.Thing.ClassKind, this.Thing.Iid);
            this.DialogResult = false;
        }

        /// <summary>
        /// Executes the <see cref="CopyUriCommand"/>.
        /// </summary>
        protected virtual void ExecuteCopyUriCommand()
        {
            Clipboard.SetText(this.ThingUri);
        }

        /// <summary>
        /// Executes the <see cref="ShallowExportCommand"/>.
        /// </summary>
        protected virtual void ExecuteShallowExportCommand()
        {
            Clipboard.SetText(this.serializer.SerializeToString(this.Thing, false));
        }

        /// <summary>
        /// Executes the <see cref="DeepExportCommand"/>.
        /// </summary>
        protected virtual void ExecuteDeepExportCommand()
        {
            Clipboard.SetText(this.serializer.SerializeToString(this.Thing, true));
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteOkCommand()
        {
            this.UpdateTransaction();

            if (this.dialogKind == ThingDialogKind.Create || this.dialogKind == ThingDialogKind.Update)
            {
                this.transaction.FinalizeSubTransaction(this.Thing, this.Container);
            }

            if (!this.isRoot)
            {
                this.DialogResult = true;
                return;
            }

            var operationContainer = this.parentTransaction.FinalizeTransaction();
            var files = this.parentTransaction.GetFiles();

            try
            {
                this.IsBusy = true;

                if (files?.Any() ?? false)
                {
                    await this.Session.Write(operationContainer, files);
                }
                else
                {
                    await this.Session.Write(operationContainer);
                }

                this.DialogResult = true;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Updates the <see cref="ThingTransaction"/> with the changes recorded in the current view-model
        /// </summary>
        protected virtual void UpdateTransaction()
        {
            this.Thing.ModifiedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Update the properties of the current view-model
        /// </summary>
        protected virtual void UpdateProperties()
        {
            logger.Trace("DialogViewModelBase.UpdateProperties called.");
            this.UpdateOkCanExecute();

            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.ThingUri = "N/A";
                return;
            }

            try
            {
                var dto = this.Thing.ToDto();
                var dtoRoute = dto.Route;

                if (this.IDalUri != null)
                {
                    var uriBuilder = new UriBuilder(this.IDalUri) { Path = dtoRoute };
                    this.ThingUri = uriBuilder.ToString();
                }
                else
                {
                    this.ThingUri = "N/A";
                }
            }
            catch (ContainmentException ex)
            {
                this.ThingUri = ex.Message;
                logger.Warn(ex);
            }
        }

        /// <summary>
        /// Execute the Create Command for a <see cref="Thing"/>
        /// </summary>
        /// <typeparam name="TThing">The type of <see cref="Thing"/> to create</typeparam>
        /// <param name="populateMethod">The populate method which clear the <see cref="ReactiveList{T}"/> the new <see cref="Thing"/> is contained in and populates it</param>
        protected void ExecuteCreateCommand<TThing>(Action populateMethod) where TThing : Thing, new()
        {
            var thing = new TThing();
            var result = this.thingDialogNavigationService.Navigate(thing, this.transaction, this.Session, false, ThingDialogKind.Create, this.thingDialogNavigationService, this.Thing, this.ChainOfContainer);

            if (!result.HasValue || !result.Value)
            {
                return;
            }

            populateMethod();
        }

        /// <summary>
        /// Execute the Delete command for a <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">the <see cref="Thing"/> to delete</param>
        /// <param name="repopulateMethod">the repopulate method to refresh the rows</param>
        protected void ExecuteDeleteCommand(Thing thing, Action repopulateMethod)
        {
            this.transaction.Delete(thing.Clone(false), this.Thing);
            repopulateMethod();
        }

        /// <summary>
        /// Execute the Edit command for a <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">the <see cref="Thing"/> to edit</param>
        /// <param name="repopulateAction">The method that repopulates the list that contains the row associated with the updated thing</param>
        protected void ExecuteEditCommand(Thing thing, Action repopulateAction)
        {
            var result = this.thingDialogNavigationService.Navigate(thing.Clone(false), this.transaction, this.Session, false, ThingDialogKind.Update, this.thingDialogNavigationService, this.Thing, this.ChainOfContainer);

            if (!result.HasValue || !result.Value)
            {
                return;
            }

            repopulateAction();
        }

        /// <summary>
        /// Execute the "Move Up" Command which puts a selected item one level up
        /// </summary>
        /// <typeparam name="TRow">
        /// The type of row
        /// </typeparam>
        /// <param name="orderedList">
        /// The list of ordered rows
        /// </param>
        /// <param name="selectedItem">
        /// the row to move
        /// </param>
        protected void ExecuteMoveUpCommand<TRow>(ReactiveList<TRow> orderedList, TRow selectedItem) where TRow : IRowViewModelBase<Thing>
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
        /// <typeparam name="TRow">
        /// The kind of row
        /// </typeparam>
        /// <param name="orderedList">
        /// The list of ordered rows
        /// </param>
        /// <param name="selectedItem">
        /// the row to move
        /// </param>
        protected void ExecuteMoveDownCommand<TRow>(ReactiveList<TRow> orderedList, TRow selectedItem) where TRow : IRowViewModelBase<Thing>
        {
            var selectedIndex = orderedList.IndexOf(selectedItem);

            if (selectedIndex == orderedList.Count - 1)
            {
                return;
            }

            orderedList.Move(selectedIndex, selectedIndex + 1);
        }

        /// <summary>
        /// Execute the Inspect command for a <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">the <see cref="Thing"/> to inspect</param>
        protected void ExecuteInspectCommand(Thing thing)
        {
            this.thingDialogNavigationService.Navigate(thing, this.transaction, this.Session, false, ThingDialogKind.Inspect, this.thingDialogNavigationService, thing.Container);
        }

        /// <summary>
        /// Update the chain of container by adding the current selected one
        /// </summary>
        private void UpdateChainOfContainer()
        {
            var lastitem = this.ChainOfContainer.Last();
            this.ChainOfContainer.Remove(lastitem);
            this.ChainOfContainer.Add(this.Container);
        }
    }
}
