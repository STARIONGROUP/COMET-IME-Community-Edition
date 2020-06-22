// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnownAssembliesClassDrop.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4IMEInstaller.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using DotLiquid;

    /// <summary>
    /// Helper class for <see cref="CDP4IMEInstallerTestFixture"/> that can also be used as a parameter for "executing" a DotLiquid template 
    /// </summary>
    public class KnownAssembliesClassDrop : Drop
    {
        /// <summary>
        /// List of all the DevExpress assemblies that should be added through the installer
        /// </summary>
        private readonly Dictionary<string, string> knownAssemblies = new Dictionary<string, string>
        {
            { "DevExpress.Charts.{VERSION}.Core.dll", "C02CAE1D-68F8-4826-A781-7022D5D88555"} ,
            { "DevExpress.CodeParser.{VERSION}.dll", "71D12292-68AC-4BBF-9DF5-C5E614A3A683"} ,
            { "DevExpress.Data.{VERSION}.dll", "90BBA601-EC72-48D0-B370-A726CFBE26D5"} ,
            { "DevExpress.Data.Desktop.{VERSION}.dll", "461C5BA0-DBDB-4F35-B87C-F0F0E94FA752" },
            { "DevExpress.DataAccess.{VERSION}.dll", "A159A34F-C2BC-4E62-AB7A-2992F34AA343"} ,
            { "DevExpress.Diagram.{VERSION}.Core.dll", "0D4DF5E0-C0AA-47CA-9523-F754919BF970"} ,
            { "DevExpress.Images.{VERSION}.dll", "B9BFD73B-BF82-4742-9F04-0A5F0F537F43"} ,
            { "DevExpress.Mvvm.{VERSION}.dll", "0D612BC9-69A2-4237-93E4-E69CF726B87B"} ,
            { "DevExpress.Office.{VERSION}.Core.dll", "6B2516CB-D935-4E94-A2E0-CFD96232C662"} ,
            { "DevExpress.Pdf.{VERSION}.Core.dll", "BE68B644-1558-4082-A5A7-1C2754BDEF90"} ,
            { "DevExpress.Pdf.{VERSION}.Drawing.dll", "A9F7341E-61D8-454C-8D03-A69FD2974052" },
            { "DevExpress.PivotGrid.{VERSION}.Core.dll", "3826E4B8-F154-4B0A-84F2-4A9CA28498D7"} ,
            { "DevExpress.RichEdit.{VERSION}.Export.dll", "6215A1C8-B0AC-48C1-8D2A-8F6CBB324D48"} ,
            { "DevExpress.Printing.{VERSION}.Core.dll", "4B9B71D8-4412-49A5-A1E5-3631B07802CB"} ,
            { "DevExpress.RichEdit.{VERSION}.Core.dll", "BF10AE75-20E2-41C5-8BD3-395C148DA6D3"} ,
            { "DevExpress.Sparkline.{VERSION}.Core.dll", "40E62069-BCCC-406F-95A0-52604A2B655A"} ,
            { "DevExpress.SpellChecker.{VERSION}.Core.dll", "C6F208B7-78F8-4584-B2B0-7099E07131BB"} ,
            { "DevExpress.Utils.{VERSION}.dll", "9071C230-2BF6-42BB-A935-F668F0AA6236"} ,
            { "DevExpress.Xpf.Accordion.{VERSION}.dll", "514DF947-1C96-4F8D-A919-C376A260FCCA" },
            { "DevExpress.Xpf.Charts.{VERSION}.dll", "794F805A-F925-48E3-9495-E6E58CD0325B"} ,
            { "DevExpress.Xpf.CodeView.{VERSION}.dll", "C5377FD4-C05C-40C4-8951-B1F873E71FA5"} ,
            { "DevExpress.Xpf.Core.{VERSION}.dll", "41F121BD-75AC-4107-B5FC-1124878A5524"} ,
            { "DevExpress.Xpf.Controls.{VERSION}.dll", "D695D0A9-3115-49ED-82A5-B485AC75052B"} ,
            { "DevExpress.Xpf.Core.{VERSION}.Extensions.dll", "CCADFC40-14FF-4E7A-A8DC-3B7CD09F474C"} ,
            { "DevExpress.Xpf.DataAccess.{VERSION}.dll", "E44FC402-03B0-4E2A-A918-438245661AE1" },
            { "DevExpress.Xpf.Diagram.{VERSION}.dll", "14D51648-6A12-48D7-BD14-87990B9C4AC7"} ,
            { "DevExpress.Xpf.Docking.{VERSION}.dll", "CED11B3D-8D6D-442E-B6EB-329DDD3B3028"} ,
            { "DevExpress.Xpf.DocumentViewer.{VERSION}.Core.dll", "A43E3DFA-5076-4D14-80D5-0C73D0B130A0"} ,
            { "DevExpress.Xpf.ExpressionEditor.{VERSION}.dll", "2D29816F-7125-438F-B36D-7F20CBB63AE3" },
            { "DevExpress.Xpf.Grid.{VERSION}.dll", "9B10747B-031D-4FCD-9E2D-5B4F714F0F70"} ,
            { "DevExpress.Xpf.Grid.{VERSION}.Core.dll", "4BF10AF7-46B3-470E-90C2-AEED67461EA4"} ,
            { "DevExpress.Xpf.Grid.{VERSION}.Extensions.dll", "0BA3E743-D3AC-49D6-827A-36D53730526D"} ,
            { "DevExpress.Xpf.Layout.{VERSION}.Core.dll", "BA9DA4E2-2D8F-4D79-9677-E62B225C62A7"} ,
            { "DevExpress.Xpf.LayoutControl.{VERSION}.dll", "5440FCA7-FDFF-404B-8A34-1F8B72FC569D"} ,
            { "DevExpress.Xpf.NavBar.{VERSION}.dll", "B5D4E1CB-FCE1-41F5-AC53-E4B7ECD6CC35"} ,
            { "DevExpress.Xpf.Office.{VERSION}.dll", "ED22B90E-D14C-46FD-9410-0DDB4CC55A04" },
            { "DevExpress.Xpf.PivotGrid.{VERSION}.dll", "B37C23DB-E8FB-4B0E-BA48-A9727703690B" },
            { "DevExpress.Xpf.Printing.{VERSION}.dll", "51C4C490-50F8-49A9-86E3-B79038CBE4F5"} ,
            { "DevExpress.Xpf.PropertyGrid.{VERSION}.dll", "995D6C5B-0792-4EBC-A6DD-3604FE92166F"} ,
            { "DevExpress.Xpf.Ribbon.{VERSION}.dll", "536398DE-BCA0-4756-B124-D56ED025548D"} ,
            { "DevExpress.Xpf.RichEdit.{VERSION}.dll", "FFA5D964-3E1D-4375-B490-5E0EC27ED878" },
            { "DevExpress.Xpf.ReportDesigner.{VERSION}.dll", "27BBFF45-CB03-475A-9A7C-E06731211115" },
            { "DevExpress.Xpf.SpellChecker.{VERSION}.dll", "B21A5DDA-08F4-4988-BB3E-2C0712005553"} ,
            { "DevExpress.Xpf.Themes.DXStyle.{VERSION}.dll", "778F6AF7-0136-4FB3-8E4B-89DEF4E96581"} ,
            { "DevExpress.Xpf.Themes.LightGray.{VERSION}.dll", "A29C4BFE-BFBA-4B96-855E-5ABD2A2F8275"} ,
            { "DevExpress.Xpf.Themes.MetropolisDark.{VERSION}.dll", "170822B6-976A-4A94-A05D-019206A2ECD6"} ,
            { "DevExpress.Xpf.Themes.MetropolisLight.{VERSION}.dll", "C507C76E-B192-4FC7-AAD7-3598D26D3038"} ,
            { "DevExpress.Xpf.Themes.Office2007Black.{VERSION}.dll", "73B623A1-1F40-44B3-81D9-A26A6935B529"} ,
            { "DevExpress.Xpf.Themes.Office2007Blue.{VERSION}.dll", "5367F803-1F48-4011-8E64-5C4741048238"} ,
            { "DevExpress.Xpf.Themes.Office2007Silver.{VERSION}.dll", "964DC6CB-D067-498E-9EDC-BE2892D4D771"} ,
            { "DevExpress.Xpf.Themes.Office2010Black.{VERSION}.dll", "3769066C-6172-4F6B-8929-063D9200CD6C"} ,
            { "DevExpress.Xpf.Themes.Office2010Blue.{VERSION}.dll", "8848D982-C746-4FE8-811C-8CB4CFE7A734"} ,
            { "DevExpress.Xpf.Themes.Office2010Silver.{VERSION}.dll", "76DBB259-49B5-4A50-A98F-43EB365D974C"} ,
            { "DevExpress.Xpf.Themes.Office2013.{VERSION}.dll", "A0C208AA-02D6-4AE8-94EA-865C01E0FF07"} ,
            { "DevExpress.Xpf.Themes.Office2013DarkGray.{VERSION}.dll", "47DDFCFD-2DC0-47FD-8383-B65B28981E81"} ,
            { "DevExpress.Xpf.Themes.Office2013LightGray.{VERSION}.dll", "F6E798C2-B2E6-4AB2-89C1-35AC67B3C315"} ,
            { "DevExpress.Xpf.Themes.Office2016Black.{VERSION}.dll", "23A852A0-C7AE-41AC-9476-5CB9EF02DFBC" },
            { "DevExpress.Xpf.Themes.Office2016BlackSE.{VERSION}.dll", "A6179DE5-7FD4-4C43-9958-AD4054935F34" },
            { "DevExpress.Xpf.Themes.Office2016Colorful.{VERSION}.dll", "D16F3591-6EFB-4CEA-88E3-9A4F1660F686" },
            { "DevExpress.Xpf.Themes.Office2016ColorfulSE.{VERSION}.dll", "380A6A42-B924-4492-AC28-CCAD29866706" },
            { "DevExpress.Xpf.Themes.Office2016DarkGraySE.{VERSION}.dll", "6977AB6C-1D26-4B35-932B-06BBDFDFCF71" },
            { "DevExpress.Xpf.Themes.Office2016White.{VERSION}.dll", "194B4B00-50B8-4BE5-9526-F60FBEAAFC7D" },
            { "DevExpress.Xpf.Themes.Office2016WhiteSE.{VERSION}.dll", "99A6087E-9BB9-420A-80A2-BB2EAF63A442" },
            { "DevExpress.Xpf.Themes.Office2019Black.{VERSION}.dll", "E30D419A-807E-4F8E-8693-18F4677EA0C7" },
            { "DevExpress.Xpf.Themes.Office2019Colorful.{VERSION}.dll", "E3E747FB-3B71-4BA0-9818-AFA996091AE4" },
            { "DevExpress.Xpf.Themes.Office2019DarkGray.{VERSION}.dll", "74C3C750-26FF-4A5E-A76C-B6EA5F998A31" },
            { "DevExpress.Xpf.Themes.Office2019HighContrast.{VERSION}.dll", "C87487FF-C8DE-407E-A524-D02C4ACAB4BE" },
            { "DevExpress.Xpf.Themes.Office2019White.{VERSION}.dll", "7B6ED347-E3EF-4AF1-B58B-90D571BF8926" },
            { "DevExpress.Xpf.Themes.Seven.{VERSION}.dll", "E12B5FD0-85B1-44CE-B686-0A164189F766"} ,
            { "DevExpress.Xpf.Themes.TouchlineDark.{VERSION}.dll", "7B9D9EAB-5997-4833-BD05-AA09BD2D771F"} ,
            { "DevExpress.Xpf.Themes.VS2010.{VERSION}.dll", "F6E8C7E6-3702-455A-B0DF-FBB967EEA8F7"} ,
            { "DevExpress.Xpf.Themes.VS2017Blue.{VERSION}.dll", "6047ABA9-B05E-43AE-808A-DC399DB4FAF7" },
            { "DevExpress.Xpf.Themes.VS2017Dark.{VERSION}.dll", "E682BD8C-9640-4582-92CB-59CFC41D923B" },
            { "DevExpress.Xpf.Themes.VS2017Light.{VERSION}.dll", "BBCC0063-B6F4-432F-90FB-06ADF8BCC18F" },
            { "DevExpress.Xpf.Themes.VS2019Blue.{VERSION}.dll", "533DFB88-2C11-46B0-9B03-4D76BD95A65E" },
            { "DevExpress.Xpf.Themes.VS2019Dark.{VERSION}.dll", "8CD4A3DC-287E-4BF9-8B41-96C91DEB9009" },
            { "DevExpress.Xpf.Themes.VS2019Light.{VERSION}.dll", "1B2B69EF-58B5-4E69-9C6A-08A22767B37C" },
            { "DevExpress.Xpo.{VERSION}.dll", "62646496-DC18-43D0-AE37-B94032EE1CAA"} ,
            { "DevExpress.XtraBars.{VERSION}.dll", "5EAFD90B-7F26-4100-86B9-7116AA16559C" },
            { "DevExpress.XtraCharts.{VERSION}.dll", "C3BD8574-1870-4780-B7D9-BF4FBB271E0D"} ,
            { "DevExpress.XtraEditors.{VERSION}.dll", "83EF3F9A-B013-49C0-B841-C1CC8F7DCB05" },
            { "DevExpress.XtraGauges.{VERSION}.Core.dll", "939F9678-5859-4EC9-A15B-603C4C81ECC9" },
            { "DevExpress.XtraGrid.{VERSION}.dll", "7F7DF990-9AA1-459B-BF10-2978AA787293" },
            { "DevExpress.XtraLayout.{VERSION}.dll", "89546FC2-40A5-4A25-9FAC-57954C4C09E8" },
            { "DevExpress.XtraNavBar.{VERSION}.dll", "38D91884-536B-40BA-ADD7-5998F0929FE8" },
            { "DevExpress.XtraPivotGrid.{VERSION}.dll", "C9B60BF2-D299-4AF3-8C04-0C7D56628EF1" },
            { "DevExpress.XtraPrinting.{VERSION}.dll", "1BACC5FE-5739-4048-954E-129C536677D3" },
            { "DevExpress.XtraReports.{VERSION}.dll", "BC95F656-BEE7-4DFA-A9D2-5569D5672272" },
            { "DevExpress.XtraTreeList.{VERSION}.dll", "36B17D24-2AA8-4E20-8A7C-2C569452A7C7" },
            { "DevExpress.XtraVerticalGrid.{VERSION}.dll", "F26DEBE4-6BF2-4A72-8754-FEFBE3BC62B3" },
            { "DevExpress.XtraWizard.{VERSION}.dll", "4043AEC7-1839-4094-8C59-466F4136E0B2" }
        };

        /// <summary>
        /// An <see cref="IEnumerable{WxsObject}"/> that contains a <see cref="WxsObject"/> for every DevExpress assembly
        /// </summary>
        public IEnumerable<WxsObject> WxsObjects { get; private set; }

        /// <summary>
        /// A <see cref="IEnumerable{String}"/> that contains the names of all DevExpress assemblies
        /// </summary>
        public IEnumerable<string> Assemblies { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="version">The DevExpress version for which to check/created installer files</param>
        public KnownAssembliesClassDrop(string version)
        {
            this.WxsObjects = this.knownAssemblies.Select(
                x => new WxsObject(x.Key.Replace("{VERSION}", version),
                    x.Value,
                    $"_{x.Value.Split('-').Last()}",
                    $"_{x.Value.Split('-').First()}"));

            this.Assemblies = this.WxsObjects.Select(x => x.AssemblyName);
        }
    }
}
