using System; 
using System.Threading; 
namespace Croc.Core 
{ 
    public interface IWaitController 
    { 
        void Sleep(TimeSpan timeout); 
        void Sleep(int millisecondsTimeout); 
        void WaitOne(WaitHandle waitHandle); 
        bool WaitOne(WaitHandle waitHandle, TimeSpan timeout); 
        int WaitAny(WaitHandle[] waitHandles); 
        int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout); 
        int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout); 
        int WaitOneOrAllOthers(WaitHandle one, WaitHandle[] others); 
    } 
}
