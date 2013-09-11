using System; 
using System.Threading; 
namespace Croc.Core.Utils.Threading 
{ 
    public class EventWaitHandleEx : WaitHandle, IDisposable 
    { 
        private readonly EventWaitHandle _innerEvent; 
        private readonly object _securityObject; 
        private bool _accessGranted = false; 
        public EventWaitHandleEx(bool initialState, bool manualReset) 
            : this(initialState, manualReset, null) 
        { 
        } 
        public EventWaitHandleEx(bool initialState, bool manualReset, object securityObject) 
        { 
            _innerEvent = manualReset  
                ? (EventWaitHandle)new ManualResetEvent(initialState)  
                : (EventWaitHandle)new AutoResetEvent(initialState); 
            base.SafeWaitHandle = _innerEvent.SafeWaitHandle; 
            _securityObject = securityObject; 
        } 
        #region Получение доступа 
        public bool GetAccess(object securityObject) 
        { 
            _accessGranted = (_securityObject == null || _securityObject.Equals(securityObject)); 
            return _accessGranted; 
        } 
        private bool CheckAccess() 
        { 
            if (_securityObject != null && !_accessGranted) 
                return false; 
            _accessGranted = false; 
            return true; 
        } 
        #endregion 
        #region Вкл/Выкл 
        public bool Reset() 
        { 
            if (!CheckAccess()) 
                return false; 
            return _innerEvent.Reset(); 
        } 
        public bool Set() 
        { 
            if (!CheckAccess()) 
                return false; 
            try 
            { 
                return _innerEvent.Set(); 
            } 
            catch 
            { 
                return false; 
            } 
        } 
        #endregion 
        #region Ожидание 
        public override bool WaitOne() 
        { 
            return _innerEvent.WaitOne(); 
        } 
        public override bool WaitOne(TimeSpan timeout) 
        { 
            return _innerEvent.WaitOne(timeout, false); 
        } 
        public override bool WaitOne(TimeSpan timeout, bool exitContext) 
        { 
            return _innerEvent.WaitOne(timeout, exitContext); 
        } 
        public override bool WaitOne(int millisecondsTimeout) 
        { 
            return _innerEvent.WaitOne(millisecondsTimeout); 
        } 
        public override bool WaitOne(int millisecondsTimeout, bool exitContext) 
        { 
            return _innerEvent.WaitOne(millisecondsTimeout, exitContext); 
        } 
        #endregion 
        #region Освобождение ресурсов 
        public override void Close() 
        { 
            Dispose(); 
        } 
        ~EventWaitHandleEx() 
        { 
            Dispose(false); 
        }         
        public void Dispose() 
        { 
            Dispose(true); 
            GC.SuppressFinalize(this); 
        } 
        protected override void Dispose(bool explicitDisposing) 
        { 
            _innerEvent.Close(); 
        } 
        #endregion 
    } 
}
