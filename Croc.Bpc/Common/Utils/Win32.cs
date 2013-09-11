using System.Runtime.InteropServices; 
namespace Croc.Bpc.Utils 
{ 
    public static class Win32 
    { 
        [StructLayout(LayoutKind.Sequential)] 
        public struct SystemTime 
        { 
            public ushort Year; 
            public ushort Month; 
            public ushort DayOfWeek; 
            public ushort Day; 
            public ushort Hour; 
            public ushort Minute; 
            public ushort Second; 
            public ushort Milliseconds; 
        }; 
        [DllImport("Kernel32")] 
        public static extern bool SetSystemTime(ref SystemTime st); 
    } 
}
