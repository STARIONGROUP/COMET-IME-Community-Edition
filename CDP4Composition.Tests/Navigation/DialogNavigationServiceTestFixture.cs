// -------------------------------------------------------------------------------------------------
// <copyright file="DialogNavigationServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Composition;
    using Moq;
    using NUnit.Framework;
    using ViewModels;
    using Views;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class DialogNavigationServiceTestFixture
    {
        private IDialogView dialogView;
        private IDialogView floatingView;
        private IDialogViewModel dialogViewModel;
        private Mock<INameMetaData> nameMetaData;
        private Mock<INameMetaData> nameViewModelMetaData;
        private Mock<ISession> session;

        private List<Lazy<IDialogView, INameMetaData>> viewList;
        private List<Lazy<IDialogViewModel, INameMetaData>> viewModelList;

        private DialogNavigationService navigationService;

        [SetUp]
        public void Setup()
        {
            this.dialogView = new TestDialog();
            this.floatingView = new TestFloatingDialog();
            this.dialogViewModel = new TestDialogViewModel();
            this.nameMetaData = new Mock<INameMetaData>();
            this.nameViewModelMetaData = new Mock<INameMetaData>();
            this.nameViewModelMetaData.Setup(x => x.Name).Returns("view-model");
            this.session = new Mock<ISession>();

            this.viewList = new List<Lazy<IDialogView, INameMetaData>>();
            this.viewList.Add(new Lazy<IDialogView, INameMetaData>(() => this.dialogView, this.nameMetaData.Object));
            this.viewList.Add(new Lazy<IDialogView, INameMetaData>(() => this.floatingView, this.nameMetaData.Object));

            this.viewModelList = new List<Lazy<IDialogViewModel, INameMetaData>>();
            this.viewModelList.Add(new Lazy<IDialogViewModel, INameMetaData>(() => this.dialogViewModel, this.nameViewModelMetaData.Object));
        }

        [Test]
        public void VerifyThatTheServiceGetsPopulated()
        {
            this.navigationService = new DialogNavigationService(this.viewList, this.viewModelList);
            Assert.AreEqual(2, this.navigationService.DialogViewKinds.Count);
            Assert.AreEqual(1, this.navigationService.DialogViewModelKinds.Count);
        }

        [Test]
        public void VerifyThatNavigateFloatingWorks()
        {
            var navigation = new DialogNavigationService(this.viewList, this.viewModelList);

            Assert.DoesNotThrow(
                () =>
                {
                    navigation.NavigateFloating(new TestFloatingDialogViewModel(new Person(), this.session.Object));
                    var view = (Window)navigation.FloatingThingDialog.Single().Value;
                    view.Close();
                });
        }

        [Test]
        public void VerifyThatThrowsWhenViewCantBeFound()
        {
            var navigation = new DialogNavigationService(this.viewList, this.viewModelList);
            Assert.Throws<ArgumentOutOfRangeException>(() => navigation.NavigateFloating(new NotfoundDialogViewModel(new Person(), this.session.Object)));
        }
    }
}

namespace CDP4Composition.Tests.Views
{
    using System.Windows;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    public class TestDialog : Window, IDialogView
    {
        public TestDialog() { }
        public TestDialog(bool value) { }

        public object DataContext { get; set; }
        public bool? ShowDialog()
        {
            return true;
        }
    }

    public class TestFloatingDialog : Window, IDialogView
    {
        public TestFloatingDialog()
        {
        }

        public TestFloatingDialog(IDialogNavigationService navigation)
        {
        }

        public object DataContext { get; set; }
        public bool? ShowDialog()
        {
            throw new System.NotImplementedException();
        }

        public void Show()
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace CDP4Composition.Tests.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using DevExpress.Mvvm;

    public class TestDialogViewModel : DialogViewModelBase
    {
        public TestDialogViewModel()
        {
            this.UICommands = new List<UICommand>();
            this.DialogTitle = "test";
            this.DialogResult = new BaseDialogResult(true);
        }

        public IEnumerable<UICommand> UICommands { get; private set; }

        public string DialogTitle { get; private set; }
    }

    public class TestFloatingDialogViewModel : FloatingDialogViewModelBase<Person>
    {
        public TestFloatingDialogViewModel(Person person, ISession session) : base(person, session)
        {
        }
    }

    public class NotfoundDialogViewModel : FloatingDialogViewModelBase<Person>
    {
        public NotfoundDialogViewModel(Person person, ISession session) : base(person, session)
        {
        }
    }
}