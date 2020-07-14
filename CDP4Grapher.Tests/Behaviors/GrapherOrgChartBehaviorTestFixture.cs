// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraoherOrgChartBehaviorTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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


namespace CDP4Grapher.Tests.Behaviors
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.Types;

    using CDP4Grapher.Behaviors;
    using CDP4Grapher.Tests.Data;
    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;
    using CDP4Grapher.Views;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Layout;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class GrapherOrgChartBehaviorTestFixture : GrapherBaseTestData
    {
        private GrapherOrgChartBehavior behavior;
        private Mock<IGrapherSaveFileDialog> saveFileDialog;
        private List<GraphElementViewModel> elementViewModels;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.elementViewModels = new List<GraphElementViewModel>()
            {
                new GraphElementViewModel(this.NestedElement),
                new GraphElementViewModel(new NestedElement(Guid.NewGuid(), this.Cache, this.Uri)
                {
                    RootElement = this.ElementDefinition1,
                    ElementUsage = new OrderedItemList<ElementUsage>(this.Option) { this.ElementUsage }
                }),

                new GraphElementViewModel(new NestedElement(Guid.NewGuid(), this.Cache, this.Uri)
                {
                    ElementUsage = new OrderedItemList<ElementUsage>(this.Option) { this.ElementUsage, this.ElementUsage1 }
                }),

                new GraphElementViewModel(new NestedElement(Guid.NewGuid(), this.Cache, this.Uri)
                {
                    ElementUsage = new OrderedItemList<ElementUsage>(this.Option) { this.ElementUsage, this.ElementUsage1, this.ElementUsage2 }
                })
            };

            //var nestedElement = new NestedElementTreeGenerator().Generate(this.Option, this.Domain).Select(x => new GraphElementViewModel(x));

            this.behavior = new GrapherOrgChartBehavior();

            this.saveFileDialog = new Mock<IGrapherSaveFileDialog>();
            this.saveFileDialog.Setup(x => x.ShowDialog()).Returns(true);
            this.saveFileDialog.Setup(x => x.OpenFile()).Returns(new MemoryStream());
        }

        [TearDown]
        public void Destroy()
        {
            this.behavior.Detach();
        }

        [Test]
        public void VerifyProperties()
        {
            this.behavior.Attach(new GrapherDiagramControl() {DataContext = new Mock<IGrapherViewModel>().Object});
            Assert.IsTrue(this.behavior.CurrentLayout != default);
            this.behavior.ItemsSource = this.elementViewModels;
        }

        [Test]
        public void VerifyExport()
        {
            this.behavior.Attach(new GrapherDiagramControl());
            this.behavior.ExportGraph(this.saveFileDialog.Object);
            this.saveFileDialog.Verify(x => x.ShowDialog(), Times.Once);
            this.saveFileDialog.Verify(x => x.OpenFile(), Times.Once);
        }

        [Test]
        public void VerifyApplyLayoutWithoutAnyDirection()
        {
            this.behavior.Attach(new GrapherDiagramControl());
            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.Circular);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.Circular);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, null);
            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.OrganisationalChart);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.OrganisationalChart);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, null);

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver));
        }

        [Test]
        public void VerifyApplyLayout()
        {
            this.behavior.Attach(new GrapherDiagramControl());
            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.MindMap, OrientationKind.Vertical);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.MindMap);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, OrientationKind.Vertical);

            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver, TipOverDirection.RightToLeft);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.TipOver);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, TipOverDirection.RightToLeft);

            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Right);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.Fugiyama);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, Direction.Right);

            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.BottomToTop);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.TreeView);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, LayoutDirection.BottomToTop);

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver, Direction.Right));
        }
    }
}
