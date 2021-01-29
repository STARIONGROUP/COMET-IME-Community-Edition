// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterArrayAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ParameterArrayAssembler"/> is to create the arrays that are
    /// used to populate the Parameter Sheet
    /// </summary>
    public class ParameterArrayAssembler
    {
        /// <summary>
        /// The <see cref="IExcelRow{T}"/> that are being used to populate the various arrays
        /// </summary>
        private readonly IEnumerable<IExcelRow<Thing>> excelRows;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterArrayAssembler"/> class.
        /// </summary>
        /// <param name="excelRows">
        /// The excel rows
        /// </param>
        public ParameterArrayAssembler(IEnumerable<IExcelRow<Thing>> excelRows)
        {
            this.excelRows = excelRows;

            this.InitializeArrays();
            this.PopulateParameterArray();
            this.PopulateLockArray();
            this.PopulateFormatArray();
        }

        /// <summary>
        /// Gets the array that contains the parameter sheet information
        /// </summary>
        public object[,] ContentArray { get; private set; }

        /// <summary>
        /// Gets the array that contains formatting information
        /// </summary>
        public object[,] FormatArray { get; private set; }

        /// <summary>
        /// Gets the array that contains the parameter sheet information
        /// </summary>
        public object[,] FormulaArray { get; private set; }

        /// <summary>
        /// Gets the array that contains locked cells information
        /// </summary>
        public object[,] LockArray { get; private set; }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet 
        /// </summary>
        private void InitializeArrays()
        {
            var nrofrows = this.excelRows.Count() + 2;

            this.ContentArray = new object[nrofrows, 13];
            this.LockArray = new object[nrofrows, 13];
            this.FormatArray = new object[nrofrows, 13];            
            this.FormulaArray = new object[nrofrows, 1];            
        }

        /// <summary>
        /// populate the <see cref="ContentArray"/> with data
        /// </summary>
        private void PopulateParameterArray()
        {
            this.ContentArray[0, 0] = "Name";
            this.ContentArray[0, 1] = "Short Name";
            this.ContentArray[0, 2] = "Computed";
            this.ContentArray[0, 3] = "Manual";
            this.ContentArray[0, 4] = "Reference";
            this.ContentArray[0, 5] = "Switch";
            this.ContentArray[0, 6] = "Actual Value";
            this.ContentArray[0, 7] = "Model Code";
            this.ContentArray[0, 8] = "Parameter Type";
            this.ContentArray[0, 9] = "Owner";
            this.ContentArray[0, 10] = "Category";
            this.ContentArray[0, 11] = "TYPE";
            this.ContentArray[0, 12] = "ValueSet Id";

            var i = 2;
            foreach (var excelRow in this.excelRows)
            {
                this.ContentArray[i, 0] = excelRow.Name;
                this.ContentArray[i, 1] = excelRow.ShortName;
                this.ContentArray[i, 2] = excelRow.ComputedValue;
                this.ContentArray[i, 3] = excelRow.ManualValue;
                this.ContentArray[i, 4] = excelRow.ReferenceValue;
                this.ContentArray[i, 5] = excelRow.Switch;
                this.ContentArray[i, 6] = excelRow.ActualValue;
                this.ContentArray[i, 7] = excelRow.ModelCode;
                this.ContentArray[i, 8] = excelRow.ParameterTypeShortName;
                this.ContentArray[i, 9] = excelRow.Owner;
                this.ContentArray[i, 10] = excelRow.Categories;
                this.ContentArray[i, 11] = excelRow.Type;
                this.ContentArray[i, 12] = excelRow.Id;

                i++;
            }
        }

        /// <summary>
        /// Populate the <see cref="FormatArray"/> with formatting data
        /// </summary>
        private void PopulateFormatArray()
        {
            for (var j = this.FormatArray.GetLowerBound(1); j <= this.FormatArray.GetUpperBound(1); j++)
            {
                this.FormatArray[0, j] = "@";
            }

            var i = 2;
            foreach (var excelRow in this.excelRows)
            {
                this.FormatArray[i, 0] = "@";              // Name
                this.FormatArray[i, 1] = "@";              // Short Name
                this.FormatArray[i, 2] = (excelRow.Format == "@") ? "general" : excelRow.Format; // ComputedValue
                this.FormatArray[i, 3] = excelRow.Format;  // ManualValue
                this.FormatArray[i, 4] = excelRow.Format;  // ReferenceValue
                this.FormatArray[i, 5] = "general";        // Switch
                this.FormatArray[i, 6] = (excelRow.Format == "@") ? "general" : excelRow.Format; // ActualValue
                this.FormatArray[i, 7] = "@";              // Model Code
                this.FormatArray[i, 8] = "@";              // ParameterTypeShortName
                this.FormatArray[i, 9] = "@";              // Owner
                this.FormatArray[i, 10] = "@";             // Categories
                this.FormatArray[i, 11] = "@";             // Type
                this.FormatArray[i, 12] = "@";             // Id

                i++;
            }
        }

        /// <summary>
        /// Populate the <see cref="LockArray"/> to enable locking and unlocking cells
        /// </summary>
        private void PopulateLockArray()
        {
            for (var j = this.LockArray.GetLowerBound(1); j <= this.LockArray.GetUpperBound(1); j++)
            {
                this.LockArray[0, j] = true;
            }

            var i = 2;
            foreach (var excelRow in this.excelRows)
            {
                this.LockArray[i, 0] = true;  // Name
                this.LockArray[i, 1] = true;  // Short Name

                if (excelRow.Type == ParameterSheetConstants.PVS || excelRow.Type == ParameterSheetConstants.PVSCT || excelRow.Type == ParameterSheetConstants.POVS || excelRow.Type == ParameterSheetConstants.POVSCT)
                {
                    this.LockArray[i, 2] = false;  // ComputedValue
                }
                else
                {
                    this.LockArray[i, 2] = true;  // ComputedValue
                }

                // ManualValue
                this.LockArray[i, 3] = excelRow.Type == ParameterSheetConstants.ED || excelRow.Type == ParameterSheetConstants.PG 
                    || excelRow.Type == ParameterSheetConstants.PVSCD  ||  excelRow.Type == ParameterSheetConstants.POVSCD  || excelRow.Type == ParameterSheetConstants.PSVSCD;

                if (excelRow.Type == ParameterSheetConstants.PVS || excelRow.Type == ParameterSheetConstants.PVSCT || excelRow.Type == ParameterSheetConstants.POVS || excelRow.Type == ParameterSheetConstants.POVSCT)
                {
                    this.LockArray[i, 4] = false; // ReferenceValue
                }
                else
                {
                    this.LockArray[i, 4] = true; // ReferenceValue    
                }

                this.LockArray[i, 5] = this.IsSwitchCellLocked(excelRow); // Switch
                this.LockArray[i, 6] = true;  // ActualValue
                this.LockArray[i, 7] = true;  // Model Code
                this.LockArray[i, 8] = true;  // ParameterTypeShortName
                this.LockArray[i, 9] = true;  // Owner
                this.LockArray[i, 10] = true; // Categories
                this.LockArray[i, 11] = true; // Type
                this.LockArray[i, 12] = true; // Id

                i++;
            }
        }

        /// <summary>
        /// Asserts whether the switch cell of the <paramref name="excelRow"/> needs to be locked or not
        /// </summary>
        /// <param name="excelRow">
        /// The <see cref="IExcelRow{T}"/> for which the locking needs to be determined
        /// </param>
        /// <returns>
        /// returns true if the cell needs to be locked, false if it should not be locked
        /// </returns>
        private bool IsSwitchCellLocked(IExcelRow<Thing> excelRow)
        {
            switch (excelRow.Type)
            {
                case ParameterSheetConstants.POVSCD:
                    return false;
                case ParameterSheetConstants.PSVSCD:
                    return false;
                case ParameterSheetConstants.PVSCD:
                    return false;
                case ParameterSheetConstants.POVS:
                    return false;
                case ParameterSheetConstants.PSVS:
                    return false;
                case ParameterSheetConstants.PVS:
                    return false;
                default:
                    return true;
            }
        }
    }
}
