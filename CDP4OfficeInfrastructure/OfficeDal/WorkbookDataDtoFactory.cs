// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookDataDtoFactory.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;    
    using System.Reflection;
    
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of the <see cref="WorkbookDataDtoFactory"/> is to process an <see cref=".Iteration"/> and collect all
    /// the <see cref="CDP4Common.DTO.Thing"/>s that are required for that <see cref="Iteration"/>.
    /// </summary>
    /// <remarks>
    /// The data that is included is the following:
    /// 1. Only reference data in the chain of RDLs of the <see cref="EngineeringModel"/> that contains the <see cref="Iteration"/>
    /// 2. Only the <see cref="Person"/> that are participants in the <see cref="EngineeringModel"/>
    /// </remarks>
    public class WorkbookDataDtoFactory
    {
        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.Iteration"/> that is being processed.
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// A cache that contains the composite properties of a Type
        /// </summary>
        private readonly Dictionary<Type, IEnumerable<PropertyInfo>> typeCompositePropertyInfoCache = new Dictionary<Type, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// Backing field for the <see cref="SiteDirectoryThings"/> property
        /// </summary>
        private readonly List<CDP4Common.DTO.Thing> siteDirectoryThings = new List<CDP4Common.DTO.Thing>();

        /// <summary>
        /// Backing field for the <see cref="IterationThings"/> property
        /// </summary>
        private readonly List<CDP4Common.DTO.Thing> iterationThings = new List<CDP4Common.DTO.Thing>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookDataDtoFactory"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="CDP4Common.EngineeringModelData.Iteration"/> that is to be processed by the <see cref="WorkbookDataDtoFactory"/>
        /// </param>
        public WorkbookDataDtoFactory(Iteration iteration)
        {
            this.iteration = iteration;
        }

        /// <summary>
        /// Processes the <see cref="CDP4Common.EngineeringModelData.Iteration"/> and compiles all the objects
        /// that are required by an Iteration.
        /// </summary>
        /// <remarks>
        /// The result is stored in the <see cref="SiteDirectoryThings"/> property.
        /// </remarks>
        public void Process()
        {
            var engineeringModel = (EngineeringModel)this.iteration.Container;
            var engineeringModelSetup = engineeringModel.EngineeringModelSetup;
            var siteDirectory = (SiteDirectory)engineeringModelSetup.Container;
            this.Process(siteDirectory, engineeringModelSetup);

            // make a clone and remove all but specified iteration, then process
            var clonedEngineeringModelSetup = engineeringModelSetup.Clone(false);
            clonedEngineeringModelSetup.IterationSetup.RemoveAll(x => x.IterationIid != this.iteration.Iid);
            this.ProcessCompositeProperties(clonedEngineeringModelSetup, this.siteDirectoryThings);

            this.iterationThings.Add(engineeringModel.ToDto());
            this.iterationThings.Add(this.iteration.ToDto());
            this.ProcessCompositeProperties(this.iteration, this.iterationThings);
        }

        /// <summary>
        /// Process the <see cref="CDP4Common.SiteDirectoryData.SiteDirectory"/> and its contents
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> to process
        /// </param>
        /// <param name="engineeringModelSetup">
        /// The <see cref="EngineeringModelSetup"/> that references a required <see cref="CDP4Common.SiteDirectoryData.ModelReferenceDataLibrary"/>
        /// </param>
        private void Process(SiteDirectory siteDirectory, EngineeringModelSetup engineeringModelSetup)
        {
            //// the SiteDirectory is not processed using the ProcessCompositeProperties method
            //// the person objects need to be filtered as well as the SiteReferenceDataLibrary
            this.siteDirectoryThings.Add(siteDirectory.ToDto());

            this.ProcessThings(siteDirectory.Domain, this.siteDirectoryThings);
            this.ProcessThings(siteDirectory.DomainGroup, this.siteDirectoryThings);
            this.ProcessThings(siteDirectory.NaturalLanguage, this.siteDirectoryThings);
            this.ProcessThings(siteDirectory.Organization, this.siteDirectoryThings);
            this.ProcessThings(siteDirectory.ParticipantRole, this.siteDirectoryThings);
            this.ProcessThings(siteDirectory.PersonRole, this.siteDirectoryThings);

            // only inlcude the RDLs in the chaing of rdls starting at the model-RDL
            var referenceDataLibraries = new List<ReferenceDataLibrary>();
            var modelReferenceDataLibrary = engineeringModelSetup.RequiredRdl.First();
            referenceDataLibraries.Add(modelReferenceDataLibrary);
            var siteReferenceDataLibrary = modelReferenceDataLibrary.GetRequiredRdls();
            referenceDataLibraries.AddRange(siteReferenceDataLibrary);
            this.ProcessThings(referenceDataLibraries, this.siteDirectoryThings);

            // only include the persons that are participants in the engineering model
            var persons = engineeringModelSetup.Participant.Select(x => x.Person);
            this.ProcessThings(persons, this.siteDirectoryThings);
        }

        /// <summary>
        /// Process the list of siteDirectoryThings
        /// </summary>
        /// <param name="things">
        /// the list of siteDirectoryThings to be processed
        /// </param>
        /// <param name="dtoContainer">
        /// The list of DTO's that will contained the processed POCO things
        /// </param>
        private void ProcessThings(IEnumerable<Thing> things, List<CDP4Common.DTO.Thing> dtoContainer)
        {
            foreach (var thing in things)
            {
                dtoContainer.Add(thing.ToDto());
                this.ProcessCompositeProperties(thing, dtoContainer);
            }
        }

        /// <summary>
        /// Process the composite properties of the provided <see cref="CDP4Common.CommonData.Thing"/>.
        /// </summary>
        /// <param name="thing">
        /// The <see cref="CDP4Common.CommonData.Thing"/> of which the composite properties need to be processed
        /// </param>
        /// <param name="dtoContainer">
        /// The list of DTO's that will contained the processed POCO things
        /// </param>
        /// <remarks>
        /// The instances of the composite properties are converted to a DTO and added to the <see cref="SiteDirectoryThings"/> property.
        /// </remarks>
        private void ProcessCompositeProperties(Thing thing, List<CDP4Common.DTO.Thing> dtoContainer)
        {
            var containerPropertiesInfo = this.GetPropertyInfos(thing);

            foreach (var containerPropertyInfo in containerPropertiesInfo)
            {
                var containerProperty = containerPropertyInfo.GetValue(thing) as IEnumerable;
                if (containerProperty != null)
                {
                    foreach (var contained in containerProperty)
                    {
                        var containedThing = contained as Thing;
                        if (containedThing != null)
                        {
                            var dto = containedThing.ToDto();
                            dtoContainer.Add(dto);

                            this.ProcessCompositeProperties(containedThing, dtoContainer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the <see cref="PropertyInfo"/> of a <see cref="CDP4Common.CommonData.Thing"/> that are of kind <see cref="AggregationKind.Composite"/>
        /// </summary>
        /// <param name="thing">
        /// The instance of <see cref="CDP4Common.CommonData.Thing"/> that is being queried
        /// </param>
        /// <returns>
        /// an instance <see cref="IEnumerable{PropertyInfo}"/> 
        /// </returns>
        private IEnumerable<PropertyInfo> GetPropertyInfos(CDP4Common.CommonData.Thing thing)
        {
            IEnumerable<PropertyInfo> containerPropertiesInfo;
            var exists = this.typeCompositePropertyInfoCache.TryGetValue(thing.GetType(), out containerPropertiesInfo);
            if (!exists)
            {
                containerPropertiesInfo = thing.GetType().GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(UmlInformationAttribute))
                   && ((UmlInformationAttribute)Attribute.GetCustomAttribute(p, typeof(UmlInformationAttribute))).Aggregation == AggregationKind.Composite);

                this.typeCompositePropertyInfoCache.Add(thing.GetType(), containerPropertiesInfo);
            }

            return containerPropertiesInfo;
        }

        /// <summary>
        /// Gets the processed <see cref="Thing"/>s that are contained by the <see cref="SiteDirectory"/>
        /// </summary>
        /// <remarks>
        /// This excludes <see cref="Thing"/>s contained by an <see cref="Iteration"/>
        /// </remarks>
        public IEnumerable<CDP4Common.DTO.Thing> SiteDirectoryThings 
        {
            get
            {
                return this.siteDirectoryThings;
            }
        }

        /// <summary>
        /// Gets the processed <see cref="Thing"/>s that are contained by the <see cref="Iteration"/>
        /// </summary>
        /// <remarks>
        /// This excludes <see cref="Thing"/>s contained by an <see cref="SiteDirectory"/>
        /// </remarks>
        public IEnumerable<CDP4Common.DTO.Thing> IterationThings
        {
            get
            {
                return this.iterationThings;
            }
        }
    }
}
