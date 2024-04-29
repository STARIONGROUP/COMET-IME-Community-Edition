// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementSpecificationContentComparerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementsSpecificationEditor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Dal;

    using CDP4Requirements.ViewModels.RequirementsSpecificationEditor;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementSpecificationContentComparer"/>
    /// </summary>
    [TestFixture]
    public class RequirementSpecificationContentComparerTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("https://www.stariongroup.eu");
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
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.cache = this.assembler.Cache;

            this.requirementSpecificationContentComparer = new RequirementSpecificationContentComparer();

            this.requirementsSpecification = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "User Requirements Document",
                ShortName = "URD"
            };

            this.requirementsGroupA = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "A" };
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
