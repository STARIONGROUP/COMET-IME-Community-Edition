// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionHelperTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.DragDrop
{
    using System.Windows.Controls;
    using CDP4Composition.DragDrop;
    using DevExpress.Xpf.Grid;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="SelectionHelper"/>
    /// </summary>
    [TestFixture, RequiresSTA]
    public class SelectionHelperTestFixture
    {
        [Test]
        public void VerifyThatIsMultiSelectEnabledReturnsExpectedResultForDataGrid()
        {
            var datagrid = new DataGrid();
            var datagridresult = SelectionHelper.IsMultiSelectEnabled(datagrid);
            Assert.IsTrue(datagridresult);
        }

        [Test]
        public void VerifyThatIsMultiSelectEnabledReturnsExpectedResultForListBox()
        {
            var listboxresult = false;

            var listbox = new ListBox();
            listbox.SelectionMode = SelectionMode.Multiple;
            listboxresult = SelectionHelper.IsMultiSelectEnabled(listbox);
            Assert.IsTrue(listboxresult);

            listbox.SelectionMode = SelectionMode.Extended;
            listboxresult = SelectionHelper.IsMultiSelectEnabled(listbox);
            Assert.IsTrue(listboxresult);

            listbox.SelectionMode = SelectionMode.Single;
            listboxresult = SelectionHelper.IsMultiSelectEnabled(listbox);
            Assert.IsFalse(listboxresult);
        }

        [Test]
        public void VerifyThatIsMultiSelectEnabledReturnsExpectedResultForDevExpressGridControl()
        {
            var gridControlResult = false;
            var gridControl = new GridControl();
            
            gridControl.SelectionMode = MultiSelectMode.MultipleRow;

            gridControlResult = SelectionHelper.IsMultiSelectEnabled(gridControl);
            Assert.IsTrue(gridControlResult);

            gridControl.SelectionMode = MultiSelectMode.Row;
            gridControlResult = SelectionHelper.IsMultiSelectEnabled(gridControl);
            Assert.IsTrue(gridControlResult);

            gridControl.SelectionMode = MultiSelectMode.Cell;
            gridControlResult = SelectionHelper.IsMultiSelectEnabled(gridControl);
            Assert.IsTrue(gridControlResult);

            gridControl.SelectionMode = MultiSelectMode.None;
            gridControlResult = SelectionHelper.IsMultiSelectEnabled(gridControl);
            Assert.IsFalse(gridControlResult);
        }
    }
}
