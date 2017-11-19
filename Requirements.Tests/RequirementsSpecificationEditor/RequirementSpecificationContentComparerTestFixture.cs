// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementSpecificationContentComparerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementsSpecificationEditor
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4Requirements.ViewModels;
    using CDP4Requirements.ViewModels.RequirementsSpecificationEditor;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using CDP4Common.Operations;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementSpecificationContentComparer"/>
    /// </summary>
    [TestFixture]
    public class RequirementSpecificationContentComparerTestFixture
    {
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Assembler assembler;

        private RequirementsSpecification requirementsSpecification;

        private RequirementsGroup requirementsGroupA;

        private RequirementsGroup requirementsGroupA_A;
        private RequirementsGroup requirementsGroupA_B;
        private RequirementsGroup requirementsGroupA_C;

        private RequirementsGroup requirementsGroupA_A_A;

        private RequirementsGroup requirementsGroupB;

        private RequirementsGroup requirementsGroupB_A;
        private RequirementsGroup requirementsGroupB_B;
        private RequirementsGroup requirementsGroupB_C;
        
        private RequirementsGroup requirementsGroupB_B_B;

        private RequirementSpecificationContentComparer requirementSpecificationContentComparer;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.requirementSpecificationContentComparer = new RequirementSpecificationContentComparer();

            this.requirementsSpecification = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "User Requirements Document",
                ShortName = "URD"
            };
            
            this.requirementsGroupA = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "A"};
            this.requirementsGroupA_A = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "A_A" };
            this.requirementsGroupA_B = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "A_B" };
            this.requirementsGroupA_C = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "A_C" };
            
            this.requirementsGroupA.Group.Add(this.requirementsGroupA_B);
            this.requirementsGroupA.Group.Add(this.requirementsGroupA_C);
            this.requirementsGroupA.Group.Add(this.requirementsGroupA_A);

            this.requirementsGroupA_A_A = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "A_A_A" };
            this.requirementsGroupA_A.Group.Add(this.requirementsGroupA_A_A);
            
            this.requirementsGroupB = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "B" };
            this.requirementsGroupB_A = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "B_A" };
            this.requirementsGroupB_B = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "B_B" };
            this.requirementsGroupB_C = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "B_C" };

            this.requirementsGroupB.Group.Add(this.requirementsGroupB_A);
            this.requirementsGroupB.Group.Add(this.requirementsGroupB_C);
            this.requirementsGroupB.Group.Add(this.requirementsGroupB_B);
            
            this.requirementsGroupB_B_B = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "B_B_B" };
            this.requirementsGroupB_B.Group.Add(this.requirementsGroupB_B_B);

            this.requirementsSpecification.Group.Add(this.requirementsGroupA);
            this.requirementsSpecification.Group.Add(this.requirementsGroupB);
        }

        [Test]
        public void VerifyThatRequirementsGroupsGetSorted()
        {
            var flatListOfRequirementGroups = new List<RequirementsGroup>();

            foreach (var requirementsGroup in this.requirementsSpecification.Group)
            {
                var containedGroups = requirementsGroup.GetAllContainedGroups();

                flatListOfRequirementGroups.AddRange(containedGroups);
                flatListOfRequirementGroups.Add(requirementsGroup);                
            }

            foreach (var requirementsGroup in flatListOfRequirementGroups)
            {
                Console.WriteLine(requirementsGroup.ShortName);
            }

            //flatListOfRequirementGroups.Sort(this.requirementSpecificationContentComparer);
        }
    }
}
