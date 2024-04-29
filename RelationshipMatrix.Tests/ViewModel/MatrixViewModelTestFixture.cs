// -------------------------------------------------------------------------------------------------
// <copyright file="MatrixViewModelTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using CDP4RelationshipMatrix.DataTypes;
    using CDP4RelationshipMatrix.ViewModels;

    using NUnit.Framework;

    public class MatrixViewModelTestFixture : ViewModelTestBase
    {
        private MatrixViewModel vm;
        private MatrixAddress matrixAdress1;
        private MatrixAddress matrixAdress2;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            this.vm = new MatrixViewModel(this.session.Object, this.iteration, null);
            this.matrixAdress1 = new MatrixAddress();
            this.matrixAdress2 = new MatrixAddress(){Column = "column", Row = 1};
        }

        [Test]
        public void AssertViewModelWorks()
        {
            Assert.DoesNotThrow(() => this.vm.MouseDownCommand.Execute(null));
            Assert.DoesNotThrow(() => this.vm.MouseDownCommand.Execute(this.matrixAdress1));
            Assert.DoesNotThrow(() => this.vm.MouseDownCommand.Execute(this.matrixAdress2));
        }
    }
}
