// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TermRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.Tests.ViewModels.Rows
{
    using System;
    using System.Collections.Concurrent;
    using System.Windows;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.Types;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class TermRowViewModelTestFixture
    {
        private Mock<IDragInfo> draginfo;
        private Mock<IDropInfo> dropinfo;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private SiteDirectory sitedir;
        private SiteReferenceDataLibrary srdl1;
        private SiteReferenceDataLibrary srdl2;
        private Glossary glossary1;
        private Glossary glossary2;
        private Term term1;
        private Term term2;
        private readonly Uri uri = new Uri("http://test.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
        
        [SetUp]
        public void Setup()
        {
            this.draginfo = new Mock<IDragInfo>();
            this.dropinfo = new Mock<IDropInfo>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl1 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.srdl2 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) {RequiredRdl = this.srdl1};
            this.glossary1 = new Glossary(Guid.NewGuid(), this.cache, this.uri);
            this.glossary2 = new Glossary(Guid.NewGuid(), this.cache, this.uri);
            this.term1 = new Term(Guid.NewGuid(), this.cache, this.uri);
            this.term2 = new Term(Guid.NewGuid(), this.cache, this.uri);

            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl1);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl2);

            this.srdl1.Glossary.Add(this.glossary1);
            this.srdl2.Glossary.Add(this.glossary2);

            this.glossary1.Term.Add(this.term1);
            this.glossary2.Term.Add(this.term2);

            this.cache.TryAdd(new CacheKey(this.sitedir.Iid, null), new Lazy<Thing>(() => this.sitedir));
            this.cache.TryAdd(new CacheKey(this.srdl1.Iid, null), new Lazy<Thing>(() => this.srdl1));
            this.cache.TryAdd(new CacheKey(this.srdl2.Iid, null), new Lazy<Thing>(() => this.srdl2));
            this.cache.TryAdd(new CacheKey(this.glossary1.Iid, null), new Lazy<Thing>(() => this.glossary1));
            this.cache.TryAdd(new CacheKey(this.glossary2.Iid, null), new Lazy<Thing>(() => this.glossary2));
            this.cache.TryAdd(new CacheKey(this.term1.Iid, null), new Lazy<Thing>(() => this.term1));
            this.cache.TryAdd(new CacheKey(this.term2.Iid, null), new Lazy<Thing>(() => this.term2));

            this.permissionService.Setup(x => x.CanWrite(ClassKind.Term, It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
        }

        [Test]
        public void VerifyThatStartDragWorks()
        {
            var term = new Term();
            var row = new TermRowViewModel(term, this.session.Object, null);
            this.draginfo.SetupProperty(x => x.Effects);
            this.draginfo.SetupProperty(x => x.Payload);
            row.StartDrag(this.draginfo.Object);

            Assert.AreEqual(DragDropEffects.Move, this.draginfo.Object.Effects);
            Assert.AreSame(term, this.draginfo.Object.Payload);
        }

        [Test]
        public void VerifyThatDragOverWorks()
        {
            var row = new TermRowViewModel(this.term1, this.session.Object, null);

            this.dropinfo.Setup(x => x.Payload).Returns(this.sitedir);
            this.dropinfo.SetupProperty(x => x.Effects);
            row.DragOver(this.dropinfo.Object);
            Assert.AreEqual(DragDropEffects.None, this.dropinfo.Object.Effects);

            //dragover current row
            this.dropinfo.Setup(x => x.Payload).Returns(this.term1);
            row.DragOver(this.dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, this.dropinfo.Object.Effects);

            //dragover a term in a another rdl that requires the current one
            var row2 = new TermRowViewModel(this.term2, this.session.Object, null);
            row2.DragOver(this.dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, this.dropinfo.Object.Effects);

            //dragover a term in an element of the required rdl
            this.dropinfo.Setup(x => x.Payload).Returns(this.term2);
            row.DragOver(this.dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Move, this.dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDropWorks()
        {
            this.dropinfo.Setup(x => x.Payload).Returns(this.term2);
            this.dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            var row = new TermRowViewModel(this.term1, this.session.Object, null);

            row.Drop(this.dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }
    }
}