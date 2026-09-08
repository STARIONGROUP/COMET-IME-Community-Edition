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

namespace CDP4Requirements.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Requirements.Rdl;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    using ClosedXML.Excel;

    /// <summary>
    /// Exports the V&amp;V deliverables of an <see cref="Iteration"/> to a single Excel workbook: the <b>VCD</b>
    /// register (one row per V&amp;V activity), the <b>VCRM</b> coverage matrix (requirements × stage gates), an
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
                ConstructActivitiesSheet(workbook, iteration);
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

            var headers = new[]
            {
                "Requirement", "Requirement Name", "Requirement Text", "Parent Requirement", "V&V Item",
                "V&V Item Name", "Method", "Stage", "Level", "Criticality", "Owner", "Plan Ref.", "Activity No.",
                "Planned", "Actual", "Status", "Compliance", "Requirement Closure", "Closed", "Close-out Reason",
                "Closed By", "Closed On", "Acceptance Criteria", "Conditions", "Facility", "Result", "Evidence",
                "Analysis Check"
            };

            WriteHeader(sheet, headers);

            var row = 2;

            var parentsByRequirement = QueryParentRequirementMap(iteration);

            foreach (var coverage in model.Coverages)
            {
                var requirementText = QueryRequirementText(coverage.Requirement);
                var parent = parentsByRequirement.TryGetValue(coverage.Requirement.Iid, out var parents) ? parents : string.Empty;

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
                    var performingActivity = model.ActivityByItem.TryGetValue(item.Iid, out var activity) ? activity : null;

                    sheet.Cell(row, 1).Value = coverage.Requirement.ShortName;
                    sheet.Cell(row, 2).Value = coverage.Requirement.Name;
                    sheet.Cell(row, 3).Value = requirementText;
                    sheet.Cell(row, 4).Value = parent;
                    sheet.Cell(row, 5).Value = item.ShortName;
                    sheet.Cell(row, 6).Value = item.Name;
                    sheet.Cell(row, 7).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Method);
                    sheet.Cell(row, 8).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Stage);
                    sheet.Cell(row, 9).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Level);
                    sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(item, VandVParameter.Criticality);
                    sheet.Cell(row, 11).Value = item.Owner?.ShortName;
                    sheet.Cell(row, 12).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVCloseOut.PlanReferenceShortName);
                    sheet.Cell(row, 13).Value = performingActivity?.ShortName ?? VandVCoverageQuery.Attribute(item, VandVParameter.ActivityNumber);
                    sheet.Cell(row, 14).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.PlannedDate);
                    sheet.Cell(row, 15).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.ActualDate);
                    sheet.Cell(row, 16).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Status);
                    sheet.Cell(row, 17).Value = compliance;
                    sheet.Cell(row, 18).Value = VandVStageGateQuery.QueryClosure(item);
                    sheet.Cell(row, 19).Value = VandVCloseOut.IsClosed(item) ? "Closed" : "Open";
                    sheet.Cell(row, 20).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.CloseOutReasonShortName);
                    sheet.Cell(row, 21).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.ClosedByShortName);
                    sheet.Cell(row, 22).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.ClosedOnShortName);
                    sheet.Cell(row, 23).Value = VandVCoverageQuery.Attribute(item, VandVParameter.AcceptanceCriteria);
                    sheet.Cell(row, 24).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Conditions);
                    sheet.Cell(row, 25).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Facility);
                    sheet.Cell(row, 26).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Result);
                    sheet.Cell(row, 27).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.EvidenceReference);
                    sheet.Cell(row, 28).Value = analysis.Display;

                    if (analysis.State == VandVAnalysisState.Violated)
                    {
                        sheet.Cell(row, 28).Style.Font.FontColor = XLColor.Red;
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
        /// Builds the VCRM sheet, requirements down, stage gates across. Each cell carries the state of the requirement
        /// at that gate, and the last column the single verdict that answers "is this requirement verified and
        /// validated", so a stage gate review can be run off the sheet and the undefined ones cannot hide.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="model">The coverage model.</param>
        private static void ConstructRvmSheet(XLWorkbook workbook, VandVCoverageModel model)
        {
            var sheet = workbook.Worksheets.Add("VCRM");
            var review = VandVStageGateQuery.Build(model);

            var headers = new List<string> { "Requirement", "Requirement Name" };
            headers.AddRange(model.Stages);
            headers.Add("V&V Status");

            WriteHeader(sheet, headers.ToArray());

            var row = 2;

            foreach (var gateRow in review.Rows)
            {
                sheet.Cell(row, 1).Value = gateRow.Requirement.ShortName;
                sheet.Cell(row, 2).Value = gateRow.Requirement.Name;

                for (var stageIndex = 0; stageIndex < gateRow.Cells.Count; stageIndex++)
                {
                    var gateCell = gateRow.Cells[stageIndex];
                    var cell = sheet.Cell(row, 3 + stageIndex);

                    cell.Value = gateCell.QueryText();
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    Colour(cell, gateCell.State);
                }

                var verdictCell = sheet.Cell(row, 3 + model.Stages.Count);
                verdictCell.Value = gateRow.VerdictText;
                Colour(verdictCell, gateRow.Verdict);

                row++;
            }

            Finish(sheet, headers.Count);
        }

        /// <summary>
        /// Colours a stage gate cell so the sheet can be read across a meeting room table.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="state">The state it carries.</param>
        private static void Colour(IXLCell cell, VandVGateState state)
        {
            switch (state)
            {
                case VandVGateState.Undefined:
                    cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    cell.Style.Font.FontColor = XLColor.Red;
                    cell.Style.Font.Bold = true;
                    break;
                case VandVGateState.Failed:
                    cell.Style.Fill.BackgroundColor = XLColor.MistyRose;
                    cell.Style.Font.FontColor = XLColor.Red;
                    cell.Style.Font.Bold = true;
                    break;
                case VandVGateState.ClosedOut:
                    cell.Style.Fill.BackgroundColor = XLColor.Honeydew;
                    break;
                case VandVGateState.Verified:
                    cell.Style.Fill.BackgroundColor = XLColor.LightCyan;
                    break;
                default:
                    cell.Style.Font.FontColor = XLColor.Gray;
                    break;
            }
        }

        /// <summary>
        /// Writes one workbook for a single report (deliverable): the activities recorded in it, their procedures
        /// step by step, and the requirements those activities cover. A report is a document somebody hands over, so
        /// it can leave the tool as one file rather than as a slice of the whole register.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="report">The report specification to export.</param>
        /// <param name="path">The full path of the <c>.xlsx</c> file to write.</param>
        public void ExportReport(Iteration iteration, RequirementsSpecification report, string path)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A destination path must be supplied.", nameof(path));
            }

            var activities = VandVActivityQuery.QueryActivities(iteration)
                .Where(activity => activity.Container == report)
                .ToList();

            var itemsByActivity = VandVActivityQuery.QueryPerformedItemsMap(iteration);
            var stepsByOwner = VandVProcedureWriter.QueryStepsMap(iteration);
            var coveredByItem = VandVItemCreator.QueryCoveringMap(iteration);

            using (var workbook = new XLWorkbook())
            {
                ConstructReportActivitySheet(workbook, report, activities, itemsByActivity);
                ConstructActivityProcedureSheet(workbook, activities, stepsByOwner);
                ConstructReportCoverageSheet(workbook, iteration, activities, itemsByActivity, coveredByItem);

                workbook.SaveAs(path);
            }
        }

        /// <summary>
        /// Builds the Activities sheet of a per-report workbook.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="report">The report being exported.</param>
        /// <param name="activities">The activities recorded in the report.</param>
        /// <param name="itemsByActivity">The performed items per activity, resolved once for the workbook.</param>
        private static void ConstructReportActivitySheet(XLWorkbook workbook, RequirementsSpecification report, IReadOnlyList<Requirement> activities, IReadOnlyDictionary<Guid, IReadOnlyList<Requirement>> itemsByActivity)
        {
            var sheet = workbook.Worksheets.Add("Activities");

            sheet.Cell(1, 1).Value = $"{report.ShortName}: {report.Name}";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 13;

            var headers = new[]
            {
                "Activity No.", "Name", "Method", "Stage", "Level", "Status", "Planned", "Actual", "Result",
                "Evidence", "Facility", "Description", "Items"
            };

            for (var column = 0; column < headers.Length; column++)
            {
                sheet.Cell(3, column + 1).Value = headers[column];
            }

            var headerRange = sheet.Range(3, 1, 3, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            var row = 4;

            foreach (var activity in activities)
            {
                sheet.Cell(row, 1).Value = activity.ShortName;
                sheet.Cell(row, 2).Value = activity.Name;
                sheet.Cell(row, 3).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Method);
                sheet.Cell(row, 4).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Stage);
                sheet.Cell(row, 5).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Level);
                sheet.Cell(row, 6).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Status);
                sheet.Cell(row, 7).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.PlannedDate);
                sheet.Cell(row, 8).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.ActualDate);
                sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Result);
                sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.EvidenceReference);
                sheet.Cell(row, 11).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Facility);
                sheet.Cell(row, 12).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Description);
                sheet.Cell(row, 13).Value = itemsByActivity.TryGetValue(activity.Iid, out var performedItems) ? performedItems.Count : 0;

                row++;
            }

            sheet.SheetView.FreezeRows(3);
            sheet.Columns(1, headers.Length).AdjustToContents(1, 60d, 60d);
        }

        /// <summary>
        /// Builds the Procedures sheet of a per-report workbook: one row per step of every activity in the report,
        /// which is the as-run record the report itself has to carry.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="activities">The activities recorded in the report.</param>
        /// <param name="stepsByOwner">The procedure steps per owner, resolved once for the workbook.</param>
        private static void ConstructActivityProcedureSheet(XLWorkbook workbook, IReadOnlyList<Requirement> activities, IReadOnlyDictionary<Guid, IReadOnlyList<Requirement>> stepsByOwner)
        {
            var sheet = workbook.Worksheets.Add("Procedures");

            var headers = new[]
            {
                "Activity No.", "Activity", "Procedure Ref.", "Preconditions", "Conditions", "Facility", "Step",
                "Action", "Expected Result", "Actual Result", "Result"
            };

            WriteHeader(sheet, headers);

            var row = 2;

            foreach (var activity in activities)
            {
                if (!stepsByOwner.TryGetValue(activity.Iid, out var activitySteps))
                {
                    continue;
                }

                foreach (var step in activitySteps)
                {
                    var result = VandVCoverageQuery.Attribute(step, VandVParameter.StepResult);

                    sheet.Cell(row, 1).Value = activity.ShortName;
                    sheet.Cell(row, 2).Value = activity.Name;
                    sheet.Cell(row, 3).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.ProcedureReference);
                    sheet.Cell(row, 4).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Preconditions);
                    sheet.Cell(row, 5).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Conditions);
                    sheet.Cell(row, 6).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Facility);
                    sheet.Cell(row, 7).Value = VandVProcedureWriter.QueryStepNumber(step);
                    sheet.Cell(row, 8).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepAction);
                    sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepExpectedResult);
                    sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepActualResult);
                    sheet.Cell(row, 11).Value = result;

                    if (VandVCoverageQuery.AreSameEnumValue(result, VandVStepResult.Fail))
                    {
                        sheet.Row(row).Style.Font.FontColor = XLColor.Red;
                    }

                    row++;
                }
            }

            Finish(sheet, headers.Length);
        }

        /// <summary>
        /// Builds the Coverage sheet of a per-report workbook: which requirement each activity in the report verifies,
        /// through which V&amp;V item, and where that item stands.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="iteration">The iteration.</param>
        /// <param name="activities">The activities recorded in the report.</param>
        /// <param name="itemsByActivity">The performed items per activity, resolved once for the workbook.</param>
        /// <param name="coveredByItem">The covered requirement per item, resolved once for the workbook.</param>
        private static void ConstructReportCoverageSheet(XLWorkbook workbook, Iteration iteration, IReadOnlyList<Requirement> activities, IReadOnlyDictionary<Guid, IReadOnlyList<Requirement>> itemsByActivity, IReadOnlyDictionary<Guid, Requirement> coveredByItem)
        {
            var sheet = workbook.Worksheets.Add("Coverage");

            var headers = new[]
            {
                "Activity No.", "V&V Item", "Requirement", "Requirement Name", "Acceptance Criteria", "Status",
                "Compliance", "Closed", "Close-out Reason", "Analysis Check"
            };

            WriteHeader(sheet, headers);

            var row = 2;

            foreach (var activity in activities)
            {
                if (!itemsByActivity.TryGetValue(activity.Iid, out var performedItems))
                {
                    continue;
                }

                foreach (var item in performedItems)
                {
                    var covered = coveredByItem.TryGetValue(item.Iid, out var coveredRequirement) ? coveredRequirement : null;
                    var analysis = VandVAnalysisChecker.Check(iteration, item);
                    var compliance = VandVCloseOut.QueryCompliance(item);

                    sheet.Cell(row, 1).Value = activity.ShortName;
                    sheet.Cell(row, 2).Value = item.ShortName;
                    sheet.Cell(row, 3).Value = covered?.ShortName;
                    sheet.Cell(row, 4).Value = covered?.Name;
                    sheet.Cell(row, 5).Value = VandVCoverageQuery.Attribute(item, VandVParameter.AcceptanceCriteria);
                    sheet.Cell(row, 6).Value = VandVActivityQuery.EffectiveAttribute(item, activity, VandVParameter.Status);
                    sheet.Cell(row, 7).Value = compliance;
                    sheet.Cell(row, 8).Value = VandVCloseOut.IsClosed(item) ? "Closed" : "Open";
                    sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(item, VandVCloseOut.CloseOutReasonShortName);
                    sheet.Cell(row, 10).Value = analysis.Display;

                    if (VandVCloseOut.IsShortfall(compliance))
                    {
                        sheet.Cell(row, 7).Style.Font.FontColor = XLColor.Red;
                    }

                    row++;
                }
            }

            Finish(sheet, headers.Length);
        }

        /// <summary>
        /// Builds the Activities sheet, one row per shared activity: the task, the report it is filed under, its
        /// planning and execution record, and the items and requirements it performs. This is the task-sheet view
        /// ECSS-E-ST-10-02 keeps alongside the per-requirement VCD.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="iteration">The iteration.</param>
        private static void ConstructActivitiesSheet(XLWorkbook workbook, Iteration iteration)
        {
            var sheet = workbook.Worksheets.Add("Activities");

            var headers = new[]
            {
                "Activity No.", "Name", "Report", "Method", "Stage", "Level", "Status", "Planned", "Actual", "Result",
                "Evidence", "Facility", "Items", "Requirements"
            };

            WriteHeader(sheet, headers);

            var row = 2;

            var itemsByActivity = VandVActivityQuery.QueryPerformedItemsMap(iteration);
            var coveredByItem = VandVItemCreator.QueryCoveringMap(iteration);

            foreach (var activity in VandVActivityQuery.QueryActivities(iteration))
            {
                var items = itemsByActivity.TryGetValue(activity.Iid, out var performedItems) ? performedItems : new List<Requirement>();

                var requirements = items
                    .Select(item => coveredByItem.TryGetValue(item.Iid, out var covered) ? covered.ShortName : null)
                    .Where(shortName => !string.IsNullOrWhiteSpace(shortName))
                    .Distinct()
                    .OrderBy(shortName => shortName);

                sheet.Cell(row, 1).Value = activity.ShortName;
                sheet.Cell(row, 2).Value = activity.Name;
                sheet.Cell(row, 3).Value = VandVActivityQuery.QueryReport(activity)?.ShortName;
                sheet.Cell(row, 4).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Method);
                sheet.Cell(row, 5).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Stage);
                sheet.Cell(row, 6).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Level);
                sheet.Cell(row, 7).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Status);
                sheet.Cell(row, 8).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.PlannedDate);
                sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.ActualDate);
                sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Result);
                sheet.Cell(row, 11).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.EvidenceReference);
                sheet.Cell(row, 12).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Facility);
                sheet.Cell(row, 13).Value = items.Count;
                sheet.Cell(row, 14).Value = string.Join(", ", requirements);

                row++;
            }

            Finish(sheet, headers.Length);
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
                    var performingActivity = model.ActivityByItem.TryGetValue(item.Iid, out var activity) ? activity : null;
                    var status = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Status);

                    if (string.IsNullOrWhiteSpace(status) || VandVCoverageQuery.AreSameEnumValue(status, VandVStatus.Planned) || VandVCoverageQuery.AreSameEnumValue(status, VandVStatus.Ready))
                    {
                        continue;
                    }

                    sheet.Cell(row, 1).Value = item.ShortName;
                    sheet.Cell(row, 2).Value = coverage.Requirement.ShortName;
                    sheet.Cell(row, 3).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Method);
                    sheet.Cell(row, 4).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Stage);
                    sheet.Cell(row, 5).Value = status;
                    sheet.Cell(row, 6).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.ActualDate);
                    sheet.Cell(row, 7).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Result);
                    sheet.Cell(row, 8).Value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.EvidenceReference);
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

            var stepsByOwner = VandVProcedureWriter.QueryStepsMap(iteration);

            foreach (var activity in VandVActivityQuery.QueryActivities(iteration))
            {
                if (!stepsByOwner.TryGetValue(activity.Iid, out var activitySteps))
                {
                    continue;
                }

                foreach (var step in activitySteps)
                {
                    var stepResult = VandVCoverageQuery.Attribute(step, VandVParameter.StepResult);

                    sheet.Cell(row, 1).Value = activity.ShortName;
                    sheet.Cell(row, 2).Value = activity.Name;
                    sheet.Cell(row, 3).Value = "(activity)";
                    sheet.Cell(row, 4).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.ProcedureReference);
                    sheet.Cell(row, 5).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Preconditions);
                    sheet.Cell(row, 6).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Conditions);
                    sheet.Cell(row, 7).Value = VandVCoverageQuery.Attribute(activity, VandVParameter.Facility);
                    sheet.Cell(row, 8).Value = VandVProcedureWriter.QueryStepNumber(step);
                    sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepAction);
                    sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepExpectedResult);
                    sheet.Cell(row, 11).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepActualResult);
                    sheet.Cell(row, 12).Value = stepResult;

                    if (VandVCoverageQuery.AreSameEnumValue(stepResult, VandVStepResult.Fail))
                    {
                        sheet.Row(row).Style.Font.FontColor = XLColor.Red;
                    }

                    row++;
                }
            }

            foreach (var coverage in model.Coverages)
            {
                foreach (var item in coverage.VandVItems)
                {
                    var steps = stepsByOwner.TryGetValue(item.Iid, out var itemSteps) ? itemSteps : new List<Requirement>();

                    if (!steps.Any())
                    {
                        continue;
                    }

                    foreach (var step in steps)
                    {
                        var result = VandVCoverageQuery.Attribute(step, VandVParameter.StepResult);

                        sheet.Cell(row, 1).Value = item.ShortName;
                        sheet.Cell(row, 2).Value = item.Name;
                        sheet.Cell(row, 3).Value = coverage.Requirement.ShortName;
                        sheet.Cell(row, 4).Value = VandVCoverageQuery.Attribute(item, VandVParameter.ProcedureReference);
                        sheet.Cell(row, 5).Value = VandVCoverageQuery.Attribute(item, VandVParameter.Preconditions);
                        sheet.Cell(row, 6).Value = VandVCoverageQuery.Attribute(item, VandVParameter.Conditions);
                        sheet.Cell(row, 7).Value = VandVCoverageQuery.Attribute(item, VandVParameter.Facility);
                        sheet.Cell(row, 8).Value = VandVProcedureWriter.QueryStepNumber(step);
                        sheet.Cell(row, 9).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepAction);
                        sheet.Cell(row, 10).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepExpectedResult);
                        sheet.Cell(row, 11).Value = VandVCoverageQuery.Attribute(step, VandVParameter.StepActualResult);
                        sheet.Cell(row, 12).Value = result;

                        if (VandVCoverageQuery.AreSameEnumValue(result, VandVStepResult.Fail))
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
        /// Names, per requirement, the requirements it derives from, resolved through the model's own
        /// requirement-to-requirement traceability relationships. ECSS-E-ST-10-02 Annex B requires the VCD to show
        /// that traceability.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <returns>The parent requirement short-names, comma separated, per requirement.</returns>
        /// <remarks>
        /// Built in one pass and looked up per row: resolving it per requirement walked the whole relationship list
        /// once for every requirement in the model, which on a ten-thousand-requirement model is what an export
        /// spent its time on.
        /// </remarks>
        private static IReadOnlyDictionary<Guid, string> QueryParentRequirementMap(Iteration iteration)
        {
            var parentsBySource = new Dictionary<Guid, SortedSet<string>>();

            foreach (var relationship in iteration.Relationship.OfType<BinaryRelationship>())
            {
                if (relationship.Source is Requirement source
                    && relationship.Target is Requirement target
                    && !VandVCoverageQuery.IsCoverageLink(relationship)
                    && !VandVCoverageQuery.IsVnVItem(target))
                {
                    if (!parentsBySource.TryGetValue(source.Iid, out var parents))
                    {
                        parents = new SortedSet<string>(StringComparer.Ordinal);
                        parentsBySource.Add(source.Iid, parents);
                    }

                    parents.Add(target.ShortName);
                }
            }

            return parentsBySource.ToDictionary(pair => pair.Key, pair => string.Join(", ", pair.Value));
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
