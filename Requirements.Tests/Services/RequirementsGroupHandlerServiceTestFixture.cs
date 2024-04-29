// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsGroupHandlerServiceTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace CDP4Requirements.Tests
{
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using NUnit.Framework;
    using Utils;

    [TestFixture]
    public class RequirementsGroupHandlerServiceTestFixture : OrderHandlerServiceTestFixtureBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void TestInsertAfterWithMissingOrderValue()
        {
            var service = new RequirementsGroupOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.grp3, this.grp4, InsertKind.InsertAfter);

            var added = transaction.AddedThing.ToList();
            var updated = transaction.UpdatedThing.ToList();

            Assert.AreEqual(2, added.OfType<RequirementsContainerParameterValue>().Count());
            Assert.AreEqual(3, updated.Count);

            var gr3Clone = (RequirementsGroup)updated.Single(x => x.Key.Iid == this.grp3.Iid).Value;
            var orderGr3 = (RequirementsContainerParameterValue)added.Single(x => gr3Clone.ParameterValue.Contains(x));

            var gr4Clone = (RequirementsGroup)updated.Single(x => x.Key.Iid == this.grp4.Iid).Value;
            var orderGr4 = (RequirementsContainerParameterValue)added.Single(x => gr4Clone.ParameterValue.Contains(x));

            var grp1Clone = (RequirementsGroup)updated.Single(x => x.Key.Iid == this.grp1.Iid).Value;

            Assert.IsTrue(int.Parse(orderGr4.Value[0]) < int.Parse(orderGr3.Value[0]));
            Assert.IsTrue(grp1Clone.Group.Contains(gr3Clone));
        }

        [Test]
        public void TestInsertBeforeWithMissingOrderValue()
        {
            var service = new RequirementsGroupOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.grp3, this.grp4, InsertKind.InsertBefore);

            var added = transaction.AddedThing.ToList();
            var updated = transaction.UpdatedThing.ToList();

            Assert.AreEqual(2, added.OfType<RequirementsContainerParameterValue>().Count());
            Assert.AreEqual(3, updated.Count);

            var gr3Clone = (RequirementsGroup)updated.Single(x => x.Key.Iid == this.grp3.Iid).Value;
            var orderGr3 = (RequirementsContainerParameterValue)added.Single(x => gr3Clone.ParameterValue.Contains(x));

            var gr4Clone = (RequirementsGroup)updated.Single(x => x.Key.Iid == this.grp4.Iid).Value;
            var orderGr4 = (RequirementsContainerParameterValue)added.Single(x => gr4Clone.ParameterValue.Contains(x));

            var grp1Clone = (RequirementsGroup)updated.Single(x => x.Key.Iid == this.grp1.Iid).Value;

            Assert.IsTrue(int.Parse(orderGr4.Value[0]) > int.Parse(orderGr3.Value[0]));
            Assert.IsTrue(grp1Clone.Group.Contains(gr3Clone));
        }
    }
}
