// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementArrayAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Assemblers
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="RequirementArrayAssembler"/> is to create the arrays that are
    /// used to populate the <see cref="Requirement"/>s Sheet
    /// </summary>
    public class RequirementArrayAssembler
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The nr of columns the <see cref="ContentArray"/> contains, excluding any columns related to <see cref="SimpleParameterValue"/>s
        /// </summary>
        private const int BaseColumns = 7;

        /// <summary>
        /// The list of <see cref="ParameterTypeColumn"/> used to populate the <see cref="ContentArray"/> with <see cref="SimpleParameterValue"/>s
        /// </summary>
        private readonly List<ParameterTypeColumn> parameterTypeColumns = new List<ParameterTypeColumn>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementArrayAssembler"/> class.
        /// </summary>
        /// <param name="requirements">
        /// The list of <see cref="Requirement"/>s that are to be contained in the <see cref="ContentArray"/>
        /// </param>
        public RequirementArrayAssembler(IEnumerable<Requirement> requirements)
        {
            this.PopulateParameterTypes(requirements);
            this.InitializeArrays(requirements);
            this.PopulateRequirementArray(requirements);
        }

        /// <summary>
        /// Gets the array that contains the <see cref="Requirement"/>s sheet information
        /// </summary>
        public object[,] ContentArray { get; private set; }

        /// <summary>
        /// Populates the <see cref="parameterTypeColumns"/> based on the provided <see cref="Requirement"/>s
        /// </summary>
        /// <param name="requirements">
        /// the <see cref="Requirement"/>s that are to be generated
        /// </param>
        private void PopulateParameterTypes(IEnumerable<Requirement> requirements)
        {
            logger.Debug("Setting ParameterType columns");

            var simpleParameterValues = requirements.SelectMany(x => x.ParameterValue).OrderBy(x => x.ParameterType.ShortName);

            var i = BaseColumns;
            foreach (var simpleParameterValue in simpleParameterValues)
            {
                if (!this.parameterTypeColumns.Any(x => x.ParameterType == simpleParameterValue.ParameterType && x.MeasurementScale == simpleParameterValue.Scale))
                {
                    var column = new ParameterTypeColumn(simpleParameterValue.ParameterType, simpleParameterValue.Scale, i);
                    this.parameterTypeColumns.Add(column);
                    i++;
                }
            }
        }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the <see cref="Requirement"/>s sheet 
        /// </summary>
        private void InitializeArrays(IEnumerable<Requirement> requirements)
        {
            logger.Debug("Initializing Arrays");

            var nrofrows = requirements.Count() + 1;

            this.ContentArray = new object[nrofrows, BaseColumns + this.parameterTypeColumns.Count];
        }

        /// <summary>
        /// populate the <see cref="ContentArray"/> with data
        /// </summary>
        private void PopulateRequirementArray(IEnumerable<Requirement> requirements)
        {
            logger.Debug("Populating content array with header info");

            this.ContentArray[0, 0] = "Specification";
            this.ContentArray[0, 1] = "Group";
            this.ContentArray[0, 2] = "Short Name";
            this.ContentArray[0, 3] = "Name";
            this.ContentArray[0, 4] = "Definition";
            this.ContentArray[0, 5] = "Owner";
            this.ContentArray[0, 6] = "Category";

            logger.Debug("Add columns for the different ParameterTypes");
            var i = 1;
            foreach (var parameterType in this.parameterTypeColumns)
            {
                this.ContentArray[0, parameterType.Column] = parameterType.Name();
                i++;
            }

            logger.Debug("Add requirement data to content array");
            int j = 1;
            foreach (var requirement in requirements)
            {
                var requirementsSpecification = (RequirementsSpecification)requirement.Container;
                this.ContentArray[j, 0] = $"{requirementsSpecification.Name} [{requirementsSpecification.ShortName}]"; ;

                this.ContentArray[j, 1] = requirement.Group != null ? $"{requirement.Group.Name} [{requirement.Group.ShortName}]" : "-"; ;
                this.ContentArray[j, 2] = requirement.ShortName;
                this.ContentArray[j, 3] = requirement.Name;

                var definition = requirement.Definition.FirstOrDefault();
                this.ContentArray[j, 4] = definition != null ? definition.Content : string.Empty;
                this.ContentArray[j, 5] = requirement.Owner.ShortName;
                this.ContentArray[j, 6] = requirement.GetAllCategoryShortNames();

                foreach (var simpleParameterValue in requirement.ParameterValue)
                {
                    var parameterTypeColumn = this.parameterTypeColumns.SingleOrDefault(x => x.ParameterType == simpleParameterValue.ParameterType && x.MeasurementScale == simpleParameterValue.Scale);
                    if (parameterTypeColumn != null)
                    {
                        this.ContentArray[j, parameterTypeColumn.Column] = simpleParameterValue.Value[0];
                    }
                }

                j++;
            }
        }

        /// <summary>
        /// The <see cref="ParameterTypeColumn"/> represents the meta-data of a column on the <see cref="Requirement"/> sheet
        /// related to the <see cref="CDP4Common.SiteDirectoryData.ParameterType"/> and <see cref="CDP4Common.SiteDirectoryData.MeasurementScale"/> of <see cref="SimpleParameterValue"/>
        /// </summary>
        private class ParameterTypeColumn
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterTypeColumn"/> class.
            /// </summary>
            /// <param name="parameterType">
            /// The <see cref="CDP4Common.SiteDirectoryData.ParameterType"/> of the <see cref="ParameterTypeColumn"/>
            /// </param>
            /// <param name="measurementScale">
            /// The <see cref="CDP4Common.SiteDirectoryData.MeasurementScale"/> of the <see cref="ParameterTypeColumn"/>
            /// </param>
            /// <param name="column">
            /// the column number of the <see cref="ParameterTypeColumn"/>
            /// </param>
            public ParameterTypeColumn(ParameterType parameterType, MeasurementScale measurementScale, int column)
            {
                this.ParameterType = parameterType;
                this.MeasurementScale = measurementScale;
                this.Column = column;
            }

            /// <summary>
            /// Gets the <see cref="CDP4Common.SiteDirectoryData.ParameterType"/>
            /// </summary>
            public ParameterType ParameterType { get; private set; }

            /// <summary>
            /// Gets the <see cref="CDP4Common.SiteDirectoryData.MeasurementScale"/>
            /// </summary>
            public MeasurementScale MeasurementScale { get; private set; }

            /// <summary>
            /// Gets the column number
            /// </summary>
            public int Column { get; private set; }

            /// <summary>
            /// Computes the derived name
            /// </summary>
            /// <returns></returns>
            public string Name()
            {
                return this.MeasurementScale == null ? this.ParameterType.ShortName : $"{this.ParameterType.ShortName} [{this.MeasurementScale.ShortName}]";
            }
        }
    }
}