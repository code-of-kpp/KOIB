using System; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Keyboard.Config; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Keyboard 
{ 
    public abstract class BaseKeyboardDriver : IKeyboardDriver 
    { 
        private static readonly object s_sync = new object(); 
        protected bool _disposed; 
        private Thread _workThread; 
        protected ILogger _logger; 
        #region IKeyboardDriver Members 
        public virtual void Init(KeyboardDriverConfig config, ILogger logger) 
        { 
            _logger = logger; 
        } 
        public event EventHandler<DriverKeyPressedEventArgs> KeyPressed; 
        protected void RaiseKeyPressed(DriverKeyPressedEventArgs e) 
        { 
            KeyPressed.RaiseEvent(this, e); 
        } 
        public void Start() 
        { 
            lock (s_sync) 
            { 
                if (_workThread != null) 
                    _workThread.SafeAbort(); 
                _workThread = ThreadUtils.StartBackgroundThread(WorkMethod); 
            } 
        } 
        private void WorkMethod() 
        { 
            while (!_disposed) 
            { 
                try 
                { 
                    if (!_disposed) 
                    { 
                        ReadKey(); 
                    } 
                } 
                catch (Exception ex) 
                { 
                    _logger.LogError(Message.KeyboardDriverWorkMethodFailed, ex, ex.Message); 
                } 
            } 
        } 
        protected abstract void ReadKey(); 
        #endregion 
        #region IDisposable Members 
        public virtual void Dispose() 
        { 
            lock (s_sync) 
            { 
                _disposed = true; 
                FreeDevice(); 
            } 
        } 
        protected virtual void FreeDevice() 
        { 
        } 
        #endregion 
    } 
}
