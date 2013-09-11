using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Utils.Threading; 

using Croc.Core; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Дескриптор ожидания нажатия нужной клавиши или комбинации клавиш 

    /// </summary> 

    /// <remarks> 

    /// При создании объекта происходит подписывание на событие клавиатуры,  

    /// а отписывание будет выполняться только в Dispose, поэтому использовать  

    /// данный класс рекомендуется с применением конструкции:  

    /// using (var e = new SubscribingEventWaitHandle(...)) { ... } 

    /// или принудительно вызывать метод Dispose 

    /// </remarks> 

    public class KeyPressedWaitHandle : EventWaitHandleEx 

    { 

        /// <summary> 

        /// Дескриптор ожидаемых нажатий клавиш 

        /// </summary> 

        public readonly KeyPressingWaitDescriptor WaitDescriptor; 

        /// <summary> 

        /// Аргументы нажатой клавиши 

        /// </summary> 

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

            Keyboard.KeyPressed += new EventHandler<KeyEventArgs>(Keyboard_KeyPressed); 

        } 

 

 

        /// <summary> 

        /// Обработка нажатия клавиши 

        /// </summary> 


        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void Keyboard_KeyPressed(object sender, KeyEventArgs e) 

        { 

            if (!WaitDescriptor.IsMatch(e)) 

                return; 

 

 

            PressedKeyArgs = e; 

            base.Set(); 

        } 

 

 

        public new void Reset() 

        { 

            if (base.Reset()) 

                PressedKeyArgs = null; 

        } 

 

 

        protected override void Dispose(bool explicitDisposing) 

        { 

            Keyboard.KeyPressed -= new EventHandler<KeyEventArgs>(Keyboard_KeyPressed); 

            base.Dispose(explicitDisposing); 

        } 

 

 

        #region Статические свойства 

 

 

        /// <summary> 

        /// Объект синхронизации 

        /// </summary> 

        private static object s_sync = new object(); 

 

 

        private static IKeyboard _keyboard; 

        /// <summary> 

        /// Менеджер клавиатуры 

        /// </summary> 

        private static IKeyboard Keyboard 

        { 

            get 

            { 

                if (_keyboard == null) 

                    lock (s_sync) 

                        if (_keyboard == null) 

                        { 

                            _keyboard = (IKeyboard)CoreApplication.Instance.GetSubsystem<UnionKeyboard>(); 

                            if (_keyboard == null) 


                                _keyboard = CoreApplication.Instance.GetSubsystemOrThrow<IKeyboard>(); 

                        } 

 

 

                return _keyboard; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _digitOrDeletePressed; 

        /// <summary> 

        /// Нажата любая цифровая клавиша или клавиша Delete 

        /// </summary> 

        public static KeyPressedWaitHandle DigitOrDeletePressed 

        { 

            get 

            { 

                if (_digitOrDeletePressed == null) 

                    _digitOrDeletePressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(new[] { KeyType.Digit, KeyType.Delete }, false)); 

 

 

                return _digitOrDeletePressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _digitPressed; 

        /// <summary> 

        /// Нажата любая цифровая клавиша 

        /// </summary> 

        public static KeyPressedWaitHandle DigitPressed 

        { 

            get 

            { 

                if (_digitPressed == null) 

                    _digitPressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(new[] { KeyType.Digit }, false)); 

 

 

                return _digitPressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _deletePressed; 

        /// <summary> 

        /// Нажата клавиша Delete 

        /// </summary> 

        public static KeyPressedWaitHandle DeletePressed 


        { 

            get 

            { 

                if (_deletePressed == null) 

                    _deletePressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(new[] { KeyType.Delete }, false)); 

 

 

                return _deletePressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _yesAndNoAtOncePressed; 

        /// <summary> 

        /// Нажаты ДА и НЕТ одновременно 

        /// </summary> 

        public static KeyPressedWaitHandle YesAndNoAtOncePressed 

        { 

            get 

            { 

                if (_yesAndNoAtOncePressed == null) 

                    _yesAndNoAtOncePressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(new[] { KeyType.Yes, KeyType.No }, true)); 

 

 

                return _yesAndNoAtOncePressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _yesOrNoPressed; 

        /// <summary> 

        /// Нажата ДА или НЕТ 

        /// </summary> 

        public static KeyPressedWaitHandle YesOrNoPressed 

        { 

            get 

            { 

                if (_yesOrNoPressed == null) 

                    _yesOrNoPressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(new[] { KeyType.Yes, KeyType.No }, false)); 

 

 

                return _yesOrNoPressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _yesOrNoOrBackPressed; 


        /// <summary> 

        /// Нажата ДА или НЕТ или ВОЗВРАТ 

        /// </summary> 

        public static KeyPressedWaitHandle YesOrNoOrBackPressed 

        { 

            get 

            { 

                if (_yesOrNoOrBackPressed == null) 

                    _yesOrNoOrBackPressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(new[] { KeyType.Yes, KeyType.No, KeyType.GoBack }, false)); 

 

 

                return _yesOrNoOrBackPressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _helpPressed; 

        /// <summary> 

        /// Нажата ПОМОЩЬ 

        /// </summary> 

        public static KeyPressedWaitHandle HelpPressed 

        { 

            get 

            { 

                if (_helpPressed == null) 

                    _helpPressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(KeyType.Help)); 

 

 

                return _helpPressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _yesPressed; 

        /// <summary> 

        /// Нажата ДА 

        /// </summary> 

        public static KeyPressedWaitHandle YesPressed 

        { 

            get 

            { 

                if (_yesPressed == null) 

                    _yesPressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(KeyType.Yes)); 

 

 

                return _yesPressed; 

            } 


        } 

 

 

        private static KeyPressedWaitHandle _noPressed; 

        /// <summary> 

        /// Нажата НЕТ 

        /// </summary> 

        public static KeyPressedWaitHandle NoPressed 

        { 

            get 

            { 

                if (_noPressed == null) 

                    _noPressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(KeyType.No)); 

 

 

                return _noPressed; 

            } 

        } 

 

 

        private static KeyPressedWaitHandle _goBackPressed; 

        /// <summary> 

        /// Нажата ВОЗВРАТ 

        /// </summary> 

        public static KeyPressedWaitHandle GoBackPressed 

        { 

            get 

            { 

                if (_goBackPressed == null) 

                    _goBackPressed = new KeyPressedWaitHandle( 

                        new KeyPressingWaitDescriptor(KeyType.GoBack)); 

 

 

                return _goBackPressed; 

            } 

        } 

 

 

        #endregion 

    } 

}


