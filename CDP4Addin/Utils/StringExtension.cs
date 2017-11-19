// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtension.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Addin.Utils
{
    using CDP4Composition;
    using NetOffice.OfficeApi.Enums;

    /// <summary>
    /// Static extension class to convert a string to a <see cref="MsoCTPDockPosition"/>
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Convert a <see cref="string"/> to a <see cref="MsoCTPDockPosition"/>
        /// </summary>
        /// <param name="str">
        /// The string to convert
        /// </param>
        /// <returns>
        /// a <see cref="MsoCTPDockPosition"/> value, the default is <see cref="MsoCTPDockPosition.msoCTPDockPositionLeft"/>
        /// </returns>
        public static MsoCTPDockPosition ToDockPosition(this string str)
        {
            MsoCTPDockPosition dockPosition;

            switch (str)
            {
                case RegionNames.LeftPanel:
                    dockPosition = MsoCTPDockPosition.msoCTPDockPositionLeft;
                    break;
                case RegionNames.RightPanel:
                    dockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
                    break;
                case RegionNames.BottomPanel:
                    dockPosition = MsoCTPDockPosition.msoCTPDockPositionBottom;
                    break;                    
                default:
                    dockPosition = MsoCTPDockPosition.msoCTPDockPositionLeft;
                    break;
            }

            return dockPosition;
        }
    }
}
