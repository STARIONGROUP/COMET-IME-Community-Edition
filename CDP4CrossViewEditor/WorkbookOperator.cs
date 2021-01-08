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
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using CDP4CrossViewEditor.Generator;

    using CDP4Dal;

    using CDP4OfficeInfrastructure.OfficeDal;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="WorkbookOperator"/> is to delegate the operations on a <see cref="Workbook"/>
    /// to the proper classes
    /// </summary>
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
        /// <param name="dialogNavigationService">
        /// The instance of <see cref="IDialogNavigationService"/> that orchestrates navigation to dialogs
        /// </param>
        public WorkbookOperator(Application application, Workbook workbook, IDialogNavigationService dialogNavigationService)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application), "the Excel application may not be null");
            }

            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook), "the workbook may not be null");
            }

            workbook.Activate();

            this.workbook = workbook;
            this.application = application;
            this.DialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to navigate to dialogs
        /// </summary>
        private IDialogNavigationService DialogNavigationService { get; set; }

        /// <summary>
        /// Rebuild the Parameter Sheet
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
        /// <param name="elementDefinitions"></param>
        /// <param name="parameterTypes"></param>
        public async Task Rebuild(ISession session, Iteration iteration, Participant participant,
            IEnumerable<ElementDefinition> elementDefinitions,
            IEnumerable<ParameterType> parameterTypes)
        {
            this.application.StatusBar = string.Empty;

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                await this.RefreshSessionData(session);

                this.WriteCrossviewSheet(session, iteration, participant, elementDefinitions, parameterTypes);

                this.WriteSessionInfoToWorkbook(session, iteration, participant);

                this.WriteWorkbookDataToWorkbook(iteration);

                this.ActivateCrossviewEditorSheet();

                this.application.StatusBar = $"Rebuild Crossview completed in [{sw.ElapsedMilliseconds}] ms";
            }
            catch (Exception ex)
            {
                this.application.StatusBar = "Rebuild Crossview sheet failed";
                Logger.Error(ex);
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
        /// <param name="elementDefinitions"></param>
        /// <param name="parameterTypes"></param>
        private void WriteCrossviewSheet(ISession session, Iteration iteration, Participant participant,
            IEnumerable<ElementDefinition> elementDefinitions,
            IEnumerable<ParameterType> parameterTypes)
        {
            var crossviewSheetGenerator = new CrossviewSheetGenerator(session, iteration, participant);
            crossviewSheetGenerator.Rebuild(this.application, this.workbook, elementDefinitions, parameterTypes);
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
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is being stored in the workbook
        /// </param>
        private void WriteWorkbookDataToWorkbook(Iteration iteration)
        {
            var workbookData = new WorkbookData(iteration);
            var workbookDataDal = new WorkbookDataDal(this.workbook);
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
                worksheet.Cells[1, 1].Select();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
