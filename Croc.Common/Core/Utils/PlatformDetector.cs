using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core.Utils 

{ 

    /// <summary> 

    /// Класс для получения информации о текущей платформе, на которой выполняется приложение 

    /// </summary> 

    public class PlatformDetector 

    { 

        /// <summary> 

        /// Платформа - это Unix 

        /// </summary> 

        public static bool IsUnix = (Environment.OSVersion.Platform == PlatformID.Unix); 

    } 

}


