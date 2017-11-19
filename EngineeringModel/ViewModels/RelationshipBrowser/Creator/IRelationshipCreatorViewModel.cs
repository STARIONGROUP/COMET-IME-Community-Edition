// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRelationshipCreatorViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;

    /// <summary>
    /// The interface for the view-model used to set the properties of a <see cref="Relationship"/>
    /// </summary>
    public interface IRelationshipCreatorViewModel : IDisposable
    {
        /// <summary>
        /// Gets the type of the <see cref="Relationship"/> to create
        /// </summary>
        string CreatorKind { get; }

        /// <summary>
        /// Gets the possible <see cref="Category"/>
        /// </summary>
        ReactiveList<Category> PossibleCategories { get; }

        /// <summary>
        /// Gets the <see cref="Category"/> applied to the <see cref="Relationship"/> to create
        /// </summary>
        List<Category> AppliedCategories { get; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Relationship"/> may be created
        /// </summary>
        bool CanCreate { get; }

        /// <summary>
        /// Re-Initializes the view-model
        /// </summary>
        void ReInitializeControl();

        /// <summary>
        /// Creates a <see cref="Relationship"/> 
        /// </summary>
        /// <returns>A new instance of <see cref="Relationship"/></returns>
        Relationship CreateRelationshipObject();
    }
}