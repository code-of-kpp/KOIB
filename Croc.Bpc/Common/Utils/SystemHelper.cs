using System; 
using Croc.Bpc.Diagnostics; 
using Croc.Core; 
using Croc.Core.Utils; 
namespace Croc.Bpc.Utils 
{ 
    public static class SystemHelper 
    { 
        public static void SetSystemTime(DateTime utcDateTime) 
        { 
            if (PlatformDetector.IsUnix) 
            { 
                ProcessHelper.StartProcessAndWaitForFinished( 
                    "date", 
                    string.Format("--universal --set=\"{0:MM/dd/yyyy HH:mm:ss}\"", utcDateTime),  
                    null, null); 
                ProcessHelper.StartProcessAndWaitForFinished( 
                    "/sbin/hwclock",  
                    "--systohc --utc --noadjfile",  
                    null, null); 
            } 
            else 
            { 
                var st = new Win32.SystemTime 
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
        public static void SyncFileSystem() 
        { 
            if (!PlatformDetector.IsUnix) 
                return; 
            try 
            { 
                Mono.Unix.Native.Syscall.sync(); 
            } 
            catch (Exception ex) 
            { 
                CoreApplication.Instance.Logger.LogWarning(Message.FileSystemSyncError, ex); 
            } 
        } 
    } 
}
