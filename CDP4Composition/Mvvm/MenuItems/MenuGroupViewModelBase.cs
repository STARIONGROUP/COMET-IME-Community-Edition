// -------------------------------------------------------------------------------------------------
// <copyright file="MenuGroupViewModelBase.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.MenuItems
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    public abstract class MenuGroupViewModelBase<T> : ViewModelBase<T> where T : Thing
    {
        /// <summary>
        /// backing field for the <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuGroupViewModelBase{T}"/> class
        /// </summary>
        /// <param name="thing">
        /// The <see cref="thing"/> to add as the container for this menu group.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        protected MenuGroupViewModelBase(T thing, ISession session) : base(thing, session)
        {
            this.SetProperties();
        }

        /// <summary>
        /// Derives the name string based on containment
        /// </summary>
        /// <returns>The formatted name of the group.</returns>
        protected abstract string DeriveName();

        /// <summary>
        /// Gets or sets the displayed Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            private set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// The objectChange event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/> containing the <see cref="EngineeringModel"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.SetProperties();
        }

        /// <summary>
        /// Updates the properties.
        /// </summary>
        protected void SetProperties()
        {
            this.Name = this.DeriveName();
        }
    }
}