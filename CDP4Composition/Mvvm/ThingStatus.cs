// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingStatus.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System.Linq;
    using CDP4Common.CommonData;

    /// <summary>
    /// A class that gives information on the status of a <see cref="Thing"/>
    /// </summary>
    public class ThingStatus
    {
        /// <summary>
        /// Initializes a new instace of the <see cref="ThingStatus"/> class
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        public ThingStatus(Thing thing)
        {
            this.Thing = thing;
            this.HasError = thing.ValidationErrors.Any();
            this.HasRelationship = thing.HasRelationship;
        }

        /// <summary>
        /// Gets the <see cref="Thing"/>
        /// </summary>
        public Thing Thing { get; }

        /// <summary>
        /// Asserts whether the <see cref="Thing"/> has errors
        /// </summary>
        public bool HasError { get; }

        /// <summary>
        /// Gets a value indicating whether the thing has associated relationships
        /// </summary>
        public bool HasRelationship { get; }
    }
}
