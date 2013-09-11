using System; 
using System.Text; 
using System.Threading; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
namespace Croc.Core.Utils.Text 
{ 
    public sealed class RollTextMachine : 
        IDisposable 
    { 
        public enum MachineState 
        { 
            Running, 
            Stopped, 
        } 
        public const int DEFAULT_UPDATE_TEXT_DELAY = 250; 
        public const int DEFAULT_START_DELAY = 500; 
        public const int DEFAULT_END_DELAY = 0; 
        public const int ROLL_LOOP_DELIM_SPACE_COUNT = 10; 
        public int UpdateTextDelay = DEFAULT_UPDATE_TEXT_DELAY; 
        public int StartDelay = DEFAULT_START_DELAY; 
        public int EndDelay = DEFAULT_END_DELAY; 
        private readonly ManualResetEvent _stopRollTextEvent = new ManualResetEvent(false); 
        public delegate bool NeedSetTextDelegate(string text); 
        public event NeedSetTextDelegate NeedSetText; 
        private bool RaiseNeedSetText(string text) 
        { 
            var handler = NeedSetText; 
            if (handler != null) 
                return handler(text); 
            return false; 
        } 
        private bool _disposed; 
        private readonly int _maxTextLength; 
        private readonly bool _rollIfLessThanMaxLen; 
        private string _rolledText = ""; 
        private string _realRolledText = ""; 
        public string RolledText 
        { 
            get 
            { 
                return _rolledText; 
            } 
            set 
            { 
                CodeContract.Requires(!string.IsNullOrEmpty(value)); 
                lock (s_syncRoot) 
                { 
                    var currentState = State; 
                    if (currentState == MachineState.Running) 
                        Stop(); 
                    _rolledText = value; 
                    if (currentState == MachineState.Running) 
                        Start(); 
                } 
            } 
        } 
        private Thread _rollTextThread; 
        private static readonly object s_syncRoot = new object(); 
        public MachineState State 
        { 
            get; 
            private set; 
        } 
        public RollTextMachine(int maxTextLength, bool rollIfLessThanMaxLen) 
        { 
            CodeContract.Requires(maxTextLength > 0); 
            _maxTextLength = maxTextLength; 
            _rollIfLessThanMaxLen = rollIfLessThanMaxLen; 
            State = MachineState.Stopped; 
        } 
        public void Start() 
        { 
            lock (s_syncRoot) 
            { 
                if (_disposed) 
                    return; 
                if (State == MachineState.Running) 
                    return; 


                StopRollTextThread(); 
                ResetCounters(); 
                SetRealRolledText(); 
                if (_rolledText.Length > 0) 
                { 
                    _stopRollTextEvent.Reset(); 
                    if (_rolledText.Length > _maxTextLength || 
                        _rollIfLessThanMaxLen) 
                    { 
                        _rollTextThread = ThreadUtils.StartBackgroundThread(RollLongTextThreadMethod); 
                    } 
                    else 
                    { 
                        _rollTextThread = ThreadUtils.StartBackgroundThread(RollShortTextThreadMethod); 
                    } 
                } 
                State = MachineState.Running; 
            } 
        } 
        public void Stop() 
        { 
            lock (s_syncRoot) 
            { 
                if (_disposed) 
                    return; 
                StopRollTextThread(); 
                State = MachineState.Stopped; 
            } 
        } 
        private void RollShortTextThreadMethod() 
        { 
            try 
            { 
                while (!RaiseNeedSetText(_rolledText)) 
                { 
                    if (_stopRollTextEvent.WaitOne(UpdateTextDelay, false)) 
                        return; 
                } 
            } 
            catch (ThreadAbortException) 
            { 
            } 
        } 
        private void RollLongTextThreadMethod() 
        { 
            try 
            { 
                string textPart; 
                GetNextTextPart(out textPart); 
                RaiseNeedSetText(textPart); 
                Thread.Sleep(StartDelay); 
                while (true) 
                { 
                    var end = GetNextTextPart(out textPart); 
                    RaiseNeedSetText(textPart); 
                    if (end) 
                        Thread.Sleep(EndDelay); 
                    if (_stopRollTextEvent.WaitOne(UpdateTextDelay, false)) 
                        return; 
                } 
            } 
            catch (ThreadAbortException) 
            { 
            } 
        } 
        private void SetRealRolledText() 
        { 
            if (_rolledText.Length == 0) 
            { 
                _realRolledText = ""; 
                return; 
            } 
            var sb = new StringBuilder(_rolledText); 
            if (_rolledText.Length <= _maxTextLength) 
                sb.Append(' ', _maxTextLength - _rolledText.Length); 
            else 
                sb.Append(' ', ROLL_LOOP_DELIM_SPACE_COUNT); 
            _realRolledText = sb.ToString(); 
        } 
        private int _currentPosition = -1; 
        private void ResetCounters() 
        { 
            _currentPosition = -1; 
        } 
        private bool GetNextTextPart(out string textPart) 
        { 
            if (++_currentPosition == _realRolledText.Length) 
                _currentPosition = 0; 
            if (_currentPosition + _maxTextLength <= _realRolledText.Length) 
            { 
                textPart = _realRolledText.Substring(_currentPosition, _maxTextLength); 
            } 
            else 
            { 
                textPart = _realRolledText.Substring(_currentPosition, _realRolledText.Length - _currentPosition); 
                textPart += _realRolledText.Substring(0, _maxTextLength - textPart.Length); 
            } 
            return _currentPosition + _maxTextLength == _realRolledText.Length; 
        } 
        private void StopRollTextThread() 
        { 
            if (_rollTextThread != null) 
            { 
                _stopRollTextEvent.Set(); 
                if (!_rollTextThread.Join(100)) 
                    _rollTextThread.SafeAbort(); 
                _rollTextThread = null; 
            } 
        } 
        #region IDisposable Members 
        public void Dispose() 
        { 
            lock (s_syncRoot) 
            { 
                StopRollTextThread(); 
                _disposed = true; 
            } 
            GC.SuppressFinalize(this); 
        } 
        #endregion 
    } 
}
