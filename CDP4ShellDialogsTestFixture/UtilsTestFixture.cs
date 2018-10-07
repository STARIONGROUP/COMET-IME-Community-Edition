// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="Utils"/> class
    /// </summary>
    [TestFixture]
    public class UtilsTestFixture
    {
        [Test]
        public void Verify_that_a_valid_IP_returns_true()
        {
            Assert.IsTrue(Utils.IsValidIp("192.168.1.100"));
        }

        [Test]
        public void Verify_that_an_invalid_IP_returns_false()
        {
            Assert.IsFalse(Utils.IsValidIp(null));

            Assert.IsFalse(Utils.IsValidIp("sometext"));
        }

        [Test]
        public void Verify_that_a_valid_hostname_returns_true()
        {
            Assert.IsTrue(Utils.IsValidHostName("cdp4.rheagroup.com"));
        }

        [Test]
        public void Verify_that_an_invalid_hostname_returns_false()
        {
            Assert.IsFalse(Utils.IsValidHostName(null));

            Assert.IsFalse(Utils.IsValidHostName("cdp4@com"));
        }

        [Test]
        public void Verify_that_a_valid_port_returns_true()
        {
            Assert.IsTrue(Utils.IsValidPort("65535"));
        }

        [Test]
        public void Verify_that_an_invalid_port_returns_false()
        {
            Assert.IsFalse(Utils.IsValidPort(null));

            Assert.IsFalse(Utils.IsValidPort("0"));

            Assert.IsFalse(Utils.IsValidPort("65536"));

            Assert.IsFalse(Utils.IsValidPort("text"));
        }
    }
}