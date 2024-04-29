﻿// ------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupRowViewModelTestFixture.cs" company="Starion Group S.A.">
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
    /// Suite of tests for the <see cref="IterationSetupRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class IterationSetupRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private IterationSetup iterationSetup;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, null);

            var iterationNumber = typeof(IterationSetup).GetProperty("IterationNumber");
            iterationNumber.SetValue(this.iterationSetup, 1);
        }
    }
}
