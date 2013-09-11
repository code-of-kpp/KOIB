using System.Threading; 
namespace Croc.Core.Utils.Threading 
{ 
    public static class ThreadUtils 
    { 
        public static Thread StartBackgroundThread(ThreadStart threadMethod) 
        { 
            var thread = new Thread(threadMethod) {IsBackground = true}; 
            thread.Start(); 
            return thread; 
        } 
        public static Thread StartBackgroundThread(ParameterizedThreadStart threadMethod, object threadParameter) 
        { 
            var thread = new Thread(threadMethod) { IsBackground = true }; 
            thread.Start(threadParameter); 
            return thread; 
        } 
    } 
}
