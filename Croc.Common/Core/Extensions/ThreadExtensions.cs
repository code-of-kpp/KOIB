using System.Threading; 
namespace Croc.Core.Extensions 
{ 
    public static class ThreadExtensions 
    { 
        public static void SafeAbort(this Thread thread) 
        { 
            try 
            { 
                thread.Abort(); 
            } 
            catch 
            { 
            } 
        } 
        public static void SafeAbort(this Thread thread, int millisecondsTimeout) 
        { 
            if (!thread.Join(millisecondsTimeout)) 
                thread.SafeAbort(); 
        } 
    } 
}
