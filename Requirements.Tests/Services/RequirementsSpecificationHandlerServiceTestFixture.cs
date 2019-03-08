// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationHandlerServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace CDP4Requirements.Tests
{
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using NUnit.Framework;
    using Utils;

    [TestFixture]
    public class RequirementsSpecificationHandlerServiceTestFixture : OrderHandlerServiceTestFixtureBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void TestInsertAfterWithMissingOrderValue()
        {
            var service = new RequirementsSpecificationOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.spec1, this.spec3, InsertKind.InsertAfter);

            var added = transaction.AddedThing.ToList();
            var updated = transaction.UpdatedThing.ToList();

            Assert.AreEqual(3, added.OfType<RequirementsContainerParameterValue>().Count());
            Assert.AreEqual(3, updated.Count);

            var spec1Clone = (RequirementsSpecification)updated.Single(x => x.Key.Iid == this.spec1.Iid).Value;
            var orderSpec1 = (RequirementsContainerParameterValue)added.Single(x => spec1Clone.ParameterValue.Contains(x));

            var spec3Clone = (RequirementsSpecification)updated.Single(x => x.Key.Iid == this.spec3.Iid).Value;
            var orderSpec4 = (RequirementsContainerParameterValue)added.Single(x => spec3Clone.ParameterValue.Contains(x));

            Assert.IsTrue(int.Parse(orderSpec4.Value[0]) < int.Parse(orderSpec1.Value[0]));
        }

        [Test]
        public void TestInsertBeforeWithMissingOrderValue()
        {
            var service = new RequirementsSpecificationOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.spec1, this.spec3, InsertKind.InsertBefore);

            var added = transaction.AddedThing.ToList();
            var updated = transaction.UpdatedThing.ToList();

            Assert.AreEqual(3, added.OfType<RequirementsContainerParameterValue>().Count());
            Assert.AreEqual(3, updated.Count);

            var spec1Clone = (RequirementsSpecification)updated.Single(x => x.Key.Iid == this.spec1.Iid).Value;
            var orderSpec1 = (RequirementsContainerParameterValue)added.Single(x => spec1Clone.ParameterValue.Contains(x));

            var spec3Clone = (RequirementsSpecification)updated.Single(x => x.Key.Iid == this.spec3.Iid).Value;
            var orderSpec3 = (RequirementsContainerParameterValue)added.Single(x => spec3Clone.ParameterValue.Contains(x));

            Assert.IsTrue(int.Parse(orderSpec3.Value[0]) > int.Parse(orderSpec1.Value[0]));
        }
    }
}
