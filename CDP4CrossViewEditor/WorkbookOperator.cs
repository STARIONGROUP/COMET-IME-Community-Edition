// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookOperator.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Generator;

    using CDP4Dal;

    using CDP4OfficeInfrastructure.OfficeDal;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NetOffice.Exceptions;

    using NLog;

    /// <summary>
    /// Workbook metadata that will be preserved between two running sessions
    /// </summary>
    public struct WorkbookMetadata
    {
        /// <summary>
        /// Element definitions user selection
        /// </summary>
        public IEnumerable<Guid> ElementDefinitions;

        /// <summary>
        /// Parameter types user selection
        /// </summary>
        public IEnumerable<Guid> ParameterTypes;

        /// <summary>
        /// Parameter sheet values changed by the user
        /// </summary>
        public Dictionary<string, string> ParameterValues;

        /// <summary>
        /// Flag that indicates value persistence
        /// </summary>
        public bool PersistValues;
    }

    /// <summary>
    /// The purpose of the <see cref="WorkbookOperator"/> is to delegate the operations on a <see cref="Workbook"/>
    /// to the proper classes
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WorkbookOperator
    {
        /// <summary>
        /// The NLog Logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Workbook"/> that is being managed by the current <see cref="WorkbookOperator"/>
        /// </summary>
        private readonly Workbook workbook;

        /// <summary>
        /// The <see cref="WorkbookMetadata"/> that is being managed by the current <see cref="WorkbookOperator"/>
        /// </summary>
        private readonly WorkbookMetadata workbookMetadata;

        /// <summary>
        /// The excel <see cref="Application"/> that is being managed by the current <see cref="WorkbookOperator"/>
        /// </summary>
        private readonly Application application;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookOperator"/> class.
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/> that is being operated on.
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is being operated on.
        /// </param>
        /// <param name="workbookMetadata">
        /// The <see cref="WorkbookMetadata"/> that was saved.
        /// </param>
        public WorkbookOperator(Application application, Workbook workbook, WorkbookMetadata workbookMetadata)
        {
            if (application == null)
            {
                Logger.Error("The Excel application may not be null");
                return;
            }

            if (workbook == null)
            {
                Logger.Error("The workbook may not be null");
                return;
            }

            workbook.Activate();

            this.application = application;
            this.workbook = workbook;
            this.workbookMetadata = workbookMetadata;
        }

        /// <summary>
        /// Rebuild the Crossview Sheet
        /// </summary>
        /// <param name="session">
        /// The current <see cref="ISession"/> that is rebuilding the parameter sheet
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains elements and parameters that are being written to the workbook
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/> for which the workbook is being rebuilt.
        /// </param>
        public async Task Rebuild(ISession session, Iteration iteration, Participant participant)
        {
            this.application.StatusBar = string.Empty;

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                await this.RefreshSessionData(session);

                this.WriteCrossviewSheet(session, iteration, participant);

                this.WriteSessionInfoToWorkbook(session, iteration, participant);

                this.WriteWorkbookDataToWorkbook();

                this.ActivateCrossviewEditorSheet();

                this.application.StatusBar = $"Rebuild Crossview completed in [{sw.ElapsedMilliseconds}] ms";
            }
            catch (Exception ex)
            {
                this.application.StatusBar = "Rebuild Crossview sheet failed";
                throw ex;
            }
            finally
            {
                this.application.Cursor = XlMousePointer.xlDefault;
            }
        }

        /// <summary>
        /// Refresh the data in cache with the latest data from the data-source
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the data-source
        /// </param>
        private async Task RefreshSessionData(ISession session)
        {
            var sw = new Stopwatch();
            sw.Start();

            this.application.Cursor = XlMousePointer.xlWait;
            this.application.StatusBar = $"CDP4: refreshing data from the data source {session.DataSourceUri}";

            await session.Refresh();

            this.application.StatusBar = $"CDP4: data refreshed in {sw.ElapsedMilliseconds} [ms]";
            this.application.Cursor = XlMousePointer.xlDefault;
        }

        /// <summary>
        /// Write the parameter sheet to the workbook
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> related with the the workbook.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains elements and parameters that are being written to the workbook
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/> for which the workbook is being rebuilt.
        /// </param>
        private void WriteCrossviewSheet(ISession session, Iteration iteration, Participant participant)
        {
            var crossviewSheetGenerator = new CrossviewSheetGenerator(session, iteration, participant);
            crossviewSheetGenerator.Rebuild(this.application, this.workbook, this.workbookMetadata);
        }

        /// <summary>
        /// Write session information to the workbook as a custom XML part
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> related with the the workbook.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is written to the workbook
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is written to the workbook
        /// </param>
        private void WriteSessionInfoToWorkbook(ISession session, Iteration iteration, Participant participant)
        {
            var engineeringModel = (EngineeringModel)iteration.Container;

            var selectedDomainOfExpertise = session.QuerySelectedDomainOfExpertise(iteration);

            var workbookSession = new WorkbookSession(participant.Person, engineeringModel.EngineeringModelSetup, iteration.IterationSetup, selectedDomainOfExpertise);
            var workbookSessionDal = new WorkbookSessionDal(this.workbook);
            workbookSessionDal.Write(workbookSession);
        }

        /// <summary>
        /// Write the <see cref="Iteration"/> data to the workbook as a custom XML part
        /// </summary>
        private void WriteWorkbookDataToWorkbook()
        {
            var workbookData = new CrossviewWorkbookData(
                this.workbookMetadata.ElementDefinitions,
                this.workbookMetadata.ParameterTypes,
                this.workbookMetadata.PersistValues ? this.workbookMetadata.ParameterValues : new Dictionary<string, string>());

            var workbookDataDal = new CrossviewWorkbookDataDal(this.workbook);
            workbookDataDal.Write(workbookData);
        }

        /// <summary>
        /// Activates the Crossview sheet
        /// </summary>
        private void ActivateCrossviewEditorSheet()
        {
            try
            {
                var worksheet = (Worksheet)this.workbook.Worksheets[CrossviewSheetConstants.CrossviewSheetName];
                worksheet.Activate();
                worksheet.ChangeEvent += this.Worksheet_ChangeEvent;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Store manually filled values on each cell content change
        /// </summary>
        /// <param name="target"> Excel target <see cref="Range"/></param>
        [ExcludeFromCodeCoverage]
        private void Worksheet_ChangeEvent(Range target)
        {
            string cellName;

            try
            {
                cellName = (target.Name as Name)?.Name;
            }
            catch (PropertyGetCOMException ex)
            {
                Logger.Error(ex);
                return;
            }

            if (string.IsNullOrEmpty(cellName))
            {
                return;
            }

            if (this.workbookMetadata.ParameterValues.ContainsKey(cellName))
            {
                this.workbookMetadata.ParameterValues[cellName] = target.Value.ToString();
            }
            else
            {
                this.workbookMetadata.ParameterValues.Add(cellName, target.Value.ToString());
            }

            this.WriteWorkbookDataToWorkbook();
        }
    }
}
