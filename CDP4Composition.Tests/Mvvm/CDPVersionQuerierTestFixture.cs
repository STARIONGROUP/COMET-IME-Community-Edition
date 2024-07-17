// ------------------------------------------------------------------------------------------------
// <copyright file="IVersioned.cs" company="Starion Group S.A.">
//   Copyright (c) 2016 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm
{
    using CDP4Common;
    using CDP4Composition.Mvvm;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CDPVersionQuerier"/> class
    /// </summary>
    [TestFixture]
    public class CDPVersionQuerierTestFixture
    {
        [Test]
        public void VerifyThatVersionedClassReturnsExpectedVersion()
        {
            var vm = new VersionedViewModel();
            
            var version = vm.QueryCdpVersion();

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Build);
        }
        
        [Test]
        public void VerifyThatNonVersionedClassReturnsExpectedVersion()
        {
            var vm = new NonVersionedViewModel();

            var version = vm.QueryCdpVersion();

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual(0, version.Build);
        }
    }

    [CDPVersion("1.2.3")]
    internal class VersionedViewModel 
    {                
    }
    
    internal class NonVersionedViewModel
    {
    }
}
