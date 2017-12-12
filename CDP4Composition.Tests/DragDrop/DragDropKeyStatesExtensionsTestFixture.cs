// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragDropKeyStatesExtensionsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.DragDrop
{
    using System.Windows;
    using CDP4Dal.Operations;
    using CDP4Composition.DragDrop;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DragDropKeyStatesExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class DragDropKeyStatesExtensionsTestFixture
    {
        [Test]
        public void VerifyThatGetOperationKindReturnsTheExpectedResults()
        {
            Assert.AreEqual(OperationKind.CopyDefaultValuesChangeOwner, DragDropKeyStatesExtensions.GetCopyOperationKind(Constants.DryCopy));

            Assert.AreEqual(OperationKind.CopyKeepValuesChangeOwner, DragDropKeyStatesExtensions.GetCopyOperationKind(Constants.CtrlCopy));

            Assert.AreEqual(OperationKind.Copy, DragDropKeyStatesExtensions.GetCopyOperationKind(Constants.ShiftCopy));

            Assert.AreEqual(OperationKind.CopyKeepValues, DragDropKeyStatesExtensions.GetCopyOperationKind(Constants.CtlrShiftCopy));

            Assert.IsNull(DragDropKeyStatesExtensions.GetCopyOperationKind(DragDropKeyStates.ShiftKey));
        }
    }
}
