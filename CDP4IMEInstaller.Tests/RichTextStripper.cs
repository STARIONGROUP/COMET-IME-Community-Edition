// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RichTextStripper.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//            Nathanael Smiechowski, Kamil Wojnowski
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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CDP4IMEInstaller.Tests
{
    /// <summary>
    /// Rich Text Stripper
    /// </summary>
    public static class RichTextStripper
    {
        /// <summary>
        /// Internal helper class for stripping a RTF file
        /// </summary>
        private class StackEntry
        {
            /// <summary>
            /// The number of characters to skip
            /// </summary>
            public int NumberOfCharactersToSkip { get; private set; }

            /// <summary>
            /// Is a <see cref="StackEntry"/> ignorable?
            /// </summary>
            public bool Ignorable { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="numberOfCharactersToSkip">The number of characters to skip</param>
            /// <param name="ignorable">Is a <see cref="StackEntry"/> ignorable?</param>
            public StackEntry(int numberOfCharactersToSkip, bool ignorable)
            {
                this.NumberOfCharactersToSkip = numberOfCharactersToSkip;
                this.Ignorable = ignorable;
            }
        }

        /// <summary>
        /// The <see cref="Regex"/>
        /// </summary>
        private static readonly Regex RtfRegex = new Regex(@"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// List of destinations
        /// </summary>
        private static readonly List<string> Destinations = new List<string>
        {
            "aftncn","aftnsep","aftnsepc","annotation","atnauthor","atndate","atnicn","atnid",
            "atnparent","atnref","atntime","atrfend","atrfstart","author","background",
            "bkmkend","bkmkstart","blipuid","buptim","category","colorschememapping",
            "colortbl","comment","company","creatim","datafield","datastore","defchp","defpap",
            "do","doccomm","docvar","dptxbxtext","ebcend","ebcstart","factoidname","falt",
            "fchars","ffdeftext","ffentrymcr","ffexitmcr","ffformat","ffhelptext","ffl",
            "ffname","ffstattext","field","file","filetbl","fldinst","fldrslt","fldtype",
            "fname","fontemb","fontfile","fonttbl","footer","footerf","footerl","footerr",
            "footnote","formfield","ftncn","ftnsep","ftnsepc","g","generator","gridtbl",
            "header","headerf","headerl","headerr","hl","hlfr","hlinkbase","hlloc","hlsrc",
            "hsv","htmltag","info","keycode","keywords","latentstyles","lchars","levelnumbers",
            "leveltext","lfolevel","linkval","list","listlevel","listname","listoverride",
            "listoverridetable","listpicture","liststylename","listtable","listtext",
            "lsdlockedexcept","macc","maccPr","mailmerge","maln","malnScr","manager","margPr",
            "mbar","mbarPr","mbaseJc","mbegChr","mborderBox","mborderBoxPr","mbox","mboxPr",
            "mchr","mcount","mctrlPr","md","mdeg","mdegHide","mden","mdiff","mdPr","me",
            "mendChr","meqArr","meqArrPr","mf","mfName","mfPr","mfunc","mfuncPr","mgroupChr",
            "mgroupChrPr","mgrow","mhideBot","mhideLeft","mhideRight","mhideTop","mhtmltag",
            "mlim","mlimloc","mlimlow","mlimlowPr","mlimupp","mlimuppPr","mm","mmaddfieldname",
            "mmath","mmathPict","mmathPr","mmaxdist","mmc","mmcJc","mmconnectstr",
            "mmconnectstrdata","mmcPr","mmcs","mmdatasource","mmheadersource","mmmailsubject",
            "mmodso","mmodsofilter","mmodsofldmpdata","mmodsomappedname","mmodsoname",
            "mmodsorecipdata","mmodsosort","mmodsosrc","mmodsotable","mmodsoudl",
            "mmodsoudldata","mmodsouniquetag","mmPr","mmquery","mmr","mnary","mnaryPr",
            "mnoBreak","mnum","mobjDist","moMath","moMathPara","moMathParaPr","mopEmu",
            "mphant","mphantPr","mplcHide","mpos","mr","mrad","mradPr","mrPr","msepChr",
            "mshow","mshp","msPre","msPrePr","msSub","msSubPr","msSubSup","msSubSupPr","msSup",
            "msSupPr","mstrikeBLTR","mstrikeH","mstrikeTLBR","mstrikeV","msub","msubHide",
            "msup","msupHide","mtransp","mtype","mvertJc","mvfmf","mvfml","mvtof","mvtol",
            "mzeroAsc","mzeroDesc","mzeroWid","nesttableprops","nextfile","nonesttables",
            "objalias","objclass","objdata","object","objname","objsect","objtime","oldcprops",
            "oldpprops","oldsprops","oldtprops","oleclsid","operator","panose","password",
            "passwordhash","pgp","pgptbl","picprop","pict","pn","pnseclvl","pntext","pntxta",
            "pntxtb","printim","private","propname","protend","protstart","protusertbl","pxe",
            "result","revtbl","revtim","rsidtbl","rxe","shp","shpgrp","shpinst",
            "shppict","shprslt","shptxt","sn","sp","staticval","stylesheet","subject","sv",
            "svb","tc","template","themedata","title","txe","ud","upr","userprops",
            "wgrffmtfilter","windowcaption","writereservation","writereservhash","xe","xform",
            "xmlattrname","xmlattrvalue","xmlclose","xmlname","xmlnstbl",
            "xmlopen"
        };

        /// <summary>
        /// Special characters
        /// </summary>
        private static readonly Dictionary<string, string> SpecialCharacters = new Dictionary<string, string>
        {
            { "par", "\n" },
            { "sect", "\n\n" },
            { "page", "\n\n" },
            { "line", "\n" },
            { "tab", "\t" },
            { "emdash", "\u2014" },
            { "endash", "\u2013" },
            { "emspace", "\u2003" },
            { "enspace", "\u2002" },
            { "qmspace", "\u2005" },
            { "bullet", "\u2022" },
            { "lquote", "\u2018" },
            { "rquote", "\u2019" },
            { "ldblquote", "\u201C" },
            { "rdblquote", "\u201D" },
        };

        /// <summary>
        /// Strip RTF Tags from RTF Text
        /// </summary>
        /// <param name="inputRtf">RTF formatted text</param>
        /// <returns>Plain text from RTF</returns>
        public static string StripRichTextFormat(string inputRtf)
        {
            if (inputRtf == null)
            {
                return null;
            }

            string returnString;

            var stack = new Stack<StackEntry>();
            var ignorable = false;              // Whether this group (and all inside it) are "ignorable".
            var ucskip = 1;                      // Number of ASCII characters to skip after a unicode character.
            var curskip = 0;                     // Number of ASCII characters left to skip
            var outList = new List<string>();    // Output buffer.

            var matches = RtfRegex.Matches(inputRtf);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var word = match.Groups[1].Value;
                    var arg = match.Groups[2].Value;
                    var hex = match.Groups[3].Value;
                    var character = match.Groups[4].Value;
                    var brace = match.Groups[5].Value;
                    var tchar = match.Groups[6].Value;

                    if (!string.IsNullOrEmpty(brace))
                    {
                        curskip = 0;

                        if (brace == "{")
                        {
                            // Push state
                            stack.Push(new StackEntry(ucskip, ignorable));
                        }
                        else if (brace == "}")
                        {
                            // Pop state
                            var entry = stack.Pop();
                            ucskip = entry.NumberOfCharactersToSkip;
                            ignorable = entry.Ignorable;
                        }
                    }
                    else if (!string.IsNullOrEmpty(character)) // \x (not a letter)
                    {
                        curskip = 0;

                        if (character == "~")
                        {
                            if (!ignorable)
                            {
                                outList.Add("\xA0");
                            }
                        }
                        else if ("{}\\".Contains(character))
                        {
                            if (!ignorable)
                            {
                                outList.Add(character);
                            }
                        }
                        else if (character == "*")
                        {
                            ignorable = true;
                        }
                    }
                    else if (!String.IsNullOrEmpty(word)) // \foo
                    {
                        curskip = 0;

                        if (Destinations.Contains(word))
                        {
                            ignorable = true;
                        }
                        else if (ignorable)
                        {
                        }
                        else if (SpecialCharacters.ContainsKey(word))
                        {
                            outList.Add(SpecialCharacters[word]);
                        }
                        else if (word == "uc")
                        {
                            ucskip = int.Parse(arg);
                        }
                        else if (word == "u")
                        {
                            var c = int.Parse(arg);

                            if (c < 0)
                            {
                                c += 0x10000;
                            }

                            outList.Add(Char.ConvertFromUtf32(c));
                            curskip = ucskip;
                        }
                    }
                    else if (!string.IsNullOrEmpty(hex)) // \'xx
                    {
                        if (curskip > 0)
                        {
                            curskip -= 1;
                        }
                        else if (!ignorable)
                        {
                            var c = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                            outList.Add(char.ConvertFromUtf32(c));
                        }
                    }
                    else if (!string.IsNullOrEmpty(tchar))
                    {
                        if (curskip > 0)
                        {
                            curskip -= 1;
                        }
                        else if (!ignorable)
                        {
                            outList.Add(tchar);
                        }
                    }
                }
            }
            else
            {
                // Didn't match the regex
                returnString = inputRtf;
            }

            returnString = string.Join(string.Empty, outList.ToArray());

            return returnString;
        }
    }
}
