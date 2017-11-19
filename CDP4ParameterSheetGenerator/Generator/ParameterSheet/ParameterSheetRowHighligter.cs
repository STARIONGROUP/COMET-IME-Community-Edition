// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetRowHighligter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.Validation;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using CDP4ParameterSheetGenerator.ParameterSheet;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="ParameterSheetRowHighligter"/> is to highlight the rows of specific <see cref="Thing"/>s
    /// in the Parameter sheet
    /// </summary>
    public class ParameterSheetRowHighligter
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Worksheet"/> that the parameters are written to.
        /// </summary>
        private Worksheet parameterSheet;

        /// <summary>
        /// The array that contains the content of the parameter section of the parameter sheet
        /// </summary>
        private object[,] parameterContent;

        /// <summary>
        /// Highlight the rows of the <see cref="Thing"/>s in the <paramref name="things"/>
        /// </summary>
        /// <param name="application">
        /// The Excel application.
        /// </param>
        /// <param name="workbook">
        /// The current <see cref="Workbook"/>.
        /// </param>
        /// <param name="things">
        /// The <see cref="Thing"/>s that need to be highlighted in the <see cref="Workbook"/>
        /// </param>
        public void HighlightRows(Application application, Workbook workbook, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            var sw = new Stopwatch();
            sw.Start();

            application.Cursor = XlMousePointer.xlWait;

            application.StatusBar = "Highlighting rows in Parameter sheet";

            this.parameterSheet = ParameterSheetUtilities.RetrieveParameterSheet(workbook);

            try
            {
                ParameterSheetUtilities.ApplyLocking(this.parameterSheet, false);

                var parameterRange = this.parameterSheet.Range(ParameterSheetConstants.ParameterRangeName);

                this.ResetDefaultColorScheme(parameterRange);

                this.parameterContent = (object[,])parameterRange.Value;

                var currentRow = parameterRange.Row;

                application.StatusBar = string.Format("Processing Parameter sheet - row: {0}", currentRow);

                for (var i = 1; i < this.parameterContent.GetLength(0); i++)
                {
                    var rowType = (string)this.parameterContent[i, ParameterSheetConstants.TypeColumn];

                    if (rowType != null)
                    {
                        var rowIid = Convert.ToString(this.parameterContent[i, ParameterSheetConstants.IdColumn]);

                        var rowIidChar = rowIid.Split(':');
                        var thingIid = rowIidChar[0];

                        Guid iid;
                        var isIid = Guid.TryParse(thingIid, out iid);

                        //TODO: fix for compites
                        if (isIid && processedValueSets.ContainsKey(iid))
                        {
                            var row = parameterRange.Rows[i];
                            row.Interior.Color = XlRgbColor.rgbYellow;
                        }
                    }

                    currentRow++;
                }

                ParameterSheetUtilities.ApplyLocking(this.parameterSheet, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                application.Cursor = XlMousePointer.xlDefault;

                sw.Stop();
                application.StatusBar = string.Format("Highlighted rows in {0} [ms]", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Resets the color scheme of the parameter rows in the parameter sheet
        /// </summary>
        /// <param name="application">
        /// The Excel application.
        /// </param>
        /// <param name="workbook">
        /// The current <see cref="Workbook"/>.
        /// </param>
        public void ResetRows(Application application, Workbook workbook)
        {
            var sw = new Stopwatch();
            sw.Start();

            application.Cursor = XlMousePointer.xlWait;

            application.StatusBar = "CDP4: Reset row color scheme in Parameter sheet";

            this.parameterSheet = ParameterSheetUtilities.RetrieveParameterSheet(workbook);

            try
            {
                var parameterRange = this.parameterSheet.Range(ParameterSheetConstants.ParameterRangeName);
                parameterRange.Interior.Color = XlRgbColor.rgbWhite;

                ParameterSheetUtilities.ApplyLocking(this.parameterSheet, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                application.Cursor = XlMousePointer.xlDefault;

                sw.Stop();
                application.StatusBar = string.Format("CDP4: Reset row color scheme in {0} [ms]", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Set the default color scheme of the parameter range
        /// </summary>
        /// <param name="parameterRange">
        /// The parameter <see cref="Range"/>
        /// </param>
        private void ResetDefaultColorScheme(Range parameterRange)
        {
            parameterRange.Interior.Color = XlRgbColor.rgbWhite;

            var topRow = parameterRange.Rows[1];
            topRow.Interior.ColorIndex = 34;
            topRow.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            topRow.Font.Bold = true;
            topRow.Font.Underline = true;
            topRow.Font.Size = 11;
        }
    }
}
