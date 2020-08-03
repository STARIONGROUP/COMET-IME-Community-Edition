using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4IME.Modularity
{
    using System.IO;

    /// <summary>
    /// 
    /// </summary>
    public class UpdateUtilities
    {
        /// <summary>
        /// Gets the path of the AppData directory
        /// </summary>
        /// <returns>the Path the AppData folder</returns>
        public static string GetAppDataPath()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RHEA", "CDP4");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            return appDataPath;
        }
    }
}
