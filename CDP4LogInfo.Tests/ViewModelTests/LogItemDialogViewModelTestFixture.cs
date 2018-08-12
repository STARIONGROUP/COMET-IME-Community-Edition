// -------------------------------------------------------------------------------------------------
// <copyright file="LogLevelImageConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Tests.ViewModelTests
{
    using System;
    using CDP4LogInfo.ViewModels.Dialogs;
    using Moq;
    using NLog;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="LogItemDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class LogItemDialogViewModelTestFixture
    {
        private LogEventInfo logEventInfo;

        [SetUp]
        public void SetUp()
        {
            this.logEventInfo = new LogEventInfo()
            {
                Level = LogLevel.Fatal,
                Message = "log message",
                TimeStamp = DateTime.Now,
                LoggerName = "logger-name"
            };
        }
        
        [Test]
        public void Verify_that_dialog_viewmodel_is_constructed()
        {
            var vm = new LogItemDialogViewModel(this.logEventInfo);
            
            Assert.AreEqual("log message", vm.Message);
            Assert.AreEqual(LogLevel.Fatal, vm.LogLevel);
            Assert.AreEqual("logger-name", vm.Logger);

        }
    }
}