using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Threading; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Core; 

using Croc.Core.Diagnostics; 

using Croc.Core.Configuration; 

using Croc.Bpc.Keyboard.Config; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Базовый класс подсистемы управления клавиатурой 

    /// </summary> 

    [SubsystemConfigurationElementTypeAttribute(typeof(KeyboardManagerConfig))] 

    public abstract class BaseKeyboard : Subsystem, IKeyboard 

	{ 

        /// <summary> 

        /// Словарь клавиш [код клавиши, настройка] 

        /// </summary> 

        private Dictionary<int, KeyConfig> _keys; 

        /// <summary> 

        /// Тип клавиатуры 

        /// </summary> 

        protected KeyboardType _type; 

        /// <summary> 

        /// Поток обработки консоли 

        /// </summary> 

        protected Thread _watcher; 

        /// <summary> 

        /// Событие "Нажата клавиша" 

        /// </summary> 

        public event EventHandler<KeyEventArgs> KeyPressed; 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="config"></param> 

        public override void Init(SubsystemConfig config) 

        { 

            var kc = (KeyboardManagerConfig)config; 

 

 

            // получаем из настроек тип клавиатуры 

            _type = kc.Keyboard.Type; 

            Logger.LogInfo(Message.KeyboardType, _type, (int)_type); 

 


 
            // формируем словарь клавиш 

            _keys = new Dictionary<int, KeyConfig>(kc.Keyboard.Keys.Count); 

            foreach (KeyConfig key in kc.Keyboard.Keys) 

                _keys[key.ScanCode] = key; 

 

 

            //Запускаем поток обработки клавиатуры 

            StartWatcher(); 

        } 

 

 

        /// <summary> 

        /// Применение нового конфига 

        /// </summary> 

        /// <param name="newConfig"></param> 

        public override void ApplyNewConfig(SubsystemConfig newConfig) 

        { 

            Init(newConfig); 

        } 

 

 

        /// <summary> 

        /// Запускает поток обработки клавиатуры 

        /// </summary> 

        private void StartWatcher() 

        { 

            lock (this) 

            { 

                if (_watcher != null) 

                    _watcher.Abort(); 

 

 

                _watcher = new Thread(WatcherMethod); 

                _watcher.Start();     

            } 

        } 

 

 

        /// <summary> 

        /// Метод потока асинхронного чтения кодов консоли 

        /// </summary> 

        protected abstract void WatcherMethod(); 

 

 

        /// <summary> 

        /// Обработка события получения новых данных 

        /// </summary> 

        /// <param name="scanCode"></param> 

        /// <param name="timeStamp"></param> 


        protected void OnNewDataReady(int scanCode, int timeStamp) 

        { 

            var handler = KeyPressed; 

            if (handler == null || _disposed) 

                return; 

 

 

            if (_keys.ContainsKey(scanCode)) 

            { 

                var key = _keys[scanCode]; 

 

 

                Logger.LogInfo(Message.KeyboardKeyPressed, scanCode, key.Type, key.Value, timeStamp); 

                handler(this, new KeyEventArgs(scanCode, key.Type, key.Value, timeStamp)); 

            } 

            else 

            { 

                Logger.LogInfo(Message.KeyboardKeyPressed, scanCode, KeyType.Unknown, 0, timeStamp); 

                handler(this, new KeyEventArgs(scanCode, KeyType.Unknown, 0, timeStamp)); 

            } 

        } 

    } 

}


