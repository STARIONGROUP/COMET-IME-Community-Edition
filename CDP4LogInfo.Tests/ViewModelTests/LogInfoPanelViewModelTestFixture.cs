// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoPanelViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2023 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Tests.ViewModelTests
{
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Composition.Navigation;

    using CDP4LogInfo.ViewModels;
    using CDP4LogInfo.ViewModels.Dialogs;

    using Moq;

    using NLog;
    using NLog.Config;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class LogInfoPanelViewModelTestFixture
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Mock<IDialogNavigationService> dialogNavigationService;

        [SetUp]
        public void Setup()
        {
            this.dialogNavigationService = new Mock<IDialogNavigationService>();

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            LogManager.Configuration = new LoggingConfiguration();
        }

        [Test]
        public void VerifyThatInfoLogEventAreCaught()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
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
            Assert.That(vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(vm.ToolTip, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatWarnLogEventAreCaught()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
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
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
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
        public async Task VerifyThatClearWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            Assert.IsFalse(((ICommand)vm.ClearCommand).CanExecute(null));

            var msg = "err";
            logger.Log(LogLevel.Error, msg);

            Assert.IsTrue(((ICommand)vm.ClearCommand).CanExecute(null));

            Assert.AreEqual(1, vm.LogEventInfo.Count);

            await vm.ClearCommand.Execute();
            Assert.AreEqual(0, vm.LogEventInfo.Count);
        }

        [Test]
        public void verifyThatExportCanExecute()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            Assert.IsFalse(((ICommand)vm.ExportCommand).CanExecute(null));

            var msg = "err";
            logger.Log(LogLevel.Error, msg);

            Assert.IsTrue(((ICommand)vm.ExportCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatUpdatingSelectedLogLevelWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
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
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            Assert.IsFalse(((ICommand)vm.ExportCommand).CanExecute(null));
            logger.Log(LogLevel.Error, "an error message");
            Assert.AreEqual(1, vm.LogEventInfo.Count);
            Assert.IsTrue(((ICommand)vm.ExportCommand).CanExecute(null));
        }

        [Test]
        public void TestLogFatalWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            vm.IsFatalLogelSelected = true;
            logger.Log(LogLevel.Fatal, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogDebugWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            vm.SelectedLogLevel = LogLevel.Debug;
            vm.IsDebugLogelSelected = true;
            logger.Log(LogLevel.Debug, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogErrorWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            vm.IsErrorLogelSelected = true;
            logger.Log(LogLevel.Error, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogInfoWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            vm.IsInfoLogelSelected = true;
            vm.SelectedLogLevel = LogLevel.Info;
            logger.Log(LogLevel.Info, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogTraceWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            vm.IsTraceLogelSelected = true;
            vm.SelectedLogLevel = LogLevel.Trace;
            logger.Log(LogLevel.Trace, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public void TestLogWarnWorks()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            vm.IsWarnLogelSelected = true;
            logger.Log(LogLevel.Warn, "test");
            var data = vm.Data;
            Assert.IsFalse(data.IsEmpty);
        }

        [Test]
        public async Task Verif_that_when_ShowDetailsDialogCommand_is_executed_dialognavigationservices_is_navigated()
        {
            var vm = new LogInfoPanelViewModel(this.dialogNavigationService.Object);
            vm.IsWarnLogelSelected = true;
            logger.Log(LogLevel.Warn, "test");

            vm.SelectedItem = vm.LogEventInfo.First();

            await vm.ShowDetailsDialogCommand.Execute();

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<LogItemDialogViewModel>()));
        }
    }
}
