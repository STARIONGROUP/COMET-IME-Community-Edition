// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementOrderHandlerServiceTestFixture.cs" company="RHEA System S.A.">
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
    public class RequirementOrderHandlerServiceTestFixture : OrderHandlerServiceTestFixtureBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void InsertBeforeFirst()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req4, this.req1, InsertKind.InsertBefore);
            var updatedPair = transaction.UpdatedThing.Single(x => x.Key.Iid == this.value4.Iid);
            var original = (SimpleParameterValue)updatedPair.Key;
            var updated = (SimpleParameterValue)updatedPair.Value;

            Assert.IsTrue(original.Value[0] != updated.Value[0]);
            Assert.IsTrue(int.Parse(updated.Value[0]) < int.Parse(this.value1.Value[0]));
        }

        [Test]
        public void InsertBetween()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req4, this.req2, InsertKind.InsertBefore);
            var updatedPair = transaction.UpdatedThing.Single(x => x.Key.Iid == this.value4.Iid);
            var original = (SimpleParameterValue)updatedPair.Key;
            var updated = (SimpleParameterValue)updatedPair.Value;

            Assert.IsTrue(original.Value[0] != updated.Value[0]);
            Assert.IsTrue(int.Parse(updated.Value[0]) > int.Parse(this.value1.Value[0]));
            Assert.IsTrue(int.Parse(updated.Value[0]) < int.Parse(this.value2.Value[0]));
        }

        [Test]
        public void InsertAfter()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req4, this.req2, InsertKind.InsertBefore);
            var updatedPair = transaction.UpdatedThing.Single(x => x.Key.Iid == this.value4.Iid);
            var original = (SimpleParameterValue)updatedPair.Key;
            var updated = (SimpleParameterValue)updatedPair.Value;

            Assert.IsTrue(original.Value[0] != updated.Value[0]);
            Assert.IsTrue(int.Parse(updated.Value[0]) > int.Parse(this.value1.Value[0]));
            Assert.IsTrue(int.Parse(updated.Value[0]) < int.Parse(this.value2.Value[0]));
        }

        [Test]
        public void TestInsertAfterFirst()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req4, this.req1, InsertKind.InsertAfter);
            var updatedPair = transaction.UpdatedThing.Single(x => x.Key.Iid == this.value4.Iid);
            var original = (SimpleParameterValue)updatedPair.Key;
            var updated = (SimpleParameterValue)updatedPair.Value;

            Assert.IsTrue(original.Value[0] != updated.Value[0]);
            Assert.IsTrue(int.Parse(updated.Value[0]) > int.Parse(this.value1.Value[0]));
            Assert.IsTrue(int.Parse(updated.Value[0]) < int.Parse(this.value2.Value[0]));
        }

        [Test]
        public void InsertBetween2()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req4, this.req2, InsertKind.InsertAfter);
            var updatedPair = transaction.UpdatedThing.Single(x => x.Key.Iid == this.value4.Iid);
            var original = (SimpleParameterValue)updatedPair.Key;
            var updated = (SimpleParameterValue)updatedPair.Value;

            Assert.IsTrue(original.Value[0] != updated.Value[0]);
            Assert.IsTrue(int.Parse(updated.Value[0]) > int.Parse(this.value2.Value[0]));
            Assert.IsTrue(int.Parse(updated.Value[0]) < int.Parse(this.value3.Value[0]));
        }

        [Test]
        public void TestInsertLast()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req4, this.req3, InsertKind.InsertAfter);
            var updatedPair = transaction.UpdatedThing.Single(x => x.Key.Iid == this.value4.Iid);
            var original = (SimpleParameterValue)updatedPair.Key;
            var updated = (SimpleParameterValue)updatedPair.Value;

            Assert.IsTrue(original.Value[0] != updated.Value[0]);
            Assert.IsTrue(int.Parse(updated.Value[0]) > int.Parse(this.value3.Value[0]));
        }

        [Test]
        public void TestInsertAfterWithMissing()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req23, this.req22, InsertKind.InsertAfter);

            var added = transaction.AddedThing.ToList();
            var updated = transaction.UpdatedThing.ToList();

            Assert.AreEqual(3, added.Count);
            Assert.AreEqual(3, updated.Count);

            var req23Clone = (Requirement)updated.Single(x => x.Key.Iid == this.req23.Iid).Value;
            var orderReq23 = (SimpleParameterValue)added.Single(x => req23Clone.ParameterValue.Contains(x));

            var req22Clone = (Requirement)updated.Single(x => x.Key.Iid == this.req22.Iid).Value;
            var orderReq22 = (SimpleParameterValue)added.Single(x => req22Clone.ParameterValue.Contains(x));

            var req21Clone = (Requirement)updated.Single(x => x.Key.Iid == this.req21.Iid).Value;
            var orderReq21 = (SimpleParameterValue)added.Single(x => req21Clone.ParameterValue.Contains(x));

            Assert.IsTrue(int.Parse(orderReq22.Value[0]) > int.Parse(orderReq21.Value[0]));
            Assert.IsTrue(int.Parse(orderReq22.Value[0]) < int.Parse(orderReq23.Value[0]));
        }

        [Test]
        public void TestInsertBeforeWithMissing()
        {
            var service = new RequirementOrderHandlerService(this.session.Object, this.orderType);

            var transaction = service.Insert(this.req23, this.req22, InsertKind.InsertBefore);

            var added = transaction.AddedThing.ToList();
            var updated = transaction.UpdatedThing.ToList();

            Assert.AreEqual(3, added.Count);
            Assert.AreEqual(3, updated.Count);

            var req23Clone = (Requirement)updated.Single(x => x.Key.Iid == this.req23.Iid).Value;
            var orderReq23 = (SimpleParameterValue)added.Single(x => req23Clone.ParameterValue.Contains(x));

            var req22Clone = (Requirement)updated.Single(x => x.Key.Iid == this.req22.Iid).Value;
            var orderReq22 = (SimpleParameterValue)added.Single(x => req22Clone.ParameterValue.Contains(x));

            var req21Clone = (Requirement)updated.Single(x => x.Key.Iid == this.req21.Iid).Value;
            var orderReq21 = (SimpleParameterValue)added.Single(x => req21Clone.ParameterValue.Contains(x));

            Assert.IsTrue(int.Parse(orderReq23.Value[0]) > int.Parse(orderReq21.Value[0]));
            Assert.IsTrue(int.Parse(orderReq22.Value[0]) > int.Parse(orderReq23.Value[0]));

        }
    }
}
