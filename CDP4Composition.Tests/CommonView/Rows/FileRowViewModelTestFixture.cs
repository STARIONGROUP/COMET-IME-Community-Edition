﻿// ------------------------------------------------------------------------------------------------
// <copyright file="FileRowViewModelTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="FileRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class FileRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Person person;
        private Participant participant;
        private Folder folder;
        private File file;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();

            this.person = new Person(Guid.NewGuid(), null, null) { Surname = "Doe", GivenName = "John" };

            this.participant = new Participant(Guid.NewGuid(), null, null);
            this.participant.Person = this.person;

            this.folder = new Folder(Guid.NewGuid(), null, null) { Name = "test" };
            this.file = new File(Guid.NewGuid(), null, null);
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
