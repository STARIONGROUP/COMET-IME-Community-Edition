// -------------------------------------------------------------------------------------------------
// <copyright file="LogDetailsViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
{
    using System.Linq;

    using CDP4IME.ViewModels;
    using NLog;
    using NUnit.Framework;

    [TestFixture]
    public class LogDetailsViewModelTestFixture
    {
        [Test]
        public void VerifyThatPropertyAreSet()
        {
            var vm = new LogDetailsViewModel(new LogEventInfo(LogLevel.Info, "testlogger", "testMsg"));
            Assert.AreNotEqual(0, vm.DetailRows.Count);
            Assert.IsNotNullOrEmpty(vm.DetailRows.First().Content);
            Assert.IsNotNullOrEmpty(vm.DetailRows.First().Property);
            Assert.IsNotNullOrEmpty(vm.DialogTitle);
        }
    }
}