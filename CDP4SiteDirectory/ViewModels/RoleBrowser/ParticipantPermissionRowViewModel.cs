// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantPermissionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// A row-view-model that represents a <see cref="PersonPermission"/> 
    /// </summary>
    public class ParticipantPermissionRowViewModel : CDP4CommonView.ParticipantPermissionRowViewModel
    {
        /// <summary>
        /// The camel case to space converter.
        /// </summary>
        private readonly CamelCaseToSpaceConverter camelCaseToSpaceConverter = new CamelCaseToSpaceConverter();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantPermissionRowViewModel"/> class.
        /// </summary>
        /// <param name="permission">The <see cref="ParticipantPermission"/> that is represented by the current row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParticipantPermissionRowViewModel(ParticipantPermission permission, ISession session,
            IViewModelBase<Thing> containerViewModel)
            : base(permission, session, containerViewModel)
        {
            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
        }

        /// <summary>
        /// Gets the ClassKind to display it in the "Name" column of the Role browser.
        /// </summary>
        public string Name
        {
            get { return this.camelCaseToSpaceConverter.Convert(this.ObjectClass, null, null, null).ToString(); }
        }

        /// <summary>
        /// Gets the ClassKind to display it in the "Name" column of the Role browser.
        /// </summary>
        public ClassKind ShortName
        {
            get { return this.ObjectClass; }
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            if (this.ContainerViewModel is ParticipantRoleRowViewModel deprecatable)
            {
                var containerIsDeprecatedSubscription = deprecatable.WhenAnyValue(vm => vm.IsDeprecated)
                    .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="PersonRoleRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            if (this.ContainerViewModel is ParticipantRoleRowViewModel deprecatable)
            {
                this.IsDeprecated = deprecatable.IsDeprecated;
            }
        }
    }
}