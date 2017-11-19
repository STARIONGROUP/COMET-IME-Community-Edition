// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedElementParameterArrayAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.OptionSheet
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="NestedElementParameterArrayAssembler"/> is to create the arrays that are
    /// used to populate the Option Sheet with <see cref="NestedElement"/>s and <see cref="NestedParameter"/>s
    /// </summary>
    public class NestedElementParameterArrayAssembler
    {
        /// <summary>
        /// The <see cref="IExcelRow{T}"/> that are being used to populate the various arrays
        /// </summary>
        private readonly IEnumerable<IExcelRow<Thing>> excelRows;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedElementParameterArrayAssembler"/> class.
        /// </summary>
        /// <param name="excelRows">
        /// The excel rows
        /// </param>
        public NestedElementParameterArrayAssembler(IEnumerable<IExcelRow<Thing>> excelRows)
        {
            this.excelRows = excelRows;

            this.InitializeArrays();
            this.PopulateContentArray();
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

            this.ContentArray = new object[nrofrows, 9];
            this.LockArray = new object[nrofrows, 9];
            this.FormatArray = new object[nrofrows, 9];
            this.FormulaArray = new object[nrofrows, 1];
        }

        /// <summary>
        /// populate the <see cref="ContentArray"/> with data
        /// </summary>
        private void PopulateContentArray()
        {
            this.ContentArray[0, 0] = "Name";
            this.ContentArray[0, 1] = "Short Name";
            this.ContentArray[0, 2] = "Model Code";
            this.ContentArray[0, 3] = "Actual Value";            
            this.ContentArray[0, 4] = "Parameter Type";
            this.ContentArray[0, 5] = "Owner";
            this.ContentArray[0, 6] = "Category";
            this.ContentArray[0, 7] = "TYPE";
            this.ContentArray[0, 8] = "ValueSet Id";

            int i = 2;
            foreach (var excelRow in this.excelRows)
            {
                this.ContentArray[i, 0] = excelRow.Name;
                this.ContentArray[i, 1] = excelRow.ShortName;
                this.ContentArray[i, 2] = excelRow.ModelCode;
                this.ContentArray[i, 3] = excelRow.ActualValue;                
                this.ContentArray[i, 4] = excelRow.ParameterTypeShortName;
                this.ContentArray[i, 5] = excelRow.Owner;
                this.ContentArray[i, 6] = excelRow.Categories;
                this.ContentArray[i, 7] = excelRow.Type;
                this.ContentArray[i, 8] = excelRow.Id;

                i++;
            }
        }

        /// <summary>
        /// Populate the <see cref="FormatArray"/> with formatting data
        /// </summary>
        private void PopulateFormatArray()
        {
            for (int j = this.FormatArray.GetLowerBound(1); j < this.FormatArray.GetUpperBound(1); j++)
            {
                this.FormatArray[0, j] = "@";
            }

            int i = 2;
            foreach (var excelRow in this.excelRows)
            {
                this.FormatArray[i, 0] = "@";              // Name
                this.FormatArray[i, 1] = "@";              // Short Name
                this.FormatArray[i, 2] = "@";              // Model Code
                this.FormatArray[i, 3] = (excelRow.Format == "@") ? "general" : excelRow.Format; // ActualValue                
                this.FormatArray[i, 4] = "@";              // ParameterTypeShortName
                this.FormatArray[i, 5] = "@";              // Owner
                this.FormatArray[i, 6] = "@";             // Categories
                this.FormatArray[i, 7] = "@";             // Type
                this.FormatArray[i, 8] = "@";             // Id

                i++;
            }
        }

        /// <summary>
        /// Populate the <see cref="LockArray"/> to enable locking and unlocking cells
        /// </summary>
        private void PopulateLockArray()
        {
            for (int j = this.LockArray.GetLowerBound(1); j < this.LockArray.GetUpperBound(1); j++)
            {
                this.LockArray[0, j] = true;
            }

            int i = 2;
            foreach (var excelRow in this.excelRows)
            {
                this.LockArray[i, 0] = true;  // Name
                this.LockArray[i, 1] = true;  // Short Name
                this.LockArray[i, 2] = true;  // Model Code
                this.LockArray[i, 3] = true;  // ActualValue
                this.LockArray[i, 4] = true;  // ParameterTypeShortName
                this.LockArray[i, 5] = true;  // Owner
                this.LockArray[i, 6] = true;  // Categories
                this.LockArray[i, 7] = true;  // Type
                this.LockArray[i, 8] = true;  // Id

                i++;
            }
        }
    }
}
