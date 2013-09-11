using Croc.Core; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Keyboard 
{ 
    public class KeyPressedWaitHandle : EventWaitHandleEx 
    { 
        private readonly object _sync = new object(); 
        public readonly KeyPressingWaitDescriptor WaitDescriptor; 
        public KeyEventArgs PressedKeyArgs 
        { 
            get; 
            private set; 
        } 
        public KeyPressedWaitHandle(KeyPressingWaitDescriptor waitDescriptor) 
            : base(false, false) 
        { 
            CodeContract.Requires(waitDescriptor != null); 
            WaitDescriptor = waitDescriptor; 
            PressedKeyArgs = KeyEventArgs.Empty; 
            Keyboard.KeyPressed += Keyboard_KeyPressed; 
        } 
        private void Keyboard_KeyPressed(object sender, KeyEventArgs e) 
        { 
            lock (_sync) 
            { 
                if (!WaitDescriptor.IsMatch(e)) 
                    return; 
                PressedKeyArgs = e; 
                Set(); 
            } 
        } 
        public new void Reset() 
        { 
            lock (_sync) 
            { 
                if (base.Reset()) 
                    PressedKeyArgs = KeyEventArgs.Empty; 
            } 
        } 
        protected override void Dispose(bool explicitDisposing) 
        { 
            Keyboard.KeyPressed -= Keyboard_KeyPressed; 
            base.Dispose(explicitDisposing); 
        } 
        #region Статические свойства 
        private static readonly object s_sync = new object(); 
        private static IKeyboardManager s_keyboard; 
        private static IKeyboardManager Keyboard 
        { 
            get 
            { 
                if (s_keyboard == null) 
                    lock (s_sync) 
                        if (s_keyboard == null) 
                            s_keyboard = CoreApplication.Instance.GetSubsystem<UnionKeyboard>() ?? 
                                         CoreApplication.Instance.GetSubsystemOrThrow<IKeyboardManager>(); 
                return s_keyboard; 
            } 
        } 
        private static KeyPressedWaitHandle s_digitOrDeletePressed; 
        public static KeyPressedWaitHandle DigitOrDeletePressed 
        { 
            get 
            { 
                if (s_digitOrDeletePressed == null) 
                    s_digitOrDeletePressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(new[] { KeyType.Digit, KeyType.Delete }, false)); 
                return s_digitOrDeletePressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_digitPressed; 
        public static KeyPressedWaitHandle DigitPressed 
        { 
            get 
            { 
                if (s_digitPressed == null) 
                    s_digitPressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(new[] { KeyType.Digit }, false)); 
                return s_digitPressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_deletePressed; 
        public static KeyPressedWaitHandle DeletePressed 
        { 
            get 
            { 
                if (s_deletePressed == null) 
                    s_deletePressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(new[] { KeyType.Delete }, false)); 
                return s_deletePressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_yesAndNoAtOncePressed; 
        public static KeyPressedWaitHandle YesAndNoAtOncePressed 
        { 
            get 
            { 
                if (s_yesAndNoAtOncePressed == null) 
                    s_yesAndNoAtOncePressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(new[] { KeyType.Yes, KeyType.No }, true)); 
                return s_yesAndNoAtOncePressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_helpAndNoAtOncePressed; 
        public static KeyPressedWaitHandle HelpAndNoAtOncePressed 
        { 
            get 
            { 
                if (s_helpAndNoAtOncePressed == null) 
                    s_helpAndNoAtOncePressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(new[] { KeyType.Help, KeyType.No }, true)); 
                return s_helpAndNoAtOncePressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_yesOrNoPressed; 
        public static KeyPressedWaitHandle YesOrNoPressed 
        { 
            get 
            { 
                if (s_yesOrNoPressed == null) 
                    s_yesOrNoPressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(new[] { KeyType.Yes, KeyType.No }, false)); 
                return s_yesOrNoPressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_yesOrNoOrBackPressed; 
        public static KeyPressedWaitHandle YesOrNoOrBackPressed 
        { 
            get 
            { 
                if (s_yesOrNoOrBackPressed == null) 
                    s_yesOrNoOrBackPressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(new[] { KeyType.Yes, KeyType.No, KeyType.GoBack }, false)); 
                return s_yesOrNoOrBackPressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_helpPressed; 
        public static KeyPressedWaitHandle HelpPressed 
        { 
            get 
            { 
                if (s_helpPressed == null) 
                    s_helpPressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(KeyType.Help)); 
                return s_helpPressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_yesPressed; 
        public static KeyPressedWaitHandle YesPressed 
        { 
            get 
            { 
                if (s_yesPressed == null) 
                    s_yesPressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(KeyType.Yes)); 
                return s_yesPressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_noPressed; 
        public static KeyPressedWaitHandle NoPressed 
        { 
            get 
            { 
                if (s_noPressed == null) 
                    s_noPressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(KeyType.No)); 
                return s_noPressed; 
            } 
        } 
        private static KeyPressedWaitHandle s_goBackPressed; 
        public static KeyPressedWaitHandle GoBackPressed 
        { 
            get 
            { 
                if (s_goBackPressed == null) 
                    s_goBackPressed = new KeyPressedWaitHandle( 
                        new KeyPressingWaitDescriptor(KeyType.GoBack)); 
                return s_goBackPressed; 
            } 
        } 
        #endregion 
    } 
}
