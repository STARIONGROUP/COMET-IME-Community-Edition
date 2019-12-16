// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsGroupRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementsSpecificationEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Requirements.Utils;
    using ReactiveUI;

    /// <summary>
    /// The <see cref="RequirementsGroupRowViewModel"/> is the view-model that represents a <see cref="RequirementsGroup"/> in the <see cref="RequirementsSpecificationEditorViewModel"/>
    /// </summary>
    public class RequirementsGroupRowViewModel : CDP4CommonView.RequirementsGroupRowViewModel, IBreadCrumb
    {
        /// <summary>
        /// Backing field for the <see cref="BreadCrumb"/> property
        /// </summary>
        private string breadCrumb;

        /// <summary>
        /// Backing field for the <see cref="Path"/> property
        /// </summary>
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4CommonView.RequirementsGroupRowViewModel"/> class
        /// </summary>
        /// <param name="requirementsGroup">The <see cref="RequirementsGroup"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public RequirementsGroupRowViewModel(RequirementsGroup requirementsGroup, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(requirementsGroup, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the <see cref="BreadCrumb"/> property
        /// </summary>
        /// <remarks>
        /// The <see cref="BreadCrumb"/> property is used for sorting
        /// </remarks>
        public string BreadCrumb
        {
            get { return this.breadCrumb; }
            set { this.RaiseAndSetIfChanged(ref this.breadCrumb, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Path"/> property
        /// </summary>
        public string Path
        {
            get { return this.path; }
            set { this.RaiseAndSetIfChanged(ref this.path, value); }
        }

        /// <summary>
        /// Gets the supporting row property needed for deprecatable browsers.
        /// </summary>
        public bool IsDeprecated
        {
            get { return false; }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.OwnerShortName = this.Thing.Owner != null ? this.Thing.Owner.ShortName : string.Empty;

            this.Path = this.Thing.Path();
            this.BreadCrumb = this.Thing.BreadCrumb();
        }
    }
}
