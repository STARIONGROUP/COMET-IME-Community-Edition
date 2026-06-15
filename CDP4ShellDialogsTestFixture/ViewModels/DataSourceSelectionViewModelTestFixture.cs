// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogsTestFixture.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Common.DTO;
    using CDP4Common.ExceptionHandlerService;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;
    using CDP4Composition.Utilities;

    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;

    using CDP4DalCommon.Authentication;

    using CDP4ShellDialogs.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DataSourceSelectionViewModel"/>
    /// </summary>
    [TestFixture]
    public class DataSourceSelectionViewModelTestFixture
    {
        private Mock<ISession> session;

        private Credentials credentials;

        /// <summary>
        /// mocked data service
        /// </summary>
        private Mock<IDal> mockedDal;

        /// <summary>
        /// mocked metadata
        /// </summary>
        private Mock<IDalMetaData> mockedMetaData;

        private Mock<IServiceLocator> serviceLocator;
        private CancellationTokenSource tokenSource;
        private List<Thing> dalOutputs;

        private Mock<IDialogNavigationService> navService;
        private Mock<IExceptionHandlerService> exceptionHandlerService;

        private Mock<ISessionCreator> sessionCreator;

        private Mock<IMessageBoxService> messageBoxService;

        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.tokenSource = new CancellationTokenSource();
            this.mockedDal = new Mock<IDal>();
            this.navService = new Mock<IDialogNavigationService>();
            this.exceptionHandlerService = new Mock<IExceptionHandlerService>();
            this.sessionCreator = new Mock<ISessionCreator>();
            this.messageBoxService = new Mock<IMessageBoxService>();

            this.sessionCreator.Setup(x => x.CreateSession(It.IsAny<IDal>(), It.IsAny<Credentials>(), this.messageBus, It.IsAny<IExceptionHandlerService>())).Returns(this.session.Object);

            this.mockedDal.Setup(x => x.IsValidUri(It.IsAny<string>())).Returns(true);
            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<Thing>>();
            openTaskCompletionSource.SetResult(this.dalOutputs);
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            this.mockedMetaData = new Mock<IDalMetaData>();
            this.mockedMetaData.Setup(x => x.Name).Returns("MockedDal");

            var mockedMetaData2 = new Mock<IDalMetaData>();
            mockedMetaData2.Setup(x => x.Name).Returns("MockedDal2");
            mockedMetaData2.Setup(x => x.DalType).Returns(DalType.File);

            var dataAccessLayerKinds = new List<Lazy<IDal, IDalMetaData>>();

            dataAccessLayerKinds.Add(
                new Lazy<IDal, IDalMetaData>(() => this.mockedDal.Object, this.mockedMetaData.Object));

            dataAccessLayerKinds.Add(new Lazy<IDal, IDalMetaData>(() => this.mockedDal.Object, mockedMetaData2.Object));

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<AvailableDals>())
                .Returns(new AvailableDals(dataAccessLayerKinds));

            this.serviceLocator.Setup(x => x.GetInstance<ICDPMessageBus>())
                .Returns(this.messageBus);

            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>())
                .Returns(this.messageBoxService.Object);

            this.credentials = new Credentials("John", "Doe", new Uri("https://www.stariongroup.eu"));
            this.session.Setup(x => x.DataSourceUri).Returns("https://www.stariongroup.eu");
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            var authenticationSchemeResponse = new AuthenticationSchemeResponse
            {
                Schemes = new List<AuthenticationSchemeKind>
                {
                    AuthenticationSchemeKind.Basic, AuthenticationSchemeKind.ExternalJwtBearer
                },
                Authority = "http://127.0.0.1/"
            };

            this.session.Setup(x => x.QueryAvailableAuthenticationScheme()).ReturnsAsync(authenticationSchemeResponse);
        }

        [Test]
        public async Task AssertThatOkCommandCanExecuteAndASessionObjectIsSet()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            Assert.IsTrue(((ICommand)viewmodel.CancelCommand).CanExecute(null));

            Assert.That(viewmodel.ErrorMessage, Is.Null.Or.Empty);

            Assert.IsNotEmpty(viewmodel.AvailableDataSourceKinds);
            viewmodel.AvailableAuthenticationScheme = new AuthenticationSchemeResponse() { Schemes = new List<AuthenticationSchemeKind>() { AuthenticationSchemeKind.Basic } };
            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";
            viewmodel.Uri = "https://www.stariongroup.eu";
            
            Assert.IsTrue(((ICommand)viewmodel.OkCommand).CanExecute(null));
            await viewmodel.OkCommand.Execute().Catch(Observable.Return(Unit.Default));
            Assert.NotNull(viewmodel.SelectedDataSourceKind);

            Assert.False(((ICommand)viewmodel.BrowseSourceCommand).CanExecute(null));
            viewmodel.SelectedDataSourceKind = viewmodel.AvailableDataSourceKinds.Single(x => x.DalType == DalType.File);
            Assert.IsTrue(((ICommand)viewmodel.BrowseSourceCommand).CanExecute(null));
        }

        [Test]
        public async Task AssertThatUriManagerDoesNotThrow()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);
            Assert.IsTrue(((ICommand)viewmodel.OpenUriManagerCommand).CanExecute(null));
            Assert.DoesNotThrowAsync(async () => await viewmodel.OpenUriManagerCommand.Execute());
        }

        [Test]
        public async Task AssertThatProxyManagerDoesNotThrow()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);
            Assert.IsTrue(((ICommand)viewmodel.OpenProxyConfigurationCommand).CanExecute(null));
            Assert.DoesNotThrowAsync(async () => await viewmodel.OpenProxyConfigurationCommand.Execute());
        }

        [Test]
        public void AssertViewModelWorksWithMultipleUris()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            Assert.IsTrue(((ICommand)viewmodel.CancelCommand).CanExecute(null));
            Assert.That(viewmodel.ErrorMessage, Is.Null.Or.Empty);

            Assert.IsNotEmpty(viewmodel.AvailableDataSourceKinds);

            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";

            var daltype0 = viewmodel.AvailableDataSourceKinds[0];
            var daltype1 = viewmodel.AvailableDataSourceKinds[1];
            viewmodel.SelectedDataSourceKind = null;

            var cl1 = new UriConfig() { Alias = "BadAlias", Uri = "KKK", DalType = daltype0.DalType.ToString() };
            var row1 = new UriRowViewModel() { UriConfig = cl1 };

            var cl2 = new UriConfig() { Uri = "https://www.stariongroup.eu", DalType = daltype0.DalType.ToString() };
            var row2 = new UriRowViewModel() { UriConfig = cl2 };

            var cl3 = new UriConfig() { Uri = "https://www.stariongroup.eu", DalType = daltype1.DalType.ToString() };
            var row3 = new UriRowViewModel() { UriConfig = cl3 };

            viewmodel.AllDefinedUris.Clear();
            viewmodel.AllDefinedUris.Add(row1);
            viewmodel.AllDefinedUris.Add(row2);
            viewmodel.AllDefinedUris.Add(row3);

            viewmodel.SelectedDataSourceKind = daltype0;
            viewmodel.SelectedUri = viewmodel.AvailableUris.Last();
            Assert.IsFalse(((ICommand)viewmodel.OkCommand).CanExecute(null));
            Assert.IsTrue(viewmodel.AvailableUris.Count == 2);

            viewmodel.SelectedDataSourceKind = daltype1;
            Assert.IsTrue(viewmodel.AvailableUris.Count == 1);
        }

        [Test]
        public async Task VerifyThatCancelWorks()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            await viewmodel.CancelCommand.Execute();

            Assert.IsFalse(viewmodel.HasError);
        }

        [Test]
        public async Task VerifyThatIfDuplicateSessionExistsErrorExists()
        {
            var sessions = new List<ISession>();
            sessions.Add(this.session.Object);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object, sessions);

            viewmodel.Uri = "https://www.stariongroup.eu";
            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";

            await viewmodel.OkCommand.Execute();

            Assert.AreEqual("A session with the username John already exists", viewmodel.ErrorMessage);
        }

        [Test]
        public void Verify_that_when_proxy_is_enabled_proxy_address_and_port_are_set()
        {
            var vm = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);
            Assert.IsFalse(vm.IsProxyEnabled);
            Assert.AreEqual(string.Empty, vm.ProxyUri);
            Assert.AreEqual(string.Empty, vm.ProxyPort);

            vm.IsProxyEnabled = true;

            Assert.AreNotEqual(string.Empty, vm.ProxyUri);
            Assert.AreNotEqual(string.Empty, vm.ProxyPort);

            vm.IsProxyEnabled = false;

            Assert.AreEqual(string.Empty, vm.ProxyUri);
            Assert.AreEqual(string.Empty, vm.ProxyPort);
        }

        [Test]
        public void AssertThatShowPasswordButtonTextMatchesState()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            // When password is hidden button should be Show
            viewmodel.IsPasswordVisible = false;
            Assert.AreEqual(viewmodel.ShowPasswordButtonText, "Show");

            // When password is visible it should show Hide
            viewmodel.IsPasswordVisible = true;
            Assert.AreEqual(viewmodel.ShowPasswordButtonText, "Hide");
        }

        [Test]
        public void AssertThatIsFullTrustAllowedWorks()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);
            Assert.That(viewmodel.IsFullTrustAllowed, Is.False);
            Assert.That(viewmodel.IsFullTrustCheckBoxEnabled, Is.True);

            viewmodel.IsFullTrustAllowed = true;
            Assert.That(viewmodel.IsFullTrustAllowed, Is.True);

            viewmodel.SelectedDataSourceKind = viewmodel.AvailableDataSourceKinds.First(x => x.DalType == DalType.File);
            Assert.That(viewmodel.IsFullTrustCheckBoxEnabled, Is.False);
            Assert.That(viewmodel.IsFullTrustAllowed, Is.False);

            viewmodel.SelectedDataSourceKind = viewmodel.AvailableDataSourceKinds.First(x => x.DalType == DalType.Web);
            Assert.That(viewmodel.IsFullTrustCheckBoxEnabled, Is.True);
            Assert.That(viewmodel.IsFullTrustAllowed, Is.False);
        }

        [Test]
        public async Task AssertThatRequestSchemaIsNotCheckedForInvalidUriOrNonWebDataSource()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);
            viewmodel.SelectedDataSourceKind = viewmodel.AvailableDataSourceKinds.Single(x => x.DalType == DalType.File);

            Assert.That(viewmodel.Uri, Is.Empty);
            Assert.That(viewmodel.SelectedDataSourceKind.DalType, Is.EqualTo(DalType.File));
            Assert.That(viewmodel.IsFullTrustAllowed, Is.False);
            Assert.That(viewmodel.IsProxyEnabled, Is.False);

            viewmodel.IsProxyEnabled = true;
            viewmodel.Uri = "Not a valid URI";

            // an invalid uri must not schedule any authentication scheme resolution
            await Task.Delay(1500);

            this.session.Verify(x => x.QueryAvailableAuthenticationScheme(), Times.Never);
            Assert.That(viewmodel.AvailableAuthenticationScheme, Is.Null);
            Assert.That(viewmodel.IsResolvingBusy, Is.False);
            Assert.That(viewmodel.ErrorMessage, Is.Null.Or.Empty);
        }

        [Test]
        public async Task AssertThatRequestSchemaIsResolvedAfterDebounceForValidUri()
        {
            this.SetupAuthenticationSchemes(AuthenticationSchemeKind.Basic);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            viewmodel.SelectedUri = new UriRowViewModel { Uri = "https://www.stariongroup.eu" };

            // the resolution is debounced and must not happen synchronously
            this.session.Verify(x => x.QueryAvailableAuthenticationScheme(), Times.Never);

            await WaitUntil(() => viewmodel.AvailableAuthenticationScheme != null);

            this.session.Verify(x => x.QueryAvailableAuthenticationScheme(), Times.AtLeastOnce);
            Assert.That(viewmodel.AvailableAuthenticationScheme.Schemes, Does.Contain(AuthenticationSchemeKind.Basic));

            await WaitUntil(() => !viewmodel.IsResolvingBusy);
            Assert.That(viewmodel.IsResolvingBusy, Is.False);
        }

        [Test]
        public async Task AssertThatRapidChangesAreDebouncedToASingleResolution()
        {
            this.SetupAuthenticationSchemes(AuthenticationSchemeKind.Basic);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            for (var i = 0; i < 5; i++)
            {
                viewmodel.Uri = $"https://www.stariongroup.eu/{i}";
            }

            await WaitUntil(() => viewmodel.AvailableAuthenticationScheme != null);

            this.session.Verify(x => x.QueryAvailableAuthenticationScheme(), Times.Once);
        }

        [Test]
        public async Task AssertThatResolutionErrorIsReportedAndBusyIsReset()
        {
            this.session.Setup(x => x.QueryAvailableAuthenticationScheme()).ThrowsAsync(new Exception("resolve failure"));

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            viewmodel.Uri = "https://www.stariongroup.eu";

            await WaitUntil(() => viewmodel.ErrorMessage == "resolve failure");

            Assert.That(viewmodel.ErrorMessage, Is.EqualTo("resolve failure"));
            Assert.That(viewmodel.AvailableAuthenticationScheme, Is.Null);

            await WaitUntil(() => !viewmodel.IsResolvingBusy);
            Assert.That(viewmodel.IsResolvingBusy, Is.False);
        }

        [Test]
        public async Task AssertThatEmptySchemesReportNotAComet()
        {
            this.session.Setup(x => x.QueryAvailableAuthenticationScheme())
                .ReturnsAsync(new AuthenticationSchemeResponse { Schemes = new List<AuthenticationSchemeKind>() });

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            viewmodel.Uri = "https://www.stariongroup.eu";

            await WaitUntil(() => viewmodel.ErrorMessage == "Not a COMET-CDP4 server");

            Assert.That(viewmodel.ErrorMessage, Is.EqualTo("Not a COMET-CDP4 server"));

            await WaitUntil(() => !viewmodel.IsResolvingBusy);
            Assert.That(viewmodel.IsResolvingBusy, Is.False);
        }

        [Test]
        public async Task AssertThatExternalProviderSchemeNavigatesToExternalAuthenticationDialog()
        {
            this.session.Setup(x => x.QueryAvailableAuthenticationScheme())
                .ReturnsAsync(new AuthenticationSchemeResponse
                {
                    Schemes = new List<AuthenticationSchemeKind> { AuthenticationSchemeKind.ExternalJwtBearer },
                    Authority = "http://127.0.0.1/"
                });

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            viewmodel.Uri = "https://www.stariongroup.eu";

            await WaitUntil(() => viewmodel.AvailableAuthenticationScheme != null);

            this.navService.Verify(x => x.NavigateModal(It.IsAny<ExternalAuthenticationDialogViewModel>()), Times.AtLeastOnce);

            await WaitUntil(() => !viewmodel.IsResolvingBusy);
            Assert.That(viewmodel.IsAuthenticatedViaExternalProvider, Is.False);
            Assert.That(viewmodel.IsResolvingBusy, Is.False);
        }

        [Test]
        public async Task AssertThatFailingUserNameQueryPromptsForLogoutAndReportsError()
        {
            this.SetupExternalAuthenticationWithFailingUserNameQuery("username failure");

            this.messageBoxService
                .Setup(x => x.ShowAlwaysOnTop(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<MessageBoxResult>()))
                .Returns(MessageBoxResult.No);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            viewmodel.Uri = "https://www.stariongroup.eu";

            await WaitUntil(() => viewmodel.ErrorMessage == "username failure");

            // the user is prompted with the logout question
            this.messageBoxService.Verify(
                x => x.ShowAlwaysOnTop(It.IsRegex("Do you want to logout"), "Logout?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes),
                Times.Once);

            // when the prompt is answered with No, no logout dialog must be navigated to
            this.navService.Verify(x => x.NavigateModal(It.IsAny<ExternalAuthenticationLogoutDialogViewModel>()), Times.Never);

            Assert.That(viewmodel.ErrorMessage, Is.EqualTo("username failure"));
            Assert.That(viewmodel.IsAuthenticatedViaExternalProvider, Is.False);

            await WaitUntil(() => !viewmodel.IsResolvingBusy);
            Assert.That(viewmodel.IsResolvingBusy, Is.False);
        }

        [Test]
        public async Task AssertThatAcceptingLogoutPromptNavigatesToLogoutDialog()
        {
            this.SetupExternalAuthenticationWithFailingUserNameQuery("username failure");

            this.messageBoxService
                .Setup(x => x.ShowAlwaysOnTop(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<MessageBoxResult>()))
                .Returns(MessageBoxResult.Yes);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            viewmodel.Uri = "https://www.stariongroup.eu";

            await WaitUntil(() => viewmodel.ErrorMessage == "username failure");

            // when the prompt is accepted, the logout dialog is navigated to
            this.navService.Verify(x => x.NavigateModal(It.IsAny<ExternalAuthenticationLogoutDialogViewModel>()), Times.Once);

            Assert.That(viewmodel.ErrorMessage, Is.EqualTo("username failure"));

            await WaitUntil(() => !viewmodel.IsResolvingBusy);
            Assert.That(viewmodel.IsResolvingBusy, Is.False);
        }

        [Test]
        public async Task AssertThatExecuteOkForFileDataSourceOpensSession()
        {
            this.session.Setup(x => x.Open(It.IsAny<bool>())).Returns(Task.CompletedTask);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);
            viewmodel.SelectedDataSourceKind = viewmodel.AvailableDataSourceKinds.Single(x => x.DalType == DalType.File);

            viewmodel.ShouldProvideCredentialsInformation = true;
            viewmodel.Uri = "https://www.stariongroup.eu";
            viewmodel.UserName = "John";
            viewmodel.Password = "Doe";

            await viewmodel.OkCommand.Execute().Catch(Observable.Return(Unit.Default));

            this.session.Verify(x => x.Open(It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public async Task AssertThatExecuteOkForWebDataSourceWithCredentialsAuthenticatesAndOpens()
        {
            this.SetupAuthenticationSchemes(AuthenticationSchemeKind.Basic);

            this.session.Setup(x => x.AuthenticateAndOpen(It.IsAny<AuthenticationSchemeKind>(), It.IsAny<AuthenticationInformation>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            // let the resolution run so that the internal session gets created and the scheme is resolved
            viewmodel.Uri = "https://www.stariongroup.eu";
            await WaitUntil(() => viewmodel.AvailableAuthenticationScheme != null && !viewmodel.IsResolvingBusy);

            Assert.That(viewmodel.ShouldProvideCredentialsInformation, Is.True);

            viewmodel.UserName = "John";
            viewmodel.Password = "Doe";

            await viewmodel.OkCommand.Execute().Catch(Observable.Return(Unit.Default));

            this.session.Verify(x => x.AuthenticateAndOpen(AuthenticationSchemeKind.Basic, It.IsAny<AuthenticationInformation>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public async Task AssertThatExecuteOkForExternalAuthenticationOpensSession()
        {
            this.SetupAuthenticationSchemes(AuthenticationSchemeKind.ExternalJwtBearer);

            this.session.Setup(x => x.Open(It.IsAny<bool>())).Returns(Task.CompletedTask);

            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            // let the resolution run so that the internal session gets created
            viewmodel.Uri = "https://www.stariongroup.eu";
            await WaitUntil(() => viewmodel.AvailableAuthenticationScheme != null && !viewmodel.IsResolvingBusy);

            // an external-only scheme does not require credentials to be entered
            Assert.That(viewmodel.ShouldProvideCredentialsInformation, Is.False);

            viewmodel.UserName = "John";
            viewmodel.Password = "Doe";

            await viewmodel.OkCommand.Execute().Catch(Observable.Return(Unit.Default));

            this.session.Verify(x => x.Open(It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public void AssertThatIsResolvingBusyCanBeSet()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, this.messageBus, this.exceptionHandlerService.Object, this.sessionCreator.Object);

            Assert.That(viewmodel.IsResolvingBusy, Is.False);

            viewmodel.IsResolvingBusy = true;

            Assert.That(viewmodel.IsResolvingBusy, Is.True);
        }

        /// <summary>
        /// Sets up the mocked session to return the provided <see cref="AuthenticationSchemeKind"/> values
        /// </summary>
        /// <param name="schemes">The supported <see cref="AuthenticationSchemeKind"/> values</param>
        private void SetupAuthenticationSchemes(params AuthenticationSchemeKind[] schemes)
        {
            this.session.Setup(x => x.QueryAvailableAuthenticationScheme())
                .ReturnsAsync(new AuthenticationSchemeResponse
                {
                    Schemes = schemes.ToList(),
                    Authority = "http://127.0.0.1/"
                });
        }

        /// <summary>
        /// Sets up an external-authentication resolution that successfully obtains a token but fails when querying the
        /// authenticated user name, which triggers the logout prompt flow.
        /// </summary>
        /// <param name="userNameQueryError">The error message thrown by <see cref="ISession.QueryAuthenticatedUserName"/></param>
        private void SetupExternalAuthenticationWithFailingUserNameQuery(string userNameQueryError)
        {
            this.SetupAuthenticationSchemes(AuthenticationSchemeKind.ExternalJwtBearer);

            this.navService
                .Setup(x => x.NavigateModal(It.IsAny<ExternalAuthenticationDialogViewModel>()))
                .Returns(new ExternalAuthenticationResult(true, new AuthenticationToken("access-token", "refresh-token")));

            this.session.Setup(x => x.QueryAuthenticatedUserName()).ThrowsAsync(new Exception(userNameQueryError));
        }

        /// <summary>
        /// Polls a <paramref name="condition"/> until it is satisfied or the timeout elapses.
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeoutMilliseconds">The maximum time to wait</param>
        private static async Task WaitUntil(Func<bool> condition, int timeoutMilliseconds = 8000)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!condition() && stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
            {
                await Task.Delay(25);
            }
        }
    }
}
