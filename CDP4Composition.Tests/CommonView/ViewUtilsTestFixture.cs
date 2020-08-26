// -------------------------------------------------------------------------------------------------
// <copyright file="ViewUtilsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

using System.Windows.Data;

namespace CDP4CommonView.Tests
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ViewUtils"/> class
    /// </summary>
    [TestFixture]
    public class ViewUtilsTestFixture
    {
        [Test]
        public void VerifyThatIfDataContextIsNullArgumentExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => ViewUtils.CreateBinding(null, null, BindingMode.TwoWay, UpdateSourceTrigger.PropertyChanged));
        }

        [Test]
        public void VerifyThatIfPathIsNullOrWhiteSpaceExceptionIsThrown()
        {
            var dataContext = new object();

            Assert.Throws<ArgumentNullException>(() => ViewUtils.CreateBinding(dataContext, null, BindingMode.TwoWay, UpdateSourceTrigger.PropertyChanged));
        }

        [Test]
        public void VerifyThatPropertiesOfBindingAreSet()
        {
            var dataContext = new object();
            var path = "PropertyName";
            
            var binding = ViewUtils.CreateBinding(dataContext, path, BindingMode.TwoWay, UpdateSourceTrigger.PropertyChanged);

            Assert.AreEqual(dataContext, binding.Source);
            Assert.IsNotNull(binding.Path);
            Assert.AreEqual(BindingMode.TwoWay, binding.Mode);
            Assert.AreEqual(UpdateSourceTrigger.PropertyChanged, binding.UpdateSourceTrigger);
        }
    }
}
