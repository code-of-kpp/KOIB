using System.Threading; 
using Croc.Core.Extensions; 
namespace Croc.Core.Utils.Threading 
{ 
    public class TimeoutWaitHandle : EventWaitHandleEx 
    { 
        private readonly int _timeout; 
        private Thread _eventThread; 
        private bool _disposed; 
        public TimeoutWaitHandle(int timeout) 
            : base(false, false) 
        { 
            _timeout = timeout; 
        } 
        new public void Reset() 
        { 
            if (_eventThread != null) 
                _eventThread.SafeAbort(); 
            _eventThread = ThreadUtils.StartBackgroundThread(WaitingForTimeout); 
        } 
        private void WaitingForTimeout() 
        { 
            Thread.Sleep(_timeout); 
            if(!_disposed) 
                Set(); 
        } 
        protected override void Dispose(bool explicitDisposing) 
        { 
            _disposed = true; 
            base.Dispose(explicitDisposing); 
        } 
    } 
}
