// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoPanelViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Tests.ViewModelTests
{
    using System.Reactive.Concurrency;

    using CDP4LogInfo.ViewModels;

    using NLog;
    using NLog.Config;
    using NUnit.Framework;
    using System.Linq;

    using ReactiveUI;

    [TestFixture]
    public class LogInfoPanelViewModelTestFixture
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            LogManager.Configuration = new LoggingConfiguration();
        }

        [Test]
        public void VerifyThatInfoLogEventAreCaught()
        {
            var vm = new LogInfoPanelViewModel();
            var msg = "info";

            logger.Log(LogLevel.Info, msg);
            Assert.AreEqual(0, vm.LogEventInfo.Count);

            logger.Log(LogLevel.Warn, msg);
            Assert.AreEqual(1, vm.LogEventInfo.Count);

            var row = vm.LogEventInfo.Single();
            Assert.AreEqual(msg, row.Message);
            Assert.AreEqual(LogLevel.Warn, row.LogLevel);
            Assert.IsNotNull(row.TimeStamp);
            Assert.IsNotNull(row.Logger);

            Assert.IsNotNullOrEmpty(vm.Caption);
            Assert.IsNotNullOrEmpty(vm.ToolTip);
        }

        [Test]
        public void VerifyThatWarnLogEventAreCaught()
        {
            var vm = new LogInfoPanelViewModel();
            var msg = "warn";

            logger.Log(LogLevel.Warn, msg);
            Assert.AreEqual(1, vm.LogEventInfo.Count);

            var row = vm.LogEventInfo.Single();
            Assert.AreEqual(msg, row.Message);
            Assert.AreEqual(LogLevel.Warn, row.LogLevel);
            Assert.IsNotNull(row.TimeStamp);
            Assert.IsNotNull(row.Logger);
        }

        [Test]
        public void VerifyThatErrLogEventAreCaught()
        {
            var vm = new LogInfoPanelViewModel();
            var msg = "err";

            logger.Log(LogLevel.Error, msg);
            Assert.AreEqual(1, vm.LogEventInfo.Count);

            var row = vm.LogEventInfo.Single();
            Assert.AreEqual(msg, row.Message);
            Assert.AreEqual(LogLevel.Error, row.LogLevel);
            Assert.IsNotNull(row.TimeStamp);
            Assert.IsNotNull(row.Logger);
        }

        [Test]
        public void VerifyThatClearWorks()
        {
            var vm = new LogInfoPanelViewModel();
            Assert.IsFalse(vm.ClearCommand.CanExecute(null));

            var msg = "err";
            logger.Log(LogLevel.Error, msg);

            Assert.IsTrue(vm.ClearCommand.CanExecute(null));

            vm.ClearCommand.Execute(null);
            Assert.AreEqual(0, vm.LogEventInfo.Count);
        }

        [Test]
        public void verifyThatExportCanExecute()
        {
            var vm = new LogInfoPanelViewModel();
            Assert.IsFalse(vm.ExportCommand.CanExecute(null));

            var msg = "err";
            logger.Log(LogLevel.Error, msg);

            Assert.IsTrue(vm.ExportCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatUpdatingSelectedLogLevelWorks()
        {
            var vm = new LogInfoPanelViewModel();
            var msg = "trace";

            vm.SelectedLogLevel = LogLevel.Fatal;
            logger.Log(LogLevel.Trace, msg);

            Assert.AreEqual(0, vm.LogEventInfo.Count);

            vm.SelectedLogLevel = LogLevel.Trace;
            logger.Log(LogLevel.Trace, msg);
            Assert.AreEqual(1, vm.LogEventInfo.Count);
        }

        [Test]
        public void VerifyThatUpdatingExportIsOnlyEnabledWhenMesagesArePresent()
        {
            var vm = new LogInfoPanelViewModel();
            Assert.IsFalse(vm.ExportCommand.CanExecute(null));
            logger.Log(LogLevel.Error, "an error message");
            Assert.AreEqual(1, vm.LogEventInfo.Count);
            Assert.IsTrue(vm.ExportCommand.CanExecute(null));
        }

        [Test]
        public void TestLogFatalWorks()
        {
            var vm = new LogInfoPanelViewModel();
            vm.IsFatalLogelSelected = true;
            logger.Log(LogLevel.Fatal, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogDebugWorks()
        {
            var vm = new LogInfoPanelViewModel();
            vm.SelectedLogLevel = LogLevel.Debug;
            vm.IsDebugLogelSelected = true;
            logger.Log(LogLevel.Debug, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogErrorWorks()
        {
            var vm = new LogInfoPanelViewModel();
            vm.IsErrorLogelSelected = true;
            logger.Log(LogLevel.Error, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogInfoWorks()
        {
            var vm = new LogInfoPanelViewModel();
            vm.IsInfoLogelSelected = true;
            vm.SelectedLogLevel = LogLevel.Info;
            logger.Log(LogLevel.Info, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogTraceWorks()
        {
            var vm = new LogInfoPanelViewModel();
            vm.IsTraceLogelSelected = true;
            vm.SelectedLogLevel = LogLevel.Trace;
            logger.Log(LogLevel.Trace, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogWarnWorks()
        {
            var vm = new LogInfoPanelViewModel();
            vm.IsWarnLogelSelected = true;
            logger.Log(LogLevel.Warn, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }
    }
}