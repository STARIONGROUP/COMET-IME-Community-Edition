// -------------------------------------------------------------------------------------------------
// <copyright file="LogLevelImageConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Tests
{
    using System;
    using System.IO.Packaging;
    using System.Threading;
    using CDP4LogInfo.Views;
    using NUnit.Framework;
    
    [TestFixture, Apartment(ApartmentState.STA)]
    public class LogLevelImageConverterTestFixture
    {
        [Test]
        public void TestConvert()
        {
            PackUriHelper.Create(new Uri("reliable://0"));
            //new FrameworkElement();            
            var converter = new LogLevelImageConverter();

            var im = converter.Convert("Info", null, null, null);
            Assert.IsNotNull(im);

            im = converter.Convert("Warn", null, null, null);
            Assert.IsNotNull(im);

            im = converter.Convert("Error", null, null, null);
            Assert.IsNotNull(im);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            var converter = new LogLevelImageConverter();
            Assert.Throws<NotImplementedException>(() =>  converter.ConvertBack(null, null, null, null));
        }
    }
}