// ------------------------------------------------------------------------------------------------
// <copyright file="WorkbookRebuildDialogResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
    using System.ComponentModel;
    using CDP4Composition.Navigation;

    /// <summary>
    /// Enumeration that specifies how the parameter sheet shall be rebuilt.
    /// </summary>
    public enum RebuildKind
    {
        /// <summary>
        /// Rebuild overwriting any changes that where made by the user.
        /// </summary>        
        [Description("Rebuild without keeping changes.")]
        Overwrite = 0,

        /// <summary>
        /// Rebuild, restoring any changes that where made by the user.
        /// </summary>
        [Description("Rebuild restoring changes.")]
        RestoreChanges = 1
    }

    /// <summary>
    /// The purpose of the <see cref="WorkbookRebuildDialogResult"/> is to return a value
    /// that specifies that the workbook shall be rebuilt overwriting any changes, or restoring
    /// the changes that where made.
    /// </summary>
    public class WorkbookRebuildDialogResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookRebuildDialogResult"/> class.
        /// </summary>
        /// <param name="res">
        /// The result
        /// </param>
        /// <param name="rebuildKind">
        /// The <see cref="RebuildKind"/> that specifies how the workbook is to be rebuilt.
        /// </param>
        public WorkbookRebuildDialogResult(bool? res, RebuildKind rebuildKind)
            : base(res)
        {
            this.RebuildKind = rebuildKind;
        }

        /// <summary>
        /// Gets the value indicating how the workbook is to be rebuilt.
        /// </summary>
        public RebuildKind RebuildKind { get; private set; }
    }
}
