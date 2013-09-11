using System; 
using System.Diagnostics; 
using System.Threading; 
using Croc.Core.Configuration; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Threading; 
namespace Croc.Core 
{ 
    public abstract class Subsystem : ISubsystem 
    { 
        public string Name 
        { 
            get; 
            set; 
        } 
        public ICoreApplication Application 
        { 
            get; 
            set; 
        } 
        public virtual void Init(SubsystemConfig config) 
        { 
        } 
        public virtual void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
        } 
        public event EventHandler<ConfigUpdatedEventArgs> ConfigUpdated; 
        protected void RaiseConfigUpdatedEvent(ConfigUpdatedEventArgs e) 
        { 
            ConfigUpdated.RaiseEvent(this, e); 
        } 
        #region Логирование 
        private static readonly object s_loggerLock = new object(); 
        private ILogger _logger; 
        public bool SeparateLog 
        { 
            get; 
            set; 
        } 
        public TraceLevel TraceLevel 
        { 
            get; 
            internal set; 
        } 
        public string LogFileFolder 
        { 
            get; 
            internal set; 
        } 
        public ILogger Logger 
        { 
            get 
            { 
                if (_logger == null) 
                    lock (s_loggerLock) 
                        if (_logger == null) 
                            _logger = Application.CreateLogger(Name, TraceLevel); 
                return _logger; 
            } 
        } 
        public void DisposeLogger() 
        { 
            if (_logger != null) 
                lock (s_loggerLock) 
                    if (_logger != null) 
                    { 
                        Disposer.DisposeObject(_logger); 
                        _logger = null; 
                    } 
        } 
        #endregion 
        #region Ожидание событий 
        protected bool Sleep(TimeSpan timeout, IWaitController waitCtrl) 
        { 
            return !(waitCtrl == null ? _disposeEvent.WaitOne(timeout) : waitCtrl.WaitOne(_disposeEvent, timeout)); 
        } 
        protected bool WaitOne(WaitHandle ev, IWaitController waitCtrl) 
        { 
            int index; 
            return WaitAny(new[] { ev }, out index, waitCtrl); 
        } 
        protected bool WaitOne(WaitHandle ev, int timeout, IWaitController waitCtrl) 
        { 
            int index; 
            return WaitAny(new[] { ev }, out index, timeout, waitCtrl); 
        } 
        protected bool WaitAny(WaitHandle[] events, out int occurredEventIndex, IWaitController waitCtrl) 
        { 
            return WaitAny(events, out occurredEventIndex, Timeout.Infinite, waitCtrl); 
        } 
        protected bool WaitAny( 
            WaitHandle[] events, out int occurredEventIndex, int timeout, IWaitController waitCtrl) 
        { 
            var eventsEx = new WaitHandle[events.Length + 1]; 
            var disposeEventIndex = events.Length; 
            events.CopyTo(eventsEx, 0); 
            eventsEx[disposeEventIndex] = _disposeEvent; 
            occurredEventIndex = 
                waitCtrl == null 
                    ? WaitHandle.WaitAny(eventsEx, timeout) 
                    : waitCtrl.WaitAny(eventsEx, timeout); 
            return occurredEventIndex != disposeEventIndex; 
        } 
        protected bool WaitAll(WaitHandle[] events, IWaitController waitCtrl) 
        { 
            var index = 
                waitCtrl == null 
                    ? WaitHandleUtils.WaitOneOrAllOthers(_disposeEvent, events) 
                    : waitCtrl.WaitOneOrAllOthers(_disposeEvent, events); 
            return index != 0; 
        } 
        #endregion 
        #region Освобождение ресурсов 
        protected ManualResetEvent _disposeEvent = new ManualResetEvent(false); 
        protected bool _disposed; 
        public int DisposeOrder 
        { 
            get; 
            set; 
        } 
        public virtual void Dispose() 
        { 
            _disposeEvent.Set(); 
            _disposed = true; 
        } 
        #endregion 
    } 
}
