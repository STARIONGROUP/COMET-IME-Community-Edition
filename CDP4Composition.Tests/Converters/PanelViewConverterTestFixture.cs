// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelViewConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Threading;
    using System.Windows.Controls;

    using CDP4Composition.Converters;

    using NUnit.Framework;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class PanelViewConverterTestFixture
    {
        private PanelViewConverter panelViewConverter;

        [SetUp]
        public void SetUp()
        {
            this.panelViewConverter = new PanelViewConverter();
        }

        [Test]
        public void VerifyThatTheConvertMethodReturnsTheExpectedViewWithDataContext()
        {
            var panelViewModel = new TestAssemblyPanelViewModel();

            var panelView = panelViewConverter.Convert(panelViewModel, null, null, null);

            Assert.IsInstanceOf<TestAssemblyPanel>(panelView);
            Assert.That(((TestAssemblyPanel)panelView).DataContext, Is.EqualTo(panelViewModel));
        }

        [Test]
        public void VerifyThatConvertBackIsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => this.panelViewConverter.ConvertBack(null, null, null, null));
        }
    }

    public class TestAssemblyPanel : UserControl, IPanelView
    {
        public TestAssemblyPanel(bool testParameter)
        {
        }

        public object DataContext { get; set; }
    }

    public class TestAssemblyPanelViewModel : UserControl, IPanelViewModel
    {
        public string Caption { get; } 

        public Guid Identifier { get;  }

        public string ToolTip { get; }

        public string DataSource { get;  }

        public bool IsDirty { get;  }

        public bool IsSelected { get; set; }

        public string TargetName { get; set; }

        public void Dispose()
        {
        }
    }
}

