// ------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of test for the hand-coded <see cref="BinaryRelationshipRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class BinaryRelationshipRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private BinaryRelationship binaryRelationship;
        private BinaryRelationshipRowViewModel viewmodel;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();            
            this.binaryRelationship = new BinaryRelationship(Guid.NewGuid(), null, null);
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
