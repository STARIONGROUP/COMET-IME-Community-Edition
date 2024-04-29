// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DockLayoutViewModelFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.ViewModels
{
    using System.Linq;
    using System.Threading;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.ViewModels;

    using DevExpress.Xpf.Docking;
    using DevExpress.Xpf.Docking.Base;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DockLayoutViewModelFixture
    {
        [Test]
        public void VerifyViewModelInitialState()
        {
            var viewModel = new DockLayoutViewModel(Mock.Of<IDialogNavigationService>());

            Assert.That(viewModel.DockPanelViewModels.Count(), Is.EqualTo(0));
        }

        [Test]
        public void VerifyViewModelIsSetAndSelectedWhenAdding()
        {
            var dialogMock = new Mock<IDialogNavigationService>();
            var viewModel = new DockLayoutViewModel(dialogMock.Object);

            var panelViewModel = Mock.Of<IPanelViewModel>();

            viewModel.AddDockPanelViewModel(panelViewModel);

            Assert.That(viewModel.DockPanelViewModels.Single(), Is.EqualTo(panelViewModel));
            Assert.IsTrue(viewModel.DockPanelViewModels.Single().IsSelected);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void VerifyCloseCommandRemovesPanelViewModel()
        {
            var viewModel  = new DockLayoutViewModel(Mock.Of<IDialogNavigationService>());

            var panelViewModelStub = Mock.Of<IPanelViewModel>();
            viewModel.AddDockPanelViewModel(panelViewModelStub);

            viewModel.DockPanelClosedCommand.Execute(new DockItemClosedEventArgs(null, new[] { new LayoutPanel() { Content = panelViewModelStub }}));
            Assert.IsEmpty(viewModel.DockPanelViewModels);
        }

        [TestCase(true, ExpectedResult = false)]
        [TestCase(false, ExpectedResult = true)]
        [Apartment(ApartmentState.STA)]
        public bool VerifyClosingCommandCancellation(bool userCancel)
        {
            var dialogStub = new Mock<IDialogNavigationService>();
            dialogStub.Setup(d => d.NavigateModal(It.IsAny<IDialogViewModel>()))
                .Returns(new BaseDialogResult(userCancel));

            var viewModel = new DockLayoutViewModel(dialogStub.Object);

            //Create panel view model stub
            var panelViewModelStub = new Mock<IPanelViewModel>();
            panelViewModelStub.SetupAllProperties();
            panelViewModelStub.SetupGet(p => p.IsDirty).Returns(true);

            //Add stub to dock view model
            viewModel.AddDockPanelViewModel(panelViewModelStub.Object);

            var args = new ItemCancelEventArgs(new LayoutPanel() 
            {
                Content = panelViewModelStub.Object 
            });

            //Act
            viewModel.DockPanelClosingCommand.Execute(args);

            return args.Cancel;
        }
    }
}
