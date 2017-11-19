// -------------------------------------------------------------------------------------------------
// <copyright file="MappingRowViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// Base class for the mapping rows
    /// </summary>
    public abstract class MappingRowViewModelBase<T> : ReactiveObject, IMappingRowViewModelBase<T> where T : Identifiable
    {
        /// <summary>
        /// Back value for <see cref="IsMapped"/>
        /// </summary>
        private bool isMapped;

        /// <summary>
        /// Backing field for <see cref="Identifiable"/>
        /// </summary>
        private T identifiable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingRowViewModelBase{T}"/> class
        /// </summary>
        /// <param name="identifiable">The <see cref="Identifiable"/> associated to this row</param>
        protected MappingRowViewModelBase(T identifiable)
        {
            this.Identifiable = identifiable;
        }

        /// <summary>
        /// Gets a value indicating if the current <see cref="DatatypeDefinition"/> is mapped
        /// </summary>
        public bool IsMapped
        {
            get { return this.isMapped; }
            set { this.RaiseAndSetIfChanged(ref this.isMapped, value); }
        }

        /// <summary>
        /// Gets the <see cref="Identifiable"/>
        /// </summary>
        public T Identifiable
        {
            get { return this.identifiable; }
            protected set { this.RaiseAndSetIfChanged(ref this.identifiable, value); }
        }


        /// <summary>
        /// Update <see cref="IsMapped"/>
        /// </summary>
        protected abstract void UpdateIsMapped();
    }
}