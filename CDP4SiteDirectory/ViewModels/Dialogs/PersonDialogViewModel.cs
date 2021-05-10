// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="PersonDialogViewModel"/> is to allow a <see cref="Person"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Person"/> will result in an <see cref="Person"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Person)]
    public class PersonDialogViewModel : CDP4CommonView.PersonDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="PasswordConfirmation"/>
        /// </summary>
        private string passwordConfirmation;

        /// <summary>
        /// Backing field for <see cref="PwdEditIsChecked"/>
        /// </summary>
        private bool pwdEditIsChecked;

        /// <summary>
        /// The backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PersonDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDialogViewModel"/> class.
        /// </summary>
        /// <param name="person">
        /// The <see cref="Person"/> that is the subject of the current view-model. This is the object
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
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="container">The container <see cref="Thing"/> for the created <see cref="Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public PersonDialogViewModel(Person person, ThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(person, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.PwdEditIsChecked).Subscribe(x =>
            {
                this.RaisePropertyChanged("Password");
                this.RaisePropertyChanged("PasswordConfirmation");
            });
        }

        /// <summary>
        /// Gets or sets the password confirmation value
        /// </summary>
        public string PasswordConfirmation
        {
            get { return this.passwordConfirmation; }
            set { this.RaiseAndSetIfChanged(ref this.passwordConfirmation, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the password edit button is checked
        /// </summary>
        public bool PwdEditIsChecked
        {
            get { return this.pwdEditIsChecked; }
            set { this.RaiseAndSetIfChanged(ref this.pwdEditIsChecked, value); }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "PersonShortName")]
        public override string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to set the default <see cref="TelephoneNumber"/>
        /// </summary>
        public ReactiveCommand<object> SetDefaultTelephoneNumberCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to set the default <see cref="EmailAddress"/>
        /// </summary>
        public ReactiveCommand<object> SetDefaultEmailAddressCommand { get; private set; } 

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public override string this[string columnName]
        {
            get
            {
                if ((columnName == "Password" || columnName == "PasswordConfirmation") && !this.PwdEditIsChecked)
                {
                    var validationErrorToRemove =
                        this.ValidationErrors.SingleOrDefault(
                            x => x.PropertyName == "Password" || x.PropertyName == "PasswordConfirmation");
                    if (validationErrorToRemove != null)
                    {
                        this.ValidationErrors.Remove(validationErrorToRemove);
                    }

                    return null;
                }

                if (!this.PwdEditIsChecked || columnName != "PasswordConfirmation")
                {
                    return ValidationService.ValidateProperty(columnName, this);
                }

                if (string.IsNullOrWhiteSpace(this.PasswordConfirmation) || this.PasswordConfirmation != this.Password)
                {
                    var rule = new ValidationService.ValidationRule
                                   {
                                       PropertyName = "PasswordConfirmation",
                                       ErrorText = "The confirmation is different from the password entered."
                                   };

                    if (this.ValidationErrors.All(r => r.PropertyName != "PasswordConfirmation"))
                    {
                        this.ValidationErrors.Add(rule);
                    }

                    return rule.ErrorText;
                }

                // confirmation ok
                var validationError = this.ValidationErrors.SingleOrDefault(x => x.PropertyName == "PasswordConfirmation");
                if (validationError!= null)
                {
                    this.ValidationErrors.Remove(validationError);
                }

                return ValidationService.ValidateProperty(columnName, this);
            }
        }

        /// <summary>
        /// Initialize the commands
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.SetDefaultTelephoneNumberCommand =
                ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedTelephoneNumber).Select(x => x != null && !this.IsReadOnly));
            this.SetDefaultTelephoneNumberCommand.Subscribe(_ => this.ExecuteSetDefaultTelephoneNumberCommand());

            this.SetDefaultEmailAddressCommand =
                ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedEmailAddress).Select(x => x != null && !this.IsReadOnly));
            this.SetDefaultEmailAddressCommand.Subscribe(_ => this.ExecuteSetDefaultEmailAddressCommand());
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.IsActive = true;
            }

            this.Password = string.Empty;
        }

        /// <summary>
        /// Populate the <see cref="PersonDialogViewModel.PossibleDefaultDomain"/> property
        /// </summary>
        protected override void PopulatePossibleDefaultDomain()
        {
            base.PopulatePossibleDefaultDomain();
            this.PossibleDefaultDomain.AddRange(this.Session.RetrieveSiteDirectory().Domain.OrderBy(x => x.Name));
            this.SelectedDefaultDomain = this.Thing.DefaultDomain;
        }

        /// <summary>
        /// Populate the <see cref="PersonDialogViewModel.PossibleRole"/> property
        /// </summary>
        protected override void PopulatePossibleRole()
        {
            base.PopulatePossibleRole();
            this.PossibleRole.AddRange(this.Session.RetrieveSiteDirectory().PersonRole);
            this.SelectedRole = this.Thing.Role;
        }

        /// <summary>
        /// Populate the <see cref="PersonDialogViewModel.PossibleOrganization"/> property
        /// </summary>
        protected override void PopulatePossibleOrganization()
        {
            base.PopulatePossibleOrganization();
            this.PossibleOrganization.AddRange(this.Session.RetrieveSiteDirectory().Organization);
            this.SelectedOrganization = this.Thing.Organization;
        }

        /// <summary>
        /// Populate the email addresses
        /// </summary>
        protected override void PopulateEmailAddress()
        {
            this.EmailAddress.Clear();
            var defaultEmail = (this.Thing.DefaultEmailAddress == null) ? Guid.Empty : this.Thing.DefaultEmailAddress.Iid;
            foreach (var thing in this.Thing.EmailAddress.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new EmailAddressRowViewModel(thing, this.Session, this);
                this.EmailAddress.Add(row);
                row.IsDefault = thing.Iid == defaultEmail;
            }

            this.SelectedDefaultEmailAddress = this.Thing.DefaultEmailAddress;
        }

        /// <summary>
        /// Populate the phone numbers
        /// </summary>
        protected override void PopulateTelephoneNumber()
        {
            this.TelephoneNumber.Clear();
            var defaultPhone = (this.Thing.DefaultTelephoneNumber == null) ? Guid.Empty : this.Thing.DefaultTelephoneNumber.Iid;
            foreach (var thing in this.Thing.TelephoneNumber.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new TelephoneNumberRowViewModel(thing, this.Session, this);
                this.TelephoneNumber.Add(row);
                row.IsDefault = thing.Iid == defaultPhone;
            }

            this.SelectedDefaultTelephoneNumber = this.Thing.DefaultTelephoneNumber;
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            if (this.PwdEditIsChecked)
            {
                this.Thing.Password = this.Password;
            }
        }

        /// <summary>
        /// Executes the <see cref="SetDefaultTelephoneNumberCommand"/>
        /// </summary>
        private void ExecuteSetDefaultTelephoneNumberCommand()
        {
            foreach (var row in this.TelephoneNumber)
            {
                row.IsDefault = false;
            }

            this.SelectedTelephoneNumber.IsDefault = true;
            this.SelectedDefaultTelephoneNumber = this.SelectedTelephoneNumber.Thing;
            this.Thing.DefaultTelephoneNumber = this.SelectedDefaultTelephoneNumber;
        }

        /// <summary>
        /// Executes the <see cref="SetDefaultEmailAddressCommand"/>
        /// </summary>
        private void ExecuteSetDefaultEmailAddressCommand()
        {
            foreach (var row in this.EmailAddress)
            {
                row.IsDefault = false;
            }

            this.SelectedEmailAddress.IsDefault = true;
            this.SelectedDefaultEmailAddress = this.SelectedEmailAddress.Thing;
            this.Thing.DefaultEmailAddress = this.SelectedDefaultEmailAddress;
        }
    }
}