﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels.OrganizationBrowser
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="Person"/> object that is contained in a <see cref="SiteDirectory"/>
    /// </summary>
    public class PersonRowViewModel : CDP4CommonView.PersonRowViewModel
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRowViewModel"/> class.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> that is being represented by the current row-view-model</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PersonRowViewModel(Person person, ISession session, IViewModelBase<Thing> containerViewModel) 
            : base(person, session, containerViewModel)
        {
            this.UpdateProperties();
        }
        #endregion

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
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            if (this.ContainerViewModel is OrganizationRowViewModel deprecatable)
            {
                var containerIsDeprecatedSubscription = deprecatable.WhenAnyValue(vm => vm.IsDeprecated)
                    .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Role != null)
            {
                this.RoleName = this.Role.Name;
                this.RoleShortName = this.Role.ShortName;
            }
            else
            {
                this.RoleName = string.Empty;
                this.RoleShortName = string.Empty;
            }

            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="OrganizationRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            if (this.ContainerViewModel is OrganizationRowViewModel deprecatable)
            {
                this.IsDeprecated = deprecatable.IsDeprecated;
            }
        }
    }
}
