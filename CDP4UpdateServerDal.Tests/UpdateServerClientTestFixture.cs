// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateServerClientTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4UpdateServerDal.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using CDP4Composition.Modularity;

    using CDP4UpdateServerDal.Enumerators;

    using DevExpress.XtraPrinting.Native;

    using Moq;
    using Moq.Protected;

    using NUnit.Framework;

    public class UpdateServerClientTestFixture
    {
        private string consolidateImeResponse;
        private string consolidatePluginResponse;
        private List<Manifest> installedManifest;
        private const string PluginName0 = "Plugin0";
        private const string PluginName1 = "Plugin1";
        private const string PluginName2 = "Plugin2";
        private const string Version0 = "0.1.0.0";
        private const string Version1 = "0.2.0.0";
        private const string BaseAddress = "http://test.test/";
        private const string MethodName = "SendAsync";

        [SetUp]
        public void Setup()
        {
            this.consolidateImeResponse = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Json", "ConsolidateImeResponse.json"));
            this.consolidatePluginResponse = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Json", "ConsolidatePluginResponse.json"));

            this.installedManifest = new List<Manifest>()
            {
                new Manifest() { Name = PluginName0, Version = Version0},
                new Manifest() { Name = PluginName1, Version = Version0},
                new Manifest() { Name = PluginName2, Version = Version0}
            };
        }

        [Test]
        public void VerifyProperty()
        {
            var client = new UpdateServerClient(new Uri(BaseAddress));
            Assert.IsNotNull(client.BaseAddress);
        }

        [Test]
        public async Task VerifyGetLatestPlugin()
        {
            var handlerMock = this.SetupResponseHandler(this.consolidatePluginResponse);

            var client = new UpdateServerClient(new Uri(BaseAddress), handlerMock.Object);
            
            var result = await client.GetLatestPlugin(this.installedManifest, new Version(Version0));

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());

            handlerMock.Protected().Verify(
                MethodName,
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Post && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/plugin"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task VerifyGetLatestIme()
        {
            var handlerMock = this.SetupResponseHandler(this.consolidateImeResponse);

            var client = new UpdateServerClient(new Uri(BaseAddress), handlerMock.Object);
            
            var result = await client.GetLatestIme(new Version(Version0), Platform.X64);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Platforms.Count);

            handlerMock.Protected().Verify(
                MethodName,
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/ime"),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Test]
        public async Task VerifyDownloadPlugin()
        {
            var handlerMock = this.SetupResponseHandler("");

            var client = new UpdateServerClient(new Uri(BaseAddress), handlerMock.Object);

            var result = await client.DownloadPlugin(PluginName0, Version1);

            Assert.IsNotNull(result);
            
            handlerMock.Protected().Verify(
                MethodName,
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/plugin/{PluginName0}/{Version1}/download"),
            ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task VerifyDownloadIME()
        {
            var handlerMock = this.SetupResponseHandler("");

            var client = new UpdateServerClient(new Uri(BaseAddress), handlerMock.Object);

            var result = await client.DownloadIme(Version1, Platform.X64);

            Assert.IsNotNull(result);

            handlerMock.Protected().Verify(
                MethodName,
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/ime/{Version1}/X64/download"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task VerifyCheckForUpdate()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(MethodName,
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Post && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/plugin"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent(this.consolidatePluginResponse) })
                .Verifiable();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(MethodName,
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Post && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/ime"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent(this.consolidateImeResponse) })
                .Verifiable();

            var client = new UpdateServerClient(new Uri(BaseAddress), handlerMock.Object);

            var result = await client.CheckForUpdate(this.installedManifest, new Version(Version0), ProcessorArchitecture.Amd64);

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count());

            handlerMock.Protected().Verify(
                MethodName,
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/ime"),
                ItExpr.IsAny<CancellationToken>()
            );

            handlerMock.Protected().Verify(
                MethodName,
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post && r.RequestUri.AbsoluteUri == $"{BaseAddress}api/plugin"),
                ItExpr.IsAny<CancellationToken>());
        }

        private Mock<HttpMessageHandler> SetupResponseHandler(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(MethodName,
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(response) })
                .Verifiable();

            return handlerMock;
        }

        [Test]
        public void VerifyServerUnreachable()
        {
            var client = new UpdateServerClient { BaseAddress = new Uri(BaseAddress) };

            Assert.ThrowsAsync<HttpRequestException>(
                async () => await client.GetLatestIme(new Version(Version1), Platform.X86));
        }
    }
}