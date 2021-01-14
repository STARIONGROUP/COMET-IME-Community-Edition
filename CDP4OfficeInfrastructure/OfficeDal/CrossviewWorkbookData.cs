// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewWorkbookData.cs" company="RHEA System S.A.">
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

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    using CDP4Common.DTO;

    using NetOffice.ExcelApi;

    /// <summary>
    /// The purpose of the <see cref="CrossviewWorkbookData"/> class is to store session data in a <see cref="Workbook"/>
    /// as a custom XML part, and retrieve it from the workbook.
    /// </summary>
    [XmlRoot("CDP4CrossviewData", Namespace = "http://cdp4crossviewdata.rheagroup.com")]
    public class CrossviewWorkbookData : CustomOfficeData
    {
        /// <summary>
        /// Backing field for the <see cref="ElementDefinitionData"/> property.
        /// </summary>
        [XmlIgnore] private string elementDefinitionData;

        /// <summary>
        /// Backing field for the <see cref="ParameterTypeData"/> property.
        /// </summary>
        [XmlIgnore] private string parameterTypeData;

        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{ElementDefinition}"/> data of the current <see cref="Workbook"/>
        /// </summary>
        [XmlElement(typeof(XmlCDataSection))]
        public XmlCDataSection ElementDefinitionData
        {
            get
            {
                var doc = new XmlDocument();
                return doc.CreateCDataSection(this.elementDefinitionData);
            }

            set => this.elementDefinitionData = value.Value;
        }

        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{ParameterType}"/> data of the current <see cref="Workbook"/>
        /// </summary>
        [XmlElement(typeof(XmlCDataSection))]
        public XmlCDataSection ParameterTypeData
        {
            get
            {
                var doc = new XmlDocument();
                return doc.CreateCDataSection(this.parameterTypeData);
            }

            set => this.parameterTypeData = value.Value;
        }

        /// <summary>
        /// Gets the <see cref="IEnumerable{ElementDefinition}"/> instances.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<Thing> ElementDefinitionList => this.Serializer.Deserialize(this.GenerateStreamFromString(this.ElementDefinitionData.Value));

        /// <summary>
        /// Gets the <see cref="IEnumerable{ParameterType}"/> instances.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<Thing> ParameterTypeList => this.Serializer.Deserialize(this.GenerateStreamFromString(this.ParameterTypeData.Value));

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewWorkbookData"/> class.
        /// </summary>
        public CrossviewWorkbookData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewWorkbookData"/> class.
        /// </summary>
        /// <param name="elementDefinitions">
        /// The element definitions list that needs to be preserved <see cref="IEnumerable{ElementDefinition}" />
        /// </param>
        /// <param name="parameterTypes">
        /// The parameter types list that needs to be preserved <see cref="IEnumerable{ParameterType}" />
        /// </param>
        public CrossviewWorkbookData(IEnumerable<CDP4Common.EngineeringModelData.ElementDefinition> elementDefinitions, IEnumerable<CDP4Common.SiteDirectoryData.ParameterType> parameterTypes)
        {
            var preservedDefinitions = elementDefinitions.Select(elementDefinition => elementDefinition.ToDto() as ElementDefinition).ToList();
            var preservedTypes = parameterTypes.Select(parameterType => parameterType.ToDto() as ParameterType).ToList();

            this.GenerateStringFromThings(preservedDefinitions);
            this.GenerateStringFromThings(preservedTypes);
        }

        /// <summary>
        /// Generate a stream from a list
        /// </summary>
        /// <typeparam name="T"><see cref="ElementDefinition"/> or <see cref="ParameterType"/></typeparam>
        /// <param name="things">List of <see cref="Thing"/>s that will serialized to stream</param>
        private void GenerateStringFromThings<T>(List<T> things)
        {
            using (var memoryStream = new MemoryStream())
            {
                this.Serializer.SerializeToStream(things, memoryStream);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    if (things[0] is ElementDefinition)
                    {
                        this.elementDefinitionData = reader.ReadToEnd();
                    }

                    if (things[0] is ParameterType)
                    {
                        this.parameterTypeData = reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Generate a stream from a string
        /// </summary>
        /// <param name="s">The string input</param>
        /// <returns>The stream</returns>
        private Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
