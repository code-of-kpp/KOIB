using System; 
using System.Threading; 
using Croc.Core; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Keyboard 
{ 
    public class RepetableKeyPressWaitHandle : EventWaitHandleEx 
    { 
        public readonly KeyPressingWaitDescriptor WaitDescriptor; 
        public KeyEventArgs PressedKeyArgs 
        { 
            get; 
            private set; 
        } 
        public readonly int PressTimes; 
        private volatile int _currentPressCount; 
        private static readonly object s_counterSync = new object(); 
        private readonly TimeSpan _maxKeysPressingInterval = TimeSpan.FromMilliseconds(500); 
        private DateTime _lastPressTime = DateTime.MaxValue; 
        public RepetableKeyPressWaitHandle(KeyPressingWaitDescriptor waitDescriptor, int pressRepeats) 
            : base(false, false) 
        { 
            CodeContract.Requires(waitDescriptor != null); 
            CodeContract.Requires(pressRepeats > 0); 
            PressTimes = pressRepeats; 
            WaitDescriptor = waitDescriptor; 
            Keyboard.KeyPressed += Keyboard_KeyPressed; 
        } 
        private void Keyboard_KeyPressed(object sender, KeyEventArgs e) 
        { 
            if (!WaitDescriptor.IsMatch(e)) 
                return; 


            var time = DateTime.Now; 
            if (time.TimeOfDay - _lastPressTime.TimeOfDay > _maxKeysPressingInterval) 
                _currentPressCount = 0; 
            _lastPressTime = time; 
            lock (s_counterSync) 
            { 
                _currentPressCount++; 
            } 
            PressedKeyArgs = e; 
            if (_currentPressCount == PressTimes) 
            { 
                ThreadUtils.StartBackgroundThread(WaitForSet); 
            } 
        } 
        private void WaitForSet() 
        { 
            Thread.Sleep(_maxKeysPressingInterval); 
            lock(s_counterSync) 
            { 
                if (_currentPressCount == PressTimes) 
                    Set(); 
            } 
        } 
        private static readonly object s_sync = new object(); 
        private static IKeyboardManager s_keyboard; 
        private static IKeyboardManager Keyboard 
        { 
            get 
            { 
                if (s_keyboard == null) 
                    lock (s_sync) 
                        if (s_keyboard == null) 
                        { 
                            s_keyboard = CoreApplication.Instance.GetSubsystem<UnionKeyboard>(); 
                            if (s_keyboard == null) 
                                s_keyboard = CoreApplication.Instance.GetSubsystemOrThrow<IKeyboardManager>(); 
                        } 
                return s_keyboard; 
            } 
        } 
    } 
}
