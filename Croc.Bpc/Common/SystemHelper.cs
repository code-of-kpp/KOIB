using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Utils; 

using Croc.Bpc.Common; 

 

 

namespace Croc.Bpc.Common 

{ 

    /// <summary> 

    /// Класс для работы с операционной системой 

    /// </summary> 

    public static class SystemHelper 

    { 

        /// <summary> 

        /// Устанавливает текущую дату и время ОС 

        /// </summary> 

        /// <param name="dt"></param> 

        public static void SetSystemTime(DateTime utcDateTime) 

        { 

            if (PlatformDetector.IsUnix) 

            { 

                // параметры команды для установки времени: 

                // --universal  - показывает, что устанавливаемое время в UTC 

                // --set        - задает новую дату и время 

                const string SETDATEPARAMSFORMAT = "--universal --set=\"{0:MM/dd/yyyy HH:mm:ss}\""; 

 

 

                var setDateParams = string.Format(SETDATEPARAMSFORMAT, utcDateTime); 

                ProcessHelper.StartProcessAndWaitForFinished("date", setDateParams, null, null); 

                ProcessHelper.StartProcessAndWaitForFinished("/sbin/hwclock", "--systohc --localtime", null, null); 

            } 

            else 

            { 

                var st = new Win32.SystemTime() 

                { 

                    Year = (ushort)utcDateTime.Year, 

                    Month = (ushort)utcDateTime.Month, 

                    Day = (ushort)utcDateTime.Day, 

                    DayOfWeek = (ushort)utcDateTime.DayOfWeek, 

                    Hour = (ushort)utcDateTime.Hour, 

                    Milliseconds = (ushort)utcDateTime.Millisecond, 

                    Minute = (ushort)utcDateTime.Minute, 

                    Second = (ushort)utcDateTime.Second 

                }; 

                Win32.SetSystemTime(ref st); 

            } 

        } 

    } 


}


