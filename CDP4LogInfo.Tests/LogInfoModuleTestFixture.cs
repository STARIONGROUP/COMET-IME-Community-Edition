// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Tests
{
    using Microsoft.Practices.Prism.Regions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LogInfoModuleTestFixture
    {
        [Test]
        public void VerifyThatModuleIsSet()
        {
            var regionManager = new Mock<IRegionManager>();
            var module = new LogInfoModule(regionManager.Object);

            Assert.IsNotNull(module.RegionManager);
        }
    }
}