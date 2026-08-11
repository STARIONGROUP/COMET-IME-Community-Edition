// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVWorkbookExporter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4VandV.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    using ClosedXML.Excel;

    /// <summary>
    /// Exports the V&amp;V deliverables of an <see cref="Iteration"/> to a single Excel workbook: the <b>VCD</b>
    /// register (one row per V&amp;V activity), the <b>RVM</b> coverage matrix (requirements × stage gates), an
    /// <b>Execution Records</b> sheet, and an <b>NCR</b> list.
    /// </summary>
    public class VandVWorkbookExporter
    {
        /// <summary>
        /// Writes the workbook for the supplied iteration to <paramref name="path"/>.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to export.</param>
        /// <param name="path">The full path of the <c>.xlsx</c> file to write.</param>
        public void Export(Iteration iteration, string path)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A destination path must be supplied.", nameof(path));
            }

            var model = VandVCoverageQuery.Build(iteration);

            using (var workbook = new XLWorkbook())
            {
                ConstructVcdSheet(workbook, iteration, model);
                ConstructRvmSheet(workbook, model);
                ConstructExecutionSheet(workbook, model);
                ConstructProcedureSheet(workbook, iteration, model);
                ConstructNcrSheet(workbook, iteration);

                workbook.SaveAs(path);
            }
        }

        /// <summary>
        /// Builds the VCD sheet, the working register, one row per V&amp;V activity.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="iteration">The iteration, needed for the requirement traceability and the analysis check.</param>
        /// <param name="model">The coverage model.</param>
        private static void ConstructVcdSheet(XLWorkbook workbook, Iteration iteration, VandVCoverageModel model)
        {
            var sheet = workbook.Worksheets.Add("VCD");

            // ECSS-E-ST-10-02 Annex B: the VCD must carry the requirement text and the traceability between
            // requirements, plus compliance and close-out as columns distinct from the execution status
            var headers = new[]
            {
                "Requirement", "Requirement Name", "Requirement Text", "Parent Requirement", "V&V Item",
                "V&V Item Name", "Method", "Stage", "Level", "Criticality", "Owner", "Plan Ref.", "Activity No.",
                "Planned", "Actual", "Status", "Compliance", "Closed", "Close-out Reason", "Closed By", "Closed On",
                "Acceptance Criteria", "Conditions", "Facility", "Result", "Evidence", "Analysis Check"
            };

            WriteHeader(sheet, headers);

            var row = 2;

            foreach (var coverage in model.Coverages)
            {
                var requirementText = QueryRequirementText(coverage.Requirement);
                var parent = QueryParentRequirements(iteration, coverage.Requirement);

                if (!coverage.VandVItems.Any())
                {
                    sheet.Cell(row, 1).Value = coverage.Requirement.ShortName;
                    sheet.Cell(row, 2).Value = coverage.Requirement.Name;
                    sheet.Cell(row, 3).Value = requirementText;
                    sheet.Cell(row, 4).Value = parent;
                    sheet.Cell(row, 5).Value = "(not covered)";
                    sheet.Row(row).Style.Font.FontColor = XLColor.Red;
                    row++;
                    continue;
                }

                foreach (var item in coverage.VandVItems)
                {
                    var analysis = VandVAnalysisChecker.Check(iteration, item);
                    var compliance = VandVCloseOut.QueryCompliance(item);

                    sheet.Cell(row, 1).Value = coverage.Requirement.ShortName;
                    sheet.Cell(row, 2).Value = coverage.Requirement.Name;
                    sheet.Cell(row, 3).Value = requirementText;
                    sheet.Cell(row, 4).Value = parent;
                    sheet.Cell(row, 5).Value = item.ShortName;
                    sheet.Cell(row, 6).Value = item.Name;
                    sheet.Cell(row, 7).Value = VandVCoverageQuery.Attribute(item, "vnv_method");
                    sheet.Cell(row, 8).Value = VandVCoverageQuery.Attribute(item, "vnv_stage");
                    sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(item, "vnv_level");
                    sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(item, "vnv_criticality");
                    sheet.Cell(row, 11).Value = item.Owner?.ShortName;
                    sheet.Cell(row, 12).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.PlanReferenceShortName);
                    sheet.Cell(row, 13).Value = VandVCoverageQuery.Attribute(item, "vnv_activity_no");
                    sheet.Cell(row, 14).Value = VandVCoverageQuery.Attribute(item, "vnv_planned_date");
                    sheet.Cell(row, 15).Value = VandVCoverageQuery.Attribute(item, "vnv_actual_date");
                    sheet.Cell(row, 16).Value = VandVCoverageQuery.Attribute(item, "vnv_status");
                    sheet.Cell(row, 17).Value = compliance;
                    sheet.Cell(row, 18).Value = VandVCloseOut.IsClosed(item) ? "Closed" : "Open";
                    sheet.Cell(row, 19).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.CloseOutReasonShortName);
                    sheet.Cell(row, 20).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.ClosedByShortName);
                    sheet.Cell(row, 21).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.ClosedOnShortName);
                    sheet.Cell(row, 22).Value = VandVCoverageQuery.Attribute(item, "vnv_acceptance");
                    sheet.Cell(row, 23).Value = VandVCoverageQuery.Attribute(item, "vnv_conditions");
                    sheet.Cell(row, 24).Value = VandVCoverageQuery.Attribute(item, "vnv_facility");
                    sheet.Cell(row, 25).Value = VandVCoverageQuery.Attribute(item, "vnv_result");
                    sheet.Cell(row, 26).Value = VandVCoverageQuery.Attribute(item, "vnv_evidence_ref");
                    sheet.Cell(row, 27).Value = analysis.Display;

                    if (analysis.State == VandVAnalysisState.Violated)
                    {
                        sheet.Cell(row, 27).Style.Font.FontColor = XLColor.Red;
                    }

                    if (VandVCloseOut.IsShortfall(compliance))
                    {
                        sheet.Cell(row, 17).Style.Font.FontColor = XLColor.Red;
                    }

                    row++;
                }
            }

            Finish(sheet, headers.Length);
        }

        /// <summary>
        /// Builds the RVM sheet, requirements down, stage gates across, showing the status of each activity so that
        /// uncovered requirements are visible at a glance.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="model">The coverage model.</param>
        private static void ConstructRvmSheet(XLWorkbook workbook, VandVCoverageModel model)
        {
            var sheet = workbook.Worksheets.Add("RVM");

            var headers = new List<string> { "Requirement", "Requirement Name" };
            headers.AddRange(model.Stages);
            headers.Add("Covered");

            WriteHeader(sheet, headers.ToArray());

            var row = 2;

            foreach (var coverage in model.Coverages)
            {
                sheet.Cell(row, 1).Value = coverage.Requirement.ShortName;
                sheet.Cell(row, 2).Value = coverage.Requirement.Name;

                for (var stageIndex = 0; stageIndex < model.Stages.Count; stageIndex++)
                {
                    var stage = model.Stages[stageIndex];
                    var cell = sheet.Cell(row, 3 + stageIndex);
                    var text = coverage.CellText(stage);

                    cell.Value = text;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                var coveredCell = sheet.Cell(row, 3 + model.Stages.Count);
                coveredCell.Value = coverage.VandVItems.Any() ? "Yes" : "No";
                coveredCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                if (!coverage.VandVItems.Any())
                {
                    sheet.Row(row).Style.Font.FontColor = XLColor.Red;
                }

                row++;
            }

            Finish(sheet, headers.Count);
        }

        /// <summary>
        /// Builds the Execution Records sheet, only the activities that have been executed.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="model">The coverage model.</param>
        private static void ConstructExecutionSheet(XLWorkbook workbook, VandVCoverageModel model)
        {
            var sheet = workbook.Worksheets.Add("Execution Records");

            var headers = new[] { "V&V Item", "Requirement", "Method", "Stage", "Status", "Actual Date", "Result", "Evidence" };
            WriteHeader(sheet, headers);

            var row = 2;

            foreach (var coverage in model.Coverages)
            {
                foreach (var item in coverage.VandVItems)
                {
                    var status = VandVCoverageQuery.Attribute(item, "vnv_status");

                    if (string.IsNullOrWhiteSpace(status) || VandVCoverageQuery.AreSameEnumValue(status, "Planned") || VandVCoverageQuery.AreSameEnumValue(status, "Ready"))
                    {
                        continue;
                    }

                    sheet.Cell(row, 1).Value = item.ShortName;
                    sheet.Cell(row, 2).Value = coverage.Requirement.ShortName;
                    sheet.Cell(row, 3).Value = VandVCoverageQuery.Attribute(item, "vnv_method");
                    sheet.Cell(row, 4).Value = VandVCoverageQuery.Attribute(item, "vnv_stage");
                    sheet.Cell(row, 5).Value = status;
                    sheet.Cell(row, 6).Value = VandVCoverageQuery.Attribute(item, "vnv_actual_date");
                    sheet.Cell(row, 7).Value = VandVCoverageQuery.Attribute(item, "vnv_result");
                    sheet.Cell(row, 8).Value = VandVCoverageQuery.Attribute(item, "vnv_evidence_ref");
                    row++;
                }
            }

            Finish(sheet, headers.Length);
        }

        /// <summary>
        /// Builds the Procedures sheet, one row per procedure step, so the as-run procedure ECSS-E-ST-10-03 expects
        /// of a test report leaves the tool with the rest of the deliverable.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="iteration">The iteration.</param>
        /// <param name="model">The coverage model.</param>
        private static void ConstructProcedureSheet(XLWorkbook workbook, Iteration iteration, VandVCoverageModel model)
        {
            var sheet = workbook.Worksheets.Add("Procedures");

            var headers = new[]
            {
                "V&V Item", "V&V Item Name", "Requirement", "Procedure Ref.", "Preconditions", "Conditions",
                "Facility", "Step", "Action", "Expected Result", "Actual Result", "Result"
            };

            WriteHeader(sheet, headers);

            var row = 2;

            foreach (var coverage in model.Coverages)
            {
                foreach (var item in coverage.VandVItems)
                {
                    var steps = VandVProcedureWriter.QuerySteps(iteration, item);

                    if (!steps.Any())
                    {
                        continue;
                    }

                    foreach (var step in steps)
                    {
                        var result = VandVCoverageQuery.Attribute(step, "vnv_step_result");

                        sheet.Cell(row, 1).Value = item.ShortName;
                        sheet.Cell(row, 2).Value = item.Name;
                        sheet.Cell(row, 3).Value = coverage.Requirement.ShortName;
                        sheet.Cell(row, 4).Value = VandVCoverageQuery.Attribute(item, "vnv_procedure_ref");
                        sheet.Cell(row, 5).Value = VandVCoverageQuery.Attribute(item, "vnv_preconditions");
                        sheet.Cell(row, 6).Value = VandVCoverageQuery.Attribute(item, "vnv_conditions");
                        sheet.Cell(row, 7).Value = VandVCoverageQuery.Attribute(item, "vnv_facility");
                        sheet.Cell(row, 8).Value = VandVProcedureWriter.QueryStepNumber(step);
                        sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(step, "vnv_step_action");
                        sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(step, "vnv_step_expected");
                        sheet.Cell(row, 11).Value = VandVCoverageQuery.Attribute(step, "vnv_step_actual");
                        sheet.Cell(row, 12).Value = result;

                        if (VandVCoverageQuery.AreSameEnumValue(result, "Fail"))
                        {
                            sheet.Row(row).Style.Font.FontColor = XLColor.Red;
                        }

                        row++;
                    }
                }
            }

            Finish(sheet, headers.Length);
        }

        /// <summary>
        /// Builds the NCR sheet, every review request raised in the model: Review Item Discrepancies, Requests for
        /// Deviation and Requests for Waiver. This is the "Restpuntenlijst" / open-points list, so it leads with what
        /// kind of request it is and what it concerns, and only then with the administrative detail.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="iteration">The iteration.</param>
        private static void ConstructNcrSheet(XLWorkbook workbook, Iteration iteration)
        {
            var sheet = workbook.Worksheets.Add("NCRs");

            var headers = new[]
            {
                "Type", "ID", "Open?", "Status", "V&V Item", "Requirement", "Title", "Classification", "Owner",
                "Created", "Content", "Replies", "Solutions"
            };

            WriteHeader(sheet, headers);

            var row = 2;

            foreach (var request in AnnotationQuery.Query(iteration))
            {
                var annotation = request.Annotation;

                sheet.Cell(row, 1).Value = AnnotationKind.Describe(annotation);
                sheet.Cell(row, 2).Value = annotation.ShortName;
                sheet.Cell(row, 3).Value = AnnotationQuery.IsOpen(annotation) ? "OPEN" : "closed";
                sheet.Cell(row, 4).Value = annotation.Status.ToString();
                sheet.Cell(row, 5).Value = request.VandVItem?.ShortName;
                sheet.Cell(row, 6).Value = request.Requirement?.ShortName;
                sheet.Cell(row, 7).Value = annotation.Title;
                sheet.Cell(row, 8).Value = annotation.Classification.ToString();
                sheet.Cell(row, 9).Value = annotation.Owner?.ShortName;
                sheet.Cell(row, 10).Value = annotation.CreatedOn.ToString("yyyy-MM-dd");
                sheet.Cell(row, 11).Value = annotation.Content;
                sheet.Cell(row, 12).Value = annotation.Discussion.Count;
                sheet.Cell(row, 13).Value = (annotation as ReviewItemDiscrepancy)?.Solution.Count ?? 0;

                row++;
            }

            Finish(sheet, headers.Length);
        }

        /// <summary>
        /// Reads the requirement text, that is, the first definition the requirement carries.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The requirement text, or an empty string.</returns>
        private static string QueryRequirementText(Requirement requirement)
        {
            return requirement.Definition.FirstOrDefault()?.Content ?? string.Empty;
        }

        /// <summary>
        /// Names the requirements this one derives from, resolved through the model's own requirement-to-requirement
        /// traceability relationships. ECSS-E-ST-10-02 Annex B requires the VCD to show that traceability.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The parent requirement short-names, comma separated.</returns>
        private static string QueryParentRequirements(Iteration iteration, Requirement requirement)
        {
            var parents = iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(relationship => relationship.Source == requirement && !VandVCoverageQuery.IsCoverageLink(relationship))
                .Select(relationship => relationship.Target)
                .OfType<Requirement>()
                .Where(target => !VandVCoverageQuery.IsVnVItem(target))
                .Select(target => target.ShortName)
                .Distinct()
                .OrderBy(shortName => shortName);

            return string.Join(", ", parents);
        }

        /// <summary>
        /// Writes and styles the header row.
        /// </summary>
        /// <param name="sheet">The worksheet.</param>
        /// <param name="headers">The header captions.</param>
        private static void WriteHeader(IXLWorksheet sheet, IReadOnlyList<string> headers)
        {
            for (var column = 0; column < headers.Count; column++)
            {
                sheet.Cell(1, column + 1).Value = headers[column];
            }

            var headerRange = sheet.Range(1, 1, 1, headers.Count);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        /// <summary>
        /// Freezes the header, adds an auto-filter and sizes the columns.
        /// </summary>
        /// <param name="sheet">The worksheet.</param>
        /// <param name="columnCount">The number of columns written.</param>
        private static void Finish(IXLWorksheet sheet, int columnCount)
        {
            sheet.SheetView.FreezeRows(1);
            sheet.Range(1, 1, 1, columnCount).SetAutoFilter();
            sheet.Columns(1, columnCount).AdjustToContents(1, 60d, 60d);
        }
    }
}
