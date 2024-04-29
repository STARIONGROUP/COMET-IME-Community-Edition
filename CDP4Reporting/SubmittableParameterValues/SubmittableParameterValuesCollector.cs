// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubmittableParameterValuesCollector.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Reporting.SubmittableParameterValues
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DevExpress.XtraPrinting;
    using DevExpress.XtraReports.UI;

    /// <summary>
    /// The implementation of the injectable interface <see cref="ISubmittableParameterValuesCollector"/> that is used to collect <see cref="SubmittableParameterValue"/>s
    /// from an <see cref="XtraReport"/>
    /// </summary>
    [Export(typeof(ISubmittableParameterValuesCollector))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExcludeFromCodeCoverage]
    public class SubmittableParameterValuesCollector : ISubmittableParameterValuesCollector
    {
        /// <summary>
        /// Collect <see cref="SubmittableParameterValue"/>s from an <see cref="XtraReport"/>
        /// </summary>
        /// <param name="report">
        /// The <see cref="XtraReport"/>
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{SubmittableParameterValue}"/> that contains all the collected <see cref="SubmittableParameterValue"/>s.
        /// </returns>
        public IEnumerable<SubmittableParameterValue> Collect(XtraReport report)
        {
            var submittableParameterValues = new List<SubmittableParameterValue>();

            foreach (Page p in report.Pages)
            {
                var iterator = new DevExpress.XtraPrinting.Native.NestedBrickIterator(p.InnerBricks);

                while (iterator.MoveNext())
                {
                    if (!(iterator.CurrentBrick is VisualBrick visualBrick))
                    {
                        continue;
                    }

                    if (!(visualBrick.BrickOwner is XRControl control))
                    {
                        continue;
                    }

                    string path = null;
                    bool isExactOptionPath = false;

                    if (SubmittableParameterValue.TryExtractValue(visualBrick.Value.ToString(), SubmittableParameterValue.PathKey, out path))
                    {
                    }
                    else if (SubmittableParameterValue.TryExtractValue(visualBrick.Value.ToString(), SubmittableParameterValue.ExactPathKey, out path))
                    {
                        isExactOptionPath = true;
                    }
                    else
                    {
                        continue;
                    }

                    var submittableParameterValue = 
                        submittableParameterValues
                        .SingleOrDefault(
                            x => 
                            x.Path == path
                            && x.IsExactOptionPath == isExactOptionPath);


                    if (submittableParameterValue == null)
                    {
                        submittableParameterValue = new SubmittableParameterValue(path, isExactOptionPath);
                        submittableParameterValues.Add(submittableParameterValue);
                    }

                    submittableParameterValue.ControlName = control.Name;
                    submittableParameterValue.Text = visualBrick.Text;
                }
            }

            return submittableParameterValues;
        }
    }
}
